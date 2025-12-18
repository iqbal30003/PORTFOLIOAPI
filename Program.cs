using PortfolioAPI.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ Fix enum binding (string + case-insensitive)
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Must be early
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("FrontendPolicy");

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");

app.UseAuthorization();
app.MapControllers();

app.Run();
