using System.Threading.Channels;

namespace StockAlert.Services;

internal sealed class QueuedAlertSender : IAlertSender, IAsyncDisposable
{
    private readonly IAlertSender _inner;
    private readonly Channel<AlertItem> _channel;
    private readonly CancellationToken _applicationToken;
    private readonly Task _workerTask;

    private readonly record struct AlertItem(string Subject, string Body);

    public QueuedAlertSender(IAlertSender inner, CancellationToken applicationToken)
    {
        _inner = inner;
        _applicationToken = applicationToken;

        _channel = Channel.CreateUnbounded<AlertItem>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        _workerTask = Task.Run(ProcessQueueAsync, applicationToken);
    }

    public async Task SendAsync(string subject, string body, CancellationToken cancellationToken)
    {
        var item = new AlertItem(subject, body);

        // Honra o cancelamento do chamador na hora de enfileirar
        await _channel.Writer.WriteAsync(item, cancellationToken);
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            await foreach (var item in _channel.Reader.ReadAllAsync(_applicationToken))
            {
                try
                {
                    await _inner.SendAsync(item.Subject, item.Body, _applicationToken);
                }
                catch (OperationCanceledException) when (_applicationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[AlertQueue] Falha ao enviar alerta: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException) when (_applicationToken.IsCancellationRequested)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();

        try
        {
            await _workerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_applicationToken.IsCancellationRequested)
        {
        }

        if (_inner is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (_inner is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
