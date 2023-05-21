using System.Text.Json;
using Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

namespace GrpcServices;

public class Processor
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private const string UndefinedValue = "Неизвестно";

    public Processor()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public Task Start(CancellationToken cancellationToken)
    {
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _cancellationTokenSource.Token);
        
        Task.Factory.StartNew(() => StartInternal(linkedToken.Token), TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    private async Task StartInternal(CancellationToken cancellationToken)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5005");
        var client = new DataStreamer.DataStreamerClient(channel);
        using var call = client.SendStreamData();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var fileName = "news.txt";
            var existedData = await File.ReadAllTextAsync(fileName, cancellationToken);
            var existedNews = JsonSerializer.Deserialize<List<News>>(
                existedData,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            foreach (var news in existedNews)
            {
                //var newsText = news.Content.ToString() ?? news.Description ?? UndefinedValue;

                //if (newsText != UndefinedValue)
                {
                    
                    await call.RequestStream.WriteAsync(new Request
                    {
                        Message = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Text = "some",//newsText,
                            TimeOfMessage = DateTime.UtcNow.ToTimestamp(),
                            Source = "source",//news.SourceId ?? UndefinedValue,
                            Author = "author"//news.Creator.ToString() ?? UndefinedValue
                        }
                    }, cancellationToken);
                }
            }
        }
    }

    public Task Stop()
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}