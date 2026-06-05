using Goal2026API.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Goal2026API.Api.Data.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.MatchNumber)
            .IsRequired();

        builder.Property(x => x.HomeTeam)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AwayTeam)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.HomeFlag)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AwayFlag)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Stadium)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MatchDateUtc)
            .IsRequired();

        builder.Property(x => x.StageCode)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsFinished)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();
    }
}