using System.Net.Http.Json;
using System.Text.Json;
using Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcServices;

public class DataSenderGrpcService: DataStreamer.DataStreamerBase
{
    private readonly Processor _processor;

    public DataSenderGrpcService(Processor processor)
    {
        _processor = processor;
    }
    
    public override async Task<AddNewsToFileResponse> AddNewsToFile(AddNewsToFileRequest request, ServerCallContext context)
    {
        var httpClient = new HttpClient();
        var res = await httpClient.GetAsync("https://newsdata.io/api/1/news?apikey=pub_224923e40254a69059d9ee747acc34275d757&language=ru"
                                            + request.Category);
        var content = await res.Content.ReadFromJsonAsync<NewsResponse>(
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

        const string fileName = "news.txt";
        
        if (content is null || content.Status != "success")
        {
            throw new Exception("Ошибка при получении новостей.");
        }

        Console.WriteLine("Получили из источника новостей - " + content.Results.Count);
        if (content.Results.Any())
        {
            var newsToSave = new List<News>();
            if (File.Exists(fileName) && new FileInfo(fileName).Length != 0)
            {
                var existedData = await File.ReadAllTextAsync(fileName);
                var existedNews = JsonSerializer.Deserialize<List<News>>(
                    existedData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                newsToSave.AddRange(existedNews);
            }

            var newsWithText = content.Results
                .Where(x => x.Content is not null || x.Description is not null)
                .ToList();
            
            newsToSave.AddRange(newsWithText);

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                var serializedResult = JsonSerializer.Serialize(newsToSave);
                await writer.WriteAsync(serializedResult);
            }
            Console.WriteLine("Добавили в файл новостей - " + newsToSave.Count);
        }

        return new AddNewsToFileResponse();
    }

    public override async Task<StartStreamResponse> StartStream(StartStreamRequest request, ServerCallContext context)
    {
        await _processor.Start(context.CancellationToken);
        return new StartStreamResponse();
    }

    public override async Task<EndStreamResponse> EndStream(EndStreamRequest request, ServerCallContext context)
    {
        await _processor.Stop();
        return new EndStreamResponse();
    }
}