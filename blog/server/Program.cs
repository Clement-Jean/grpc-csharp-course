using Blog.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opt =>
{
    opt.ListenAnyIP(50051);
});
builder.Services.AddGrpc();
var app = builder.Build();

app.MapGrpcService<BlogServiceImpl>();
app.Run();
