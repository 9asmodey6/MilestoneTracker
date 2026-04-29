namespace MilestoneTracker.Infrastructure.Persistence.Configurations.ChildAccessToken;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChildAccessTokenConfiguration : IEntityTypeConfiguration<ChildAccessToken>
{
    public void Configure(EntityTypeBuilder<ChildAccessToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.HasOne(t => t.Child)
            .WithMany()
            .HasForeignKey(t => t.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.UsedByParent)
            .WithMany()
            .HasForeignKey(t => t.UsedByParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
