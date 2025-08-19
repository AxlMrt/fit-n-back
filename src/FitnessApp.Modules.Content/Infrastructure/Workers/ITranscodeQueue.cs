using System.Threading.Channels;

namespace FitnessApp.Modules.Content.Infrastructure.Workers;

public interface ITranscodeQueue
{
    Task EnqueueAsync(TranscodeRequest request);
}

public class ChannelTranscodeQueue : ITranscodeQueue
{
    private readonly Channel<TranscodeRequest> _channel;

    public ChannelTranscodeQueue(Channel<TranscodeRequest> channel)
    {
        _channel = channel;
    }

    public Task EnqueueAsync(TranscodeRequest request)
    {
        if (!_channel.Writer.TryWrite(request))
        {
            // fallback to waiting
            return _channel.Writer.WriteAsync(request).AsTask();
        }

        return Task.CompletedTask;
    }
}
