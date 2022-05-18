using Calculator.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opt =>
{
    opt.ListenAnyIP(50051);
});
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
var app = builder.Build();

app.MapGrpcService<CalculatorServiceImpl>();
app.MapGrpcReflectionService();
app.Run();