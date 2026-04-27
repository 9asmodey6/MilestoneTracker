namespace MilestoneTracker.API.Extensions;

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MilestoneTracker.Infrastructure.Persistence;

public static class MigrationsExtension
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<AppDbContext>>();
        var context = services.GetRequiredService<AppDbContext>();

        logger.LogInformation("Starting database migrations and seeding for {ContextName}...", nameof(AppDbContext));

        var sw = Stopwatch.StartNew();

        try
        {
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                logger.LogInformation(
                    "Found {Count} pending migrations: {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations));

                context.Database.Migrate();

                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations found. Database is up to date.");
            }

            sw.Stop();
            logger.LogInformation(
                "Database initialization (migration & seeding) completed in {ElapsedMs}ms",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogCritical(ex,
                "An error occurred during database migration after {ElapsedMs}ms. Process terminated.",
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}