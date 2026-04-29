namespace MilestoneTracker.Infrastructure.Persistence.Configurations.Child;

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
            .HasMaxLength(100);


        builder.HasIndex(t => t.Token)
            .IsUnique();
        builder.Property(t => t.CreatedAt)
            .IsRequired();
        builder.Property(t => t.ExpiresAt)
            .IsRequired();


        builder.HasOne(t => t.Child)
            .WithMany()
            .HasForeignKey(t => t.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.UsedByParent)
            .WithMany()
            .HasForeignKey(t => t.UsedByParentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.ChildId);
        builder.HasIndex(t => t.CreatorId);
        builder.HasIndex(t => t.IsUsed);
    }
}