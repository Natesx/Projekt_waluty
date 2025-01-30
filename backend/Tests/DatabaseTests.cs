using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using YourNamespace;

public class DatabaseTests
{
    private readonly DbContextOptions<CurrencyExchangeContext> _dbContextOptions;

    public DatabaseTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<CurrencyExchangeContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;
    }

    [Fact]
    public async Task CanConnectToDatabase()
    {
        using (var context = new CurrencyExchangeContext(_dbContextOptions))
        {
            var canConnect = await context.Database.CanConnectAsync();
            Assert.True(canConnect);
        }
    }
}
