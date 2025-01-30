using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ✅ Konfiguracja logowania do pliku
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // 🔥 Użyj Serilog jako loggera aplikacji

// ✅ Konfiguracja usług
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Konfiguracja SQLite
builder.Services.AddDbContext<CurrencyExchangeContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CurrencyExchangeContext")));

// ✅ Konfiguracja CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Budowa aplikacji
var app = builder.Build();

// ✅ Aktywacja CORS
app.UseCors("AllowAll");

// ✅ Automatyczne migracje
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CurrencyExchangeContext>();

    try
    {
        Log.Information("Applying migrations...");
        dbContext.Database.Migrate(); // ✅ Automatyczna migracja
        Log.Information("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Error($"Error applying migrations: {ex.Message}");
    }
}

// ✅ Konfiguracja Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // 🔥 Logowanie żądań HTTP
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// ✅ Uruchomienie aplikacji
try
{
    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
