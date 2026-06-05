using Goal2026API.Api.Data;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Entities;
using Goal2026API.Services;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Api.Services;

public interface IUserSyncService
{
    Task<UserDto> SyncUserAsync(
    FirebaseTokenInfo tokenInfo,
    SyncUserRequestDto? dto,
    CancellationToken cancellationToken = default);

    Task<UserDto?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateProfileAsync(string firebaseUid, UpdateMyProfileDto dto, CancellationToken cancellationToken = default);
}

public sealed class UserSyncService : IUserSyncService
{
    private static readonly HashSet<string> AllowedPhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly AppDbContext _dbContext;
    private readonly IStorageService _storageService;

    public UserSyncService(
        AppDbContext dbContext,
        IStorageService storageService)
    {
        _dbContext = dbContext;
        _storageService = storageService;
    }

    public async Task<UserDto> SyncUserAsync(
    FirebaseTokenInfo tokenInfo,
    SyncUserRequestDto? dto,
    CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users
            .SingleOrDefaultAsync(x => x.FirebaseUid == tokenInfo.Uid, cancellationToken);

        if (existingUser is null)
        {
            var emailAlreadyUsedByAnotherUid = await _dbContext.Users
                .AnyAsync(x => x.Email == tokenInfo.Email && x.FirebaseUid != tokenInfo.Uid, cancellationToken);

            if (emailAlreadyUsedByAnotherUid)
            {
                throw new InvalidOperationException("A user with the same email already exists under a different Firebase UID.");
            }

            var newUser = new User
            {
                FirebaseUid = tokenInfo.Uid,
                Email = tokenInfo.Email,
                Nickname = null,
                PhotoKey = null,
                PhotoContentType = null,
                PreferredLanguage = NormalizePreferredLanguage(dto?.PreferredLanguage),
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapAsync(newUser, cancellationToken);
        }

        if (!string.Equals(existingUser.Email, tokenInfo.Email, StringComparison.OrdinalIgnoreCase))
        {

            var emailAlreadyUsedByAnotherUid = await _dbContext.Users
                .AnyAsync(x => x.Email == tokenInfo.Email && x.FirebaseUid != tokenInfo.Uid, cancellationToken);

            if (emailAlreadyUsedByAnotherUid)
            {
                throw new InvalidOperationException("A user with the same email already exists under a different Firebase UID.");
            }

            existingUser.Email = tokenInfo.Email;
        }


        if (!string.IsNullOrWhiteSpace(dto?.PreferredLanguage))
        {
            existingUser.PreferredLanguage = NormalizePreferredLanguage(dto.PreferredLanguage);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapAsync(existingUser, cancellationToken);

    }

    private static string NormalizePreferredLanguage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "en";

        var lang = value.Trim().ToLowerInvariant();

        if (lang.StartsWith("es"))
            return "es";

        if (lang.StartsWith("en"))
            return "en";

        return "en";
    }

    public async Task<UserDto?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return await MapAsync(user, cancellationToken);
    }

    public async Task<UserDto?> UpdateProfileAsync(
        string firebaseUid,
        UpdateMyProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.Nickname = NormalizeNickname(dto.Nickname);
        user.PhotoKey = NormalizePhotoKey(dto.PhotoKey);
        user.PhotoContentType = NormalizePhotoContentType(dto.PhotoContentType, user.PhotoKey);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await MapAsync(user, cancellationToken);
    }

    private static string? NormalizeNickname(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > 256)
        {
            throw new ArgumentException("Nickname cannot exceed 256 characters.");
        }

        return trimmed;
    }

    private static string? NormalizePhotoKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > 500)
        {
            throw new ArgumentException("PhotoKey cannot exceed 500 characters.");
        }

        return trimmed;
    }

    private static string? NormalizePhotoContentType(string? contentType, string? photoKey)
    {
        if (string.IsNullOrWhiteSpace(photoKey))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("PhotoContentType is required when PhotoKey is provided.");
        }

        var trimmed = contentType.Trim();

        if (trimmed.Length > 100)
        {
            throw new ArgumentException("PhotoContentType cannot exceed 100 characters.");
        }

        if (!AllowedPhotoContentTypes.Contains(trimmed))
        {
            throw new ArgumentException("PhotoContentType is not allowed.");
        }

        return trimmed;
    }

    private async Task<UserDto> MapAsync(User user, CancellationToken cancellationToken)
    {
        string? photoUrl = null;

        if (!string.IsNullOrWhiteSpace(user.PhotoKey))
        {
            var readUrl = await _storageService.CreateReadUrlAsync(user.PhotoKey, cancellationToken);
            photoUrl = readUrl.Url;
        }

        return new UserDto
        {
            Id = user.Id,
            FirebaseUid = user.FirebaseUid,
            Email = user.Email,
            Nickname = user.Nickname,
            PhotoKey = user.PhotoKey,
            PhotoContentType = user.PhotoContentType,
            PhotoUrl = photoUrl,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}