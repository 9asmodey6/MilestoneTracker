namespace MilestoneTracker.Infrastructure.Persistence.Configurations.UserState;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserStateConfiguration : IEntityTypeConfiguration<UserState>
{
    public void Configure(EntityTypeBuilder<UserState> builder)
    {
        builder.HasKey(us => us.Id);
        
        builder.HasIndex(us => us.ChatId).IsUnique();
        
        builder.Property(us => us.State)
            .HasConversion<int>();
        
        builder.Property(us => us.StateData)
            .HasColumnType("jsonb");
    }
}