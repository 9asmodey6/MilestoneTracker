namespace MilestoneTracker.Infrastructure.Persistence.Configurations.MilestoneMedia;

using Domain.Entities.Milestones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MilestoneMediaConfiguration : IEntityTypeConfiguration<MilestoneMedia>
{
    public void Configure(EntityTypeBuilder<MilestoneMedia> builder)
    {
        builder.HasKey(mm => mm.Id);

        builder.Property(mm => mm.FileId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(mm => mm.Caption)
            .HasMaxLength(1000);
        
        builder.Property(mm => mm.Type)
            .HasConversion<int>();

        builder.HasIndex(mm => mm.MilestoneId);
    }
}