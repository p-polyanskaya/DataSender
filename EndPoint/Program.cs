using GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddSingleton<Processor>();

var app = builder.Build();

app.MapGrpcService<DataSenderGrpcService>();
app.MapGrpcReflectionService();

app.Run();