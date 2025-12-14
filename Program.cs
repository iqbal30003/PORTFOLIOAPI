var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.UseAuthorization();
app.MapControllers();

app.Run();
