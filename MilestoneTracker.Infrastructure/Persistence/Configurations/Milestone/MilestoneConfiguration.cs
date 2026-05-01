namespace MilestoneTracker.Infrastructure.Persistence.Configurations.Milestone;

using Domain.Entities.Milestones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasQueryFilter(m => !m.IsDeleted);
        
        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.HasOne(m => m.Child)
            .WithMany(c => c.Milestones)
            .HasForeignKey(m => m.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.MediaFiles)
            .WithOne()
            .HasForeignKey(mf => mf.MilestoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.OccurredAt);
        builder.HasIndex(m => m.Category);
    }
}