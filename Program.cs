using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using FirebaseAdmin;
using Goal2026API.Api.Auth;
using Goal2026API.Api.Data;
using Goal2026API.Api.Middleware;
using Goal2026API.Api.Options;
using Goal2026API.Api.Services;
using Goal2026API.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<FirebaseOptions>(
    builder.Configuration.GetSection(FirebaseOptions.SectionName));

builder.Services.Configure<FirebaseMagicLinkOptions>(
    builder.Configuration.GetSection(FirebaseMagicLinkOptions.SectionName));

builder.Services.Configure<SendGridOptions>(
    builder.Configuration.GetSection(SendGridOptions.SectionName));

builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));

builder.Services.Configure<S3StorageOptions>(
    builder.Configuration.GetSection(S3StorageOptions.SectionName));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ApiNotificationService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Goal2026API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {firebase_id_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var s3Options = builder.Configuration
        .GetSection(S3StorageOptions.SectionName)
        .Get<S3StorageOptions>()
        ?? throw new InvalidOperationException("S3Storage configuration is missing.");

    if (string.IsNullOrWhiteSpace(s3Options.BucketName))
    {
        throw new InvalidOperationException("S3Storage:BucketName is missing.");
    }

    if (string.IsNullOrWhiteSpace(s3Options.Region))
    {
        throw new InvalidOperationException("S3Storage:Region is missing.");
    }

    var profileName = string.IsNullOrWhiteSpace(s3Options.AwsProfile)
        ? "default"
        : s3Options.AwsProfile.Trim();

    var region = RegionEndpoint.GetBySystemName(s3Options.Region);

    var accessKey = builder.Configuration["AWS:AccessKeyId"];
    var secretKey = builder.Configuration["AWS:SecretAccessKey"];

    AWSCredentials awsCredentials;

    if (!string.IsNullOrWhiteSpace(accessKey) &&
        !string.IsNullOrWhiteSpace(secretKey))
    {
        awsCredentials = new BasicAWSCredentials(accessKey, secretKey);
    }
    else
    {
        var chain = new CredentialProfileStoreChain();

        if (!chain.TryGetAWSCredentials(profileName, out awsCredentials))
        {
            throw new InvalidOperationException(
                $"AWS credentials not found for profile '{profileName}'.");
        }
    }

    return new AmazonS3Client(awsCredentials, region);
});

builder.Services.AddScoped<IFirebaseTokenService, FirebaseTokenService>();
builder.Services.AddScoped<IUserSyncService, UserSyncService>();
builder.Services.AddScoped<IUserPredictionService, UserPredictionService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IStorageService, S3StorageService>();
builder.Services.AddScoped<IUserSimulationService, UserSimulationService>();

builder.Services.AddScoped<IFirebaseMagicLinkService, FirebaseMagicLinkService>();
builder.Services.AddScoped<ITransactionalEmailService, SendGridTransactionalEmailService>();
builder.Services.AddScoped<IAuthMagicLinkService, AuthMagicLinkService>();

builder.Services.AddScoped<IGroupInvitationService, GroupInvitationService>();
builder.Services.AddScoped<IGroupInvitationEmailService, GroupInvitationEmailService>();
builder.Services.AddSingleton<IInvitationTokenService, InvitationTokenService>();

builder.Services.AddScoped<IGroupStandingsService, GroupStandingsService>();

builder.Services.AddHttpClient<IRecaptchaService, RecaptchaService>();

builder.Services.AddAuthentication("Firebase")
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>("Firebase", _ => { });

builder.Services.AddAuthorization();

var frontendBaseUrl = builder.Configuration["Frontend:BaseUrl"]
    ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://goal2026.net",
                "https://www.goal2026.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 64 * 1024; // 64 KB
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Too many requests. Please wait a moment and try again."
        }, cancellationToken);
    };

    options.AddPolicy("auth-sync", httpContext =>
    {
        var ipAddress =
            httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    //Global protection: 100 requests per minute per IP.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ipAddress =
            httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    // Magic Link / Auth endpoints.
    options.AddPolicy("magic-link", httpContext =>
    {
        var ipAddress =
            httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    // Heavy DB calculations: standings, dashboards, simulations.
    options.AddPolicy("heavy-read", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    // Group creation / group update endpoints.
    options.AddPolicy("group-write", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    // Email invitations: very sensitive.
    options.AddPolicy("invite-emails", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    // Small CSV download.
    options.AddFixedWindowLimiter("csv-download", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.AutoReplenishment = true;
    });


});

var firebaseOptions = builder.Configuration
    .GetSection(FirebaseOptions.SectionName)
    .Get<FirebaseOptions>()
    ?? throw new InvalidOperationException("Firebase configuration is missing.");

if (FirebaseApp.DefaultInstance is null)
{
    GoogleCredential credential;

    if (!string.IsNullOrWhiteSpace(firebaseOptions.ServiceAccountJson))
    {
        credential = GoogleCredential.FromJson(firebaseOptions.ServiceAccountJson);
    }
    else if (!string.IsNullOrWhiteSpace(firebaseOptions.ServiceAccountPath))
    {
        credential = GoogleCredential.FromFile(firebaseOptions.ServiceAccountPath);
    }
    else
    {
        throw new InvalidOperationException(
            "Firebase:ServiceAccountJson or Firebase:ServiceAccountPath is required.");
    }

    FirebaseApp.Create(new AppOptions
    {
        Credential = credential
    });
}

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("FrontendPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();