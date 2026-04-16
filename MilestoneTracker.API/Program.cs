using MilestoneTracker.Application;
using MilestoneTracker.Infrastructure;
using MilestoneTracker.Infrastructure.Options;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.ApplyConfigurations(builder.Configuration)
    .AddTelegramBot(builder.Configuration)
    .AddInfractructure(builder.Configuration)
    .AddApplication()
    .AddSerilogLogging();


var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. User: {UserId}";
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
    var options = builder.Configuration.GetSection("Telegram").Get<TelegramOptions>();

    if (!string.IsNullOrEmpty(options?.WebhookUrl))
    {
        try
        {
            await botClient.DeleteWebhook();
            Console.WriteLine("🗑️ Старый webhook удалён");

            await botClient.SetWebhook(
                url: options.WebhookUrl,
                cancellationToken: default);
            
            Console.WriteLine($"Webhook установлен на: {options.WebhookUrl}");
            
            var webhookInfo = await botClient.GetWebhookInfo(
                cancellationToken: default);
            
            Console.WriteLine($"  Webhook Info:");
            Console.WriteLine($"   URL: {webhookInfo.Url}");
            Console.WriteLine($"   Pending Updates: {webhookInfo.PendingUpdateCount}");
            if (!string.IsNullOrEmpty(webhookInfo.LastErrorMessage))
            {
                Console.WriteLine($"    Last Error: {webhookInfo.LastErrorMessage}");
                Console.WriteLine($"   Last Error Date: {webhookInfo.LastErrorDate}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Ошибка установки webhook: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }
    else
    {
        Console.WriteLine(" WebhookUrl не настроен в конфигурации!");
    }
}


app.Run();
