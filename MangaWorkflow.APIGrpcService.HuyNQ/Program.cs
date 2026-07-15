using MangaWorkflow.APIGrpcService.HuyNQ.Commons;
using MangaWorkflow.APIGrpcService.HuyNQ.Services;
using MangaWorkflow.Repositories.HuyNQ;
using MangaWorkflow.Services.HuyNQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddScoped<IChapterHuyNqService, ChapterHuyNqService>();
builder.Services.AddScoped<ChapterHuyNqRepository>();

builder.Services.AddScoped<ISystemUserAccountService, SystemUserAccountService>();
builder.Services.AddScoped<SystemUserAccountRepository>();

// Tracks tokens revoked via logout. Singleton so the in-memory store is shared.
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // Reject tokens that were revoked through the logout endpoint.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();

                if (jti != null && blacklist.IsRevoked(jti))
                    context.Fail("This token has been revoked.");

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<ChapterHuyNqGRPCService>();
app.MapGrpcService<SystemUserAccountGRPCService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
