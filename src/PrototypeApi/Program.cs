using PrototypeApi.Configuration;
using PrototypeApi.HealthChecks;
using PrototypeApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Local secrets override (gitignored) - real keys/passwords go here, never in appsettings.*.json
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// --- Framework services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Global exception handling -> ProblemDetails ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- CORS: allow any origin but only GET/POST methods ---
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .WithMethods("GET", "POST")));

// --- Application DI wiring (see Configuration/ApplicationServiceConfig.cs) ---
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgresql");

var app = builder.Build();

await StartupChecks.EnsureDatabaseIsReachableAsync(app);

// Must run early so it wraps the rest of the pipeline
app.UseExceptionHandler();

// --- Middleware pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
