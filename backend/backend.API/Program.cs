using backend.API;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var pathBanco = Path.Combine(AppContext.BaseDirectory, "trabalhoFinal.db");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={pathBanco}"));

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<LogsService>();

// Configurar Antiforgery (CSRF protection)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; // Nome do header que o React vai enviar
    options.Cookie.Name = "XSRF-TOKEN"; // Nome do cookie
    options.Cookie.SameSite = SameSiteMode.Strict; // Proteção adicional
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Apenas HTTPS
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Necessário para cookies CSRF
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowFrontend");
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Endpoint para fornecer o token CSRF ao frontend
var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
app.MapGet("/api/csrf-token", (HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
        new CookieOptions
        {
            HttpOnly = false, // Frontend precisa ler o cookie
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
    return Results.Ok(new { token = tokens.RequestToken });
}).WithName("GetCsrfToken");

app.MapControllers();

// Fallback para o React (SPA)
app.MapFallbackToFile("index.html");

app.Run();
