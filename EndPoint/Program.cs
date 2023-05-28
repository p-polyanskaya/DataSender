using GrpcServices;
using Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GrpcSettings>(builder.Configuration.GetSection(nameof(GrpcSettings)));
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddSingleton<Processor>();

var app = builder.Build();

app.MapGrpcService<DataSenderGrpcService>();
app.MapGrpcReflectionService();

app.Run();