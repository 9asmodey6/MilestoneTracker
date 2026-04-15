namespace MilestoneTracker.Infrastructure.Services;

using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

public class UpdateChannelQueue
{
    private readonly Channel<Update> _channel;
    private readonly ILogger<UpdateChannelQueue> _logger;

    public UpdateChannelQueue(ILogger<UpdateChannelQueue> logger)
    {
        _logger = logger;

        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<Update>(options);

        _logger.LogInformation("Background Update Queue initialized");
    }

    public async ValueTask PushUpdateAsync(Update update, CancellationToken ct)
    {
        _logger.LogInformation(
            "Update {Id} queued. Current queue count: {Count}",
            update.Id,
            _channel.Reader.Count);

        await _channel.Writer.WriteAsync(update, ct);

        _logger.LogInformation("Update {UpdateId} successfully enqueued", update.Id);
    }

    public IAsyncEnumerable<Update> ReadAllAsync(CancellationToken ct)
    {
       return _channel.Reader.ReadAllAsync(ct);
    }
}