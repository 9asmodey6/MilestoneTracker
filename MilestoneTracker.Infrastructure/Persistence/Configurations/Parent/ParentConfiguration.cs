namespace MilestoneTracker.Infrastructure.Persistence.Configurations.Parent;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ChatId)
            .IsRequired();

        builder.HasIndex(p => p.ChatId)
            .IsUnique();

        builder.HasMany(p => p.Children)
            .WithMany(c => c.Parents);
    }
}