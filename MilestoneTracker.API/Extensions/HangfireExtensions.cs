namespace MilestoneTracker.API.Extensions;

using Hangfire;
using MilestoneTracker.Application.Common.Shared.Interfaces.Services;

public static class HangfireExtensions
{
    public static WebApplication UseHangfireRecurringJobs(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<IChildAccessTokenService>(
            "clear-invalid-tokens",
            service => service.ClearInvalidTokensAsync(CancellationToken.None),
            Cron.Hourly
        );

        return app;
    }
}