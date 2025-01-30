using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrenciesController : ControllerBase
    {
        private readonly CurrencyExchangeContext _context;
        private readonly ILogger<CurrenciesController> _logger;

        public CurrenciesController(CurrencyExchangeContext context, ILogger<CurrenciesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

[HttpGet]
public async Task<ActionResult<IEnumerable<string>>> GetCurrencies()
{
    _logger.LogInformation("Fetching distinct currency codes from the Rates table.");
    var currencies = await _context.Rates
        .Select(r => r.Code)
        .Distinct()
        .ToListAsync();

    return Ok(currencies);
}


private async Task<bool> FetchFromNBP(DateTime startDate, DateTime endDate)
{
    using (var httpClient = new HttpClient())
    {
        var currentStartDate = startDate;
        while (currentStartDate <= endDate)
        {
            var currentEndDate = currentStartDate.AddDays(92);
            if (currentEndDate > endDate)
            {
                currentEndDate = endDate;
            }

            var url = $"http://api.nbp.pl/api/exchangerates/tables/A/{currentStartDate:yyyy-MM-dd}/{currentEndDate:yyyy-MM-dd}/?format=json";
            _logger.LogInformation($"📡 Wysyłam żądanie do: {url}");

            try
            {
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"⚠️ Błąd API NBP: {response.StatusCode}");
                    return false;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var exchangeRates = JsonSerializer.Deserialize<List<NBPTable>>(jsonResponse);

                if (exchangeRates == null || !exchangeRates.Any())
                {
                    _logger.LogWarning("⚠️ API NBP nie zwróciło danych.");
                    return false;
                }

                foreach (var table in exchangeRates)
                {
                    var tableEntry = await _context.Tables.FirstOrDefaultAsync(t => t.No == table.No);
                    if (tableEntry == null)
                    {
                        tableEntry = new TableA
                        {
                            TableName = table.Table,
                            No = table.No,
                            EffectiveDate = table.EffectiveDate
                        };
                        _context.Tables.Add(tableEntry);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"✅ Dodano wpis do TableA: {table.No}");
                    }

                    foreach (var rate in table.Rates)
                    {
                        if (string.IsNullOrWhiteSpace(rate.Code) || rate.Mid <= 0)
                        {
                            _logger.LogWarning($"⚠️ Pominięto błędny kurs: Kod={rate.Code}, Mid={rate.Mid}");
                            continue;
                        }

                        var existingRate = await _context.Rates
                            .FirstOrDefaultAsync(r => r.TableId == tableEntry.Id && r.Code == rate.Code);

                        if (existingRate == null)
                        {
                            var rateEntry = new Rate
                            {
                                TableId = tableEntry.Id,
                                Currency = rate.Currency,
                                Code = rate.Code,
                                Mid = rate.Mid
                            };
                            _context.Rates.Add(rateEntry);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Dane zapisane w bazie.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Błąd podczas pobierania danych: {ex.Message}");
                return false;
            }

            currentStartDate = currentEndDate.AddDays(1); // Przesuwamy przedział czasowy
        }
    }

    return true;
}



        [HttpPost("fetch/{startDate}/{endDate}")]
public async Task<IActionResult> FetchCurrencies(string startDate, string endDate)
{
    if (!DateTime.TryParse(startDate, out var parsedStartDate) || !DateTime.TryParse(endDate, out var parsedEndDate))
    {
        _logger.LogWarning("⚠️ Nieprawidłowy format daty.");
        return BadRequest("⚠️ Nieprawidłowy format daty. Użyj YYYY-MM-DD.");
    }

    _logger.LogInformation($"📡 Pobieranie kursów dla zakresu: {parsedStartDate} - {parsedEndDate}");
    
    var success = await FetchFromNBP(parsedStartDate, parsedEndDate);
    
    if (!success)
    {
        return StatusCode(500, "❌ Nie udało się pobrać danych z NBP API.");
    }

    // 📊 Po zapisaniu pobierz dane z bazy i zwróć na frontend
    var rates = await _context.Rates
        .Where(r => r.Table.EffectiveDate >= parsedStartDate && r.Table.EffectiveDate <= parsedEndDate)
        .Select(r => new RateDto
        {
            EffectiveDate = r.Table.EffectiveDate,
            Currency = r.Currency,
            Code = r.Code,
            Mid = r.Mid
        })
        .ToListAsync();

    _logger.LogInformation($"✅ Dane pobrane i zapisane w bazie. Liczba rekordów: {rates.Count}");

    return Ok(rates);
}

        
[HttpGet("rates")]
public async Task<ActionResult<IEnumerable<RateDto>>> GetRates(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] string? filterType,
    [FromQuery] int? year,
    [FromQuery] int? quarter,
    [FromQuery] int? month,
    [FromQuery] string? day)
{
    _logger.LogInformation($"Fetching rates from the database for range: {startDate} to {endDate}");
    
    var query = _context.Rates
        .Where(r => r.Table.EffectiveDate >= startDate && r.Table.EffectiveDate <= endDate)
        .Select(r => new RateDto
        {
            EffectiveDate = r.Table.EffectiveDate,
            Currency = r.Currency,
            Code = r.Code,
            Mid = r.Mid
        });

    if (filterType == "year" && year.HasValue)
        query = query.Where(r => r.EffectiveDate.Year == year);

    if (filterType == "quarter" && quarter.HasValue)
        query = query.Where(r => (r.EffectiveDate.Month - 1) / 3 + 1 == quarter);

    if (filterType == "month" && month.HasValue)
        query = query.Where(r => r.EffectiveDate.Month == month);

    if (filterType == "day" && !string.IsNullOrEmpty(day))
        query = query.Where(r => r.EffectiveDate.ToString("yyyy-MM-dd") == day);

    return Ok(await query.ToListAsync());
}

// DTO for returning rate data
public class RateDto
{
    public DateTime EffectiveDate { get; set; }
    public string Currency { get; set; }
    public string Code { get; set; }
    public decimal Mid { get; set; }
}

[HttpGet("validate")]
public async Task<IActionResult> ValidateData()
{
    _logger.LogInformation("Validating data in the Rates table.");
    var invalidRates = await _context.Rates
        .Where(r => string.IsNullOrEmpty(r.Code) || r.Mid <= 0)
        .ToListAsync();

    if (!invalidRates.Any())
    {
        _logger.LogInformation("No invalid data found in the Rates table.");
        return Ok("No invalid data found.");
    }

    _logger.LogWarning($"Found {invalidRates.Count} invalid entries in the Rates table.");
    return Ok(invalidRates);
}

    }

    // Models for JSON deserialization
    public class NBPTable
{
    [JsonPropertyName("table")]
    public string Table { get; set; }

    [JsonPropertyName("no")]
    public string No { get; set; }

    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }

    [JsonPropertyName("rates")]
    public List<NBPRate> Rates { get; set; } = new List<NBPRate>();
}


    public class NBPRate
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("mid")]
        public decimal Mid { get; set; }
    }
}
