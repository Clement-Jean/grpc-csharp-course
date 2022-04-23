using greet.server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureServices((context, services) =>
{
    HostConfig.CertificateFileLocation = context.Configuration["CertPath"];
    HostConfig.CertificatePassword = context.Configuration["CertPassword"];
}).ConfigureKestrel(opt =>
{
    opt.ListenAnyIP(50052, listenOpt =>
    {
        _ = listenOpt.UseHttps(HostConfig.CertificateFileLocation, HostConfig.CertificatePassword);
    });
    opt.ListenAnyIP(50051);
});

builder.Services.AddGrpc();
var app = builder.Build();

app.MapGrpcService<GreetServiceImpl>();
app.Run();