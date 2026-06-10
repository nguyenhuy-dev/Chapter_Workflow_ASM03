using MangaWorkflow.APIGrpcService.HuyNQ.Services;
using MangaWorkflow.Repositories.HuyNQ;
using MangaWorkflow.Services.HuyNQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddScoped<IChapterHuyNqService, ChapterHuyNqService>();
builder.Services.AddScoped<ChapterHuyNqRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<ChapterHuyNqGRPCService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
