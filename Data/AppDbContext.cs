using Goal2026API.Api.Data.Entities;
using Goal2026API.Api.Data.Entities;
using Goal2026API.Api.Entities;
using Goal2026API.Entities.FirebasePushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserMatchSimulation> UserMatchSimulations { get; set; }

    public DbSet<User> Users => Set<User>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<UserMatchPrediction> UserMatchPredictions => Set<UserMatchPrediction>();

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupScoringRule> GroupScoringRules => Set<GroupScoringRule>();
    public DbSet<GroupInvitation> GroupInvitations => Set<GroupInvitation>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<GroupUserMatchScore> GroupUserMatchScores => Set<GroupUserMatchScore>();

    public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();

    public DbSet<GroupUserJourneyStandingSnapshot> GroupUserJourneyStandingSnapshots
        => Set<GroupUserJourneyStandingSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<UserDeviceToken>(entity =>
        {
            entity.ToTable("UserDeviceToken");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Token)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.Channel)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Platform)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.DeviceName)
                .HasMaxLength(200);

            entity.Property(x => x.AppVersion)
                .HasMaxLength(50);

            entity.Property(x => x.InvalidReason)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => new { x.UserId, x.Token })
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .HasColumnName("UserId");

            entity.Property(x => x.Type)
                .HasColumnName("EventKey")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasColumnName("Title")
                .HasMaxLength(250);

            entity.Property(x => x.Body)
                .HasColumnName("Body");

            entity.Property(x => x.DataJson)
                .HasColumnName("DataJson");

            entity.Property(x => x.DeduplicationKey)
                .HasColumnName("DeduplicationKey")
                .HasMaxLength(300);

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("CreatedAt");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NotificationDelivery>(entity =>
        {
            entity.ToTable("NotificationDelivery");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NotificationId)
                .HasColumnName("NotificationId");

            entity.Property(x => x.UserId)
                .HasColumnName("UserId");

            entity.Property(x => x.Channel)
                .HasColumnName("Channel")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("Status")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.RetryCount)
                .HasColumnName("RetryCount");

            entity.Property(x => x.MaxRetries)
                .HasColumnName("MaxRetries");

            entity.Property(x => x.ScheduledAtUtc)
                .HasColumnName("ScheduledAt");

            entity.Property(x => x.LockedAtUtc)
                .HasColumnName("LockedAt");

            entity.Property(x => x.LockedBy)
                .HasColumnName("LockedBy")
                .HasMaxLength(100);

            entity.Property(x => x.SentAtUtc)
                .HasColumnName("SentAt");

            entity.Property(x => x.FailedAtUtc)
                .HasColumnName("FailedAt");

            entity.Property(x => x.ErrorMessage)
                .HasColumnName("LastError");

            entity.Property(x => x.ProviderResponse)
                .HasColumnName("ProviderResponse");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("CreatedAt");

            entity.HasOne(x => x.Notification)
                .WithMany(x => x.Deliveries)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupUserJourneyStandingSnapshot>(entity =>
        {
            entity.ToTable("GroupUserJourneyStandingSnapshots");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.GroupId).IsRequired();
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.JourneyNumber).IsRequired();

            entity.Property(x => x.JourneyDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.PointsOfDay).IsRequired();
            entity.Property(x => x.CumulativePoints).IsRequired();
            entity.Property(x => x.PositionInJourney).IsRequired();
            entity.Property(x => x.LastMatchId);
            entity.Property(x => x.UpdatedAtUtc).IsRequired();

            entity.HasIndex(x => new { x.GroupId, x.UserId, x.JourneyDate })
                .IsUnique();

            entity.HasIndex(x => new { x.GroupId, x.JourneyNumber, x.PositionInJourney });

            entity.HasIndex(x => new { x.GroupId, x.UserId });
        });

        var user = modelBuilder.Entity<User>();

        user.ToTable("Users");
        user.HasKey(x => x.Id);

        user.HasIndex(x => x.FirebaseUid).IsUnique();
        user.HasIndex(x => x.Email);

        user.Property(x => x.FirebaseUid).HasMaxLength(128).IsRequired();
        user.Property(x => x.Email).HasMaxLength(256).IsRequired();
        user.Property(x => x.Nickname).HasMaxLength(256);
        user.Property(x => x.PhotoKey).HasMaxLength(500);
        user.Property(x => x.PhotoContentType).HasMaxLength(100);
        user.Property(x => x.CreatedAtUtc).IsRequired();

        var match = modelBuilder.Entity<Match>();

        match.ToTable("Matches");
        match.HasKey(x => x.Id);

        match.Property(x => x.GroupCode).HasMaxLength(10).IsRequired();
        match.Property(x => x.MatchNumber).IsRequired();
        match.Property(x => x.HomeTeam).HasMaxLength(100).IsRequired();
        match.Property(x => x.AwayTeam).HasMaxLength(100).IsRequired();
        match.Property(x => x.HomeFlag).HasMaxLength(50).IsRequired();
        match.Property(x => x.AwayFlag).HasMaxLength(50).IsRequired();
        match.Property(x => x.Stadium).HasMaxLength(150).IsRequired();
        match.Property(x => x.City).HasMaxLength(100).IsRequired();
        match.Property(x => x.MatchDateUtc).IsRequired();
        match.Property(x => x.StageCode).HasMaxLength(30).IsRequired();
        match.Property(x => x.IsActive).IsRequired();
        match.Property(x => x.IsFinished).IsRequired();
        match.Property(x => x.CreatedAtUtc).IsRequired();
        match.Property(x => x.UpdatedAtUtc).IsRequired();

        var userMatchPrediction = modelBuilder.Entity<UserMatchPrediction>();

        userMatchPrediction.ToTable("UserMatchPredictions");
        userMatchPrediction.HasKey(x => x.Id);

        userMatchPrediction.Property(x => x.UserId).IsRequired();
        userMatchPrediction.Property(x => x.MatchId).IsRequired();
        userMatchPrediction.Property(x => x.PredictedHomeGoals);
        userMatchPrediction.Property(x => x.PredictedAwayGoals);
        userMatchPrediction.Property(x => x.HasPrediction).IsRequired();
        userMatchPrediction.Property(x => x.CreatedAtUtc).IsRequired();
        userMatchPrediction.Property(x => x.UpdatedAtUtc).IsRequired();

        userMatchPrediction.HasOne(x => x.Match)
            .WithMany(x => x.UserMatchPredictions)
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        userMatchPrediction.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        userMatchPrediction.HasIndex(x => new { x.UserId, x.MatchId }).IsUnique();
        userMatchPrediction.HasIndex(x => x.UserId);
        userMatchPrediction.HasIndex(x => x.MatchId);

        var group = modelBuilder.Entity<Group>();

        group.ToTable("Groups");
        group.HasKey(x => x.Id);

        group.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        group.Property(x => x.OwnerUserId).IsRequired();
        group.Property(x => x.IsDeleted).IsRequired();
        group.Property(x => x.CreatedAtUtc).IsRequired();
        group.Property(x => x.CreatedByUserId).IsRequired();
        group.Property(x => x.UpdatedAtUtc).IsRequired();
        group.Property(x => x.UpdatedByUserId).IsRequired();

        group.HasIndex(x => x.OwnerUserId);
        group.HasIndex(x => new { x.IsDeleted, x.OwnerUserId });

        group.HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        group.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        group.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        group.Property(x => x.TimeZoneId)
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue("America/New_York");

        var groupMember = modelBuilder.Entity<GroupMember>();

        groupMember.ToTable("GroupMembers");
        groupMember.HasKey(x => new { x.GroupId, x.UserId });

        groupMember.Property(x => x.GroupId).IsRequired();
        groupMember.Property(x => x.UserId).IsRequired();
        groupMember.Property(x => x.JoinedAtUtc).IsRequired();
        groupMember.Property(x => x.IsDeleted).IsRequired();
        groupMember.Property(x => x.CreatedAtUtc).IsRequired();
        groupMember.Property(x => x.CreatedByUserId).IsRequired();
        groupMember.Property(x => x.UpdatedAtUtc).IsRequired();
        groupMember.Property(x => x.UpdatedByUserId).IsRequired();

        groupMember.HasIndex(x => x.UserId);
        groupMember.HasIndex(x => new { x.GroupId, x.IsDeleted });
        groupMember.HasIndex(x => new { x.UserId, x.IsDeleted });

        groupMember.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        groupMember.HasOne(x => x.Group)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        groupMember.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupMember.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupMember.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var groupScoringRule = modelBuilder.Entity<GroupScoringRule>();

        groupScoringRule.ToTable("GroupScoringRules");
        groupScoringRule.HasKey(x => x.GroupId);

        groupScoringRule.Property(x => x.GroupId).IsRequired();

        groupScoringRule.Property(x => x.EnableOutcomeRule).IsRequired();
        groupScoringRule.Property(x => x.OutcomePoints);

        groupScoringRule.Property(x => x.EnableExactScoreRule).IsRequired();
        groupScoringRule.Property(x => x.ExactHomeGoalsPoints);
        groupScoringRule.Property(x => x.ExactAwayGoalsPoints);
        groupScoringRule.Property(x => x.RequireBothExactScores).IsRequired();

        groupScoringRule.Property(x => x.EnableGoalDifferenceRule).IsRequired();
        groupScoringRule.Property(x => x.ClosedMatchPoints);
        groupScoringRule.Property(x => x.ComfortableWinPoints);
        groupScoringRule.Property(x => x.BlowoutPoints);

        groupScoringRule.Property(x => x.CreatedAtUtc).IsRequired();
        groupScoringRule.Property(x => x.CreatedByUserId).IsRequired();
        groupScoringRule.Property(x => x.UpdatedAtUtc).IsRequired();
        groupScoringRule.Property(x => x.UpdatedByUserId).IsRequired();

        groupScoringRule.HasOne(x => x.Group)
            .WithOne(x => x.ScoringRule)
            .HasForeignKey<GroupScoringRule>(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        groupScoringRule.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupScoringRule.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        var groupInvitation = modelBuilder.Entity<GroupInvitation>();

        groupInvitation.ToTable("GroupInvitations");
        groupInvitation.HasKey(x => x.Id);

        groupInvitation.Property(x => x.GroupId).IsRequired();

        groupInvitation.Property(x => x.InvitedUserId).IsRequired(false);

        groupInvitation.Property(x => x.InvitedEmail)
            .HasMaxLength(256)
            .IsRequired();

        groupInvitation.Property(x => x.InvitedEmailNormalized)
            .HasMaxLength(256)
            .IsRequired();

        groupInvitation.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        groupInvitation.Property(x => x.ExpiresAtUtc).IsRequired();

        groupInvitation.Property(x => x.CreatedByUserId).IsRequired();

        groupInvitation.Property(x => x.Status)
            .HasMaxLength(20)
            .IsRequired();

        groupInvitation.Property(x => x.CreatedAtUtc).IsRequired();
        groupInvitation.Property(x => x.RespondedAtUtc);

        groupInvitation.Property(x => x.AcceptedByUserId).IsRequired(false);
        groupInvitation.Property(x => x.DeclinedByUserId).IsRequired(false);

        groupInvitation.Property(x => x.IsDeleted).IsRequired();
        groupInvitation.Property(x => x.UpdatedAtUtc).IsRequired();
        groupInvitation.Property(x => x.UpdatedByUserId).IsRequired();

        groupInvitation.HasIndex(x => x.GroupId);
        groupInvitation.HasIndex(x => x.InvitedUserId);
        groupInvitation.HasIndex(x => x.InvitedEmailNormalized);
        groupInvitation.HasIndex(x => new { x.GroupId, x.InvitedEmailNormalized, x.IsDeleted });
        groupInvitation.HasIndex(x => x.TokenHash).IsUnique();

        groupInvitation.HasOne(x => x.Group)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        groupInvitation.HasOne(x => x.InvitedUser)
            .WithMany()
            .HasForeignKey(x => x.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupInvitation.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupInvitation.HasOne(x => x.AcceptedByUser)
            .WithMany()
            .HasForeignKey(x => x.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupInvitation.HasOne(x => x.DeclinedByUser)
            .WithMany()
            .HasForeignKey(x => x.DeclinedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        groupInvitation.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupUserMatchScore>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.GroupId, x.UserId, x.MatchId })
                .IsUnique();

            entity.HasIndex(x => new { x.GroupId, x.MatchId });
            entity.HasIndex(x => new { x.GroupId, x.UserId });
        });

        var appSetting = modelBuilder.Entity<AppSetting>();

        appSetting.ToTable("AppSettings");
        appSetting.HasKey(x => x.Id);

        appSetting.Property(x => x.Key)
            .HasMaxLength(100)
            .IsRequired();

        appSetting.Property(x => x.Value)
            .HasMaxLength(500)
            .IsRequired();

        appSetting.Property(x => x.CreatedAtUtc).IsRequired();
        appSetting.Property(x => x.UpdatedAtUtc).IsRequired();

        appSetting.HasIndex(x => x.Key).IsUnique();
    }
}