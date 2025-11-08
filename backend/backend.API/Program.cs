using backend.API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var pathBanco = Path.Combine(AppContext.BaseDirectory, "trabalhoFinal.db");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={pathBanco}"));

//Adicionar repos

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//teste

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.EnsureCreated();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Fallback para o React (SPA)
app.MapFallbackToFile("index.html");

app.Run();
