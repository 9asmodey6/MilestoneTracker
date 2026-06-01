namespace MilestoneTracker.API.Middlewares;

using System.Text;
using System.Text.Json;
using Application.Common.Constants;

public class UserIdEnrichmentMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Path.StartsWithSegments(EndpointConstants.TelegramBotRoute)
            && context.Request.Method == HttpMethods.Post)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            try
            {
                using var jsonDoc = JsonDocument.Parse(body);
                if (jsonDoc.RootElement
                        .TryGetProperty(RequestItemsConstants.Message, out var message) &&
                    message
                        .TryGetProperty(RequestItemsConstants.Chat, out var chat) &&
                    chat
                        .TryGetProperty(RequestItemsConstants.Id, out var chatId))
                {
                    context.Items[RequestItemsConstants.UserIdEnrichment] = chatId.GetInt64();
                }
            }
            catch (JsonException)
            {
            }

            context.Request.Body.Position = 0;
        }

        await next(context);
    }
}