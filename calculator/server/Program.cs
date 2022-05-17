using Calculator.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
var app = builder.Build();

app.MapGrpcService<CalculatorServiceImpl>();
app.MapGrpcReflectionService();
app.Run();