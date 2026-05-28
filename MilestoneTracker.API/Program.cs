using Hangfire;
using MilestoneTracker.Application;
using MilestoneTracker.Infrastructure;
using Serilog;
using MilestoneTracker.API.Extensions;
using MilestoneTracker.Infrastructure.Options;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.ApplyConfigurations(builder.Configuration)
    .AddTelegramBot(builder.Configuration)
    .AddInfractructure(builder.Configuration)
    .AddApplication()
    .AddSerilogLogging()
    .AddAppHealthChecks();


var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. User: {UserId}";
});


if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
    app.UseHangfireDashboard(); 
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseHangfireRecurringJobs();

app.MapControllers();
app.Run();