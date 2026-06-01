namespace MilestoneTracker.API.Extensions;

using System.Threading.RateLimiting;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

public static class RateLimiterExtension
{
    public static IServiceCollection AddLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(PoliciesConstants.RateLimiterPolicies.TelegramBotPolicy, context =>
            {
                if (context.Items.TryGetValue(RequestItemsConstants.UserIdEnrichment, out var chatIdObj)
                    && chatIdObj is long chatId)
                {
                    return RateLimitPartition.GetFixedWindowLimiter(chatId.ToString(), _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }

                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey: ip, factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.OnRejected = async (rejectedContext, cancellationToken) =>
            {
                var httpContext = rejectedContext.HttpContext;

                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                httpContext.Response.ContentType = "text/plain";

                if (httpContext.Items.TryGetValue(RequestItemsConstants.UserIdEnrichment, out var chatIdObj)
                    && chatIdObj is long chatId)
                {
                    var servicesProvider = httpContext.RequestServices;

                    var logger = servicesProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiter");
                    logger.LogWarning("Rate limit exceeded for ChatId: {ChatId}", chatId);

                    var cache = servicesProvider.GetRequiredService<IMemoryCache>();
                    var cacheKey = CacheKeys.UserRateWarningPrefix + chatId;

                    if (!cache.TryGetValue(cacheKey, out _))
                    {
                        cache.Set(cacheKey, true, TimeSpan.FromMinutes(1));

                        var messageService = servicesProvider.GetRequiredService<ITelegramMessageService>();
                        try
                        {
                            await messageService.SendTextMessageAsync(
                                chatId,
                                "Слишком много сообщений! Пожалуйста, подождите немного прежде чем отправить следующее.",
                                ct: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to send rate limit warning to Telegram chat {ChatId}", chatId);
                        }
                    }
                }

                await httpContext.Response.WriteAsync("Ignored due to rate limit", cancellationToken);
            };
        });

        return services;
    }
}