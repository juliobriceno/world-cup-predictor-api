using Goal2026API.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Goal2026API.Api.Data.Configurations;

public sealed class UserMatchPredictionConfiguration : IEntityTypeConfiguration<UserMatchPrediction>
{
    public void Configure(EntityTypeBuilder<UserMatchPrediction> builder)
    {
        builder.ToTable("UserMatchPredictions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MatchId)
            .IsRequired();

        builder.Property(x => x.PredictedHomeGoals);

        builder.Property(x => x.PredictedAwayGoals);

        builder.Property(x => x.HasPrediction)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.Match)
            .WithMany(x => x.UserMatchPredictions)
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Add later in SQL or EF if you want:
        // builder.HasIndex(x => new { x.UserId, x.MatchId }).IsUnique();
    }
}