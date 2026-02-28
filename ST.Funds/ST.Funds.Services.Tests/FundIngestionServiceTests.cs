using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ST.Funds.Data.DataContext;
using ST.Funds.Data.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using ST.Funds.Application.Services.FundIngestion;
using ST.Funds.Application.Config;

namespace ST.Funds.Services.Tests
{
    [TestFixture]
    public class FundIngestionServiceTests
    {
        private FundsDbContext _context;
        private Mock<ILogger<FundIngestionService>> _loggerMock;
        private IOptions<List<FundSourceConfig>> _options;

        // ============================
        // SETUP
        // ============================
        [SetUp]
        public void Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<FundsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new FundsDbContext(dbOptions);

            _loggerMock = new Mock<ILogger<FundIngestionService>>();

            _options = Options.Create(new List<FundSourceConfig>
            {
                new FundSourceConfig
                {
                    Url = "http://test.com"
                }
            });
        }

        private FundIngestionService CreateService(HttpClient client)
        {
            return new FundIngestionService(
                client,
                _context,
                _options,
                _loggerMock.Object);
        }

        // ============================
        // FAKE HTTP HANDLER
        // ============================
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public FakeHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        // ============================
        // GetFundsAsync returns all
        // ============================
        [Test]
        public async Task GetFundsAsync_Returns_All_Funds()
        {
            _context.Funds.Add(new Fund { Name = "Fund1", MarketCode = "F1" });
            _context.Funds.Add(new Fund { Name = "Fund2", MarketCode = "F2" });
            await _context.SaveChangesAsync();

            var service = CreateService(new HttpClient());

            var result = await service.GetFundsAsync(new FundQueryParameters());

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        // ============================
        // Filtering works
        // ============================
        [Test]
        public async Task GetFundsAsync_Filters_By_OngoingCharge()
        {
            _context.Funds.Add(new Fund { Name = "Low", OngoingCharge = 0.2m });
            _context.Funds.Add(new Fund { Name = "High", OngoingCharge = 0.6m });
            await _context.SaveChangesAsync();

            var service = CreateService(new HttpClient());

            var result = await service.GetFundsAsync(
                new FundQueryParameters { MaxOngoingCharge = 0.3m });

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Low"));
        }

        // ============================
        // GetByMarketCodeAsync returns fund
        // ============================
        [Test]
        public async Task GetByMarketCodeAsync_Returns_Fund()
        {
            _context.Funds.Add(new Fund { Name = "Test", MarketCode = "ABC" });
            await _context.SaveChangesAsync();

            var service = CreateService(new HttpClient());

            var result = await service.GetByMarketCodeAsync("ABC");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Test"));
        }

        // ============================
        // GetByMarketCodeAsync returns null
        // ============================
        [Test]
        public async Task GetByMarketCodeAsync_Returns_Null_When_NotFound()
        {
            var service = CreateService(new HttpClient());

            var result = await service.GetByMarketCodeAsync("NOTFOUND");

            Assert.That(result, Is.Null);
        }

        // ============================
        // RefreshFundsAsync adds new fund
        // ============================
        [Test]
        public async Task RefreshFundsAsync_Adds_New_Fund()
        {
            var external = new
            {
                data = new
                {
                    quote = new
                    {
                        name = "New Fund",
                        marketCode = "NEW1",
                        lastPrice = 100,
                        lastPriceDate = DateTime.UtcNow,
                        ongoingCharge = 0.3m,
                        sectorName = "Sector",
                        currency = "GBP"
                    }
                }
            };

            var json = JsonSerializer.Serialize(external);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);

            var service = CreateService(client);

            await service.RefreshFundsAsync();

            Assert.That(_context.Funds.Count(), Is.EqualTo(1));
        }

        // ============================
        // Invalid external response handled
        // ============================
        [Test]
        public async Task RefreshFundsAsync_Invalid_Response_Does_Not_Add()
        {
            var json = "{}";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var handler = new FakeHttpMessageHandler(response);
            var client = new HttpClient(handler);

            var service = CreateService(client);

            await service.RefreshFundsAsync();

            Assert.That(_context.Funds.Count(), Is.EqualTo(0));
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}