using Blog.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
var app = builder.Build();

app.MapGrpcService<BlogServiceImpl>();
app.Run();
