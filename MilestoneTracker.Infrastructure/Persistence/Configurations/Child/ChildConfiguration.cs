namespace MilestoneTracker.Infrastructure.Persistence.Configurations.Child;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ChildConfiguration : IEntityTypeConfiguration<Child>
{
    public void Configure(EntityTypeBuilder<Child> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PhotoFileId)
            .HasMaxLength(255);
        
        builder.HasMany(c => c.Milestones)
            .WithOne(m => m.Child)
            .HasForeignKey(m => m.ChildId);

        builder.HasMany(c => c.Parents)
            .WithMany(p => p.Children);
    }
}