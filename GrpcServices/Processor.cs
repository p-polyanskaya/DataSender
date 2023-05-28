using System.Text.Json;
using Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Options;
using Microsoft.Extensions.Options;

namespace GrpcServices;

public class Processor
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private const string UndefinedValue = "Неизвестно";
    private readonly IOptions<GrpcSettings> _grpcSettings;

    public Processor(IOptions<GrpcSettings> grpcSettings)
    {
        _grpcSettings = grpcSettings;
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
        using var channel = GrpcChannel.ForAddress(_grpcSettings.Value.Url);
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
                var newsText = news.Content ?? news.Description ?? UndefinedValue;

                if (newsText != UndefinedValue)
                {
                    await call.RequestStream.WriteAsync(new Request
                    {
                        Message = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Text = newsText,
                            TimeOfMessage = DateTime.UtcNow.ToTimestamp(),
                            Source = news.SourceId ?? UndefinedValue,
                            Author = GetAuthor(news.Creator)
                        }
                    }, cancellationToken);
                }
            }
        }
    }

    private string GetAuthor(List<string>? creator)
    {
        if (creator is null || !creator.Any())
        {
            return UndefinedValue;
        }

        var author = string.Join(',', creator);
        return author;
    }

    public Task Stop()
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}