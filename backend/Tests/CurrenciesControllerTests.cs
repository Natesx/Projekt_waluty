using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YourNamespace.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using YourNamespace;
using System;

public class CurrenciesControllerTests
{
    private readonly CurrenciesController _controller;
    private readonly Mock<ILogger<CurrenciesController>> _loggerMock;
    private readonly CurrencyExchangeContext _dbContext;

    public CurrenciesControllerTests()
    {
        _loggerMock = new Mock<ILogger<CurrenciesController>>();
        var dbContextOptions = new DbContextOptionsBuilder<CurrencyExchangeContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;

        _dbContext = new CurrencyExchangeContext(dbContextOptions);
        _controller = new CurrenciesController(_dbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCurrencies_ReturnsListOfCurrencies()
    {
        // Arrange
        _dbContext.Rates.Add(new Rate { Code = "USD", Currency = "Dolar Amerykański", Mid = 4.50M });
        _dbContext.Rates.Add(new Rate { Code = "EUR", Currency = "Euro", Mid = 4.80M });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetCurrencies();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var currencies = Assert.IsType<List<string>>(okResult.Value);
        Assert.NotEmpty(currencies);
        Assert.Contains("USD", currencies);
        Assert.Contains("EUR", currencies);
    }

    [Fact]
    public async Task GetRates_InvalidDate_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetRates(DateTime.MinValue, DateTime.MinValue, null, null, null, null, null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
