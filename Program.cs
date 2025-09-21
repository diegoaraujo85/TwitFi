using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using TweetFi.Data;
using TweetFi.Services;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco SQLite
builder.Services.AddDbContext<TwitterDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();    

builder.Services.AddHttpClient();
builder.Services.AddScoped<TwitterServiceV2>();
builder.Services.AddHostedService<TwitterPollingServiceV2>();

var app = builder.Build();

// Limpa o console
Console.Clear();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== tweetFi Service ===");
Console.ResetColor();

// Exibe endereço do serviço
var serverAddresses = app.Urls.Count > 0 ? string.Join(", ", app.Urls) : "http://localhost:5000";
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Servidor iniciado em: {serverAddresses}");
Console.ResetColor();

// Contagem inicial de usuários
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TwitterDbContext>();
    var totalUsers = await db.TwitterStates.CountAsync();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Usuários monitorados no banco: {totalUsers}");
    Console.ResetColor();
}

// Endpoints de teste
app.MapGet("/", () => "tweetFi API rodando!");
app.MapGet("/api/twitter/summary", async (TwitterDbContext db) =>
{
    var states = await db.TwitterStates.ToListAsync();
    return Results.Ok(states);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
