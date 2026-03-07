using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ST.Funds.Api.Controllers;
using ST.Funds.Application.DTO;
using ST.Funds.Application.Services.FundIngestion;
using ST.Funds.Data.Models;

namespace ST.Funds.Api.Tests
{
    [TestFixture]
    public class FundsControllerTests
    {
        private Mock<IFundIngestionService> _serviceMock;
        private Mock<Microsoft.Extensions.Logging.ILogger<FundsController>> _loggerMock;
        private FundsController _controller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<IFundIngestionService>();
            _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<FundsController>>();

            _controller = new FundsController(
                _serviceMock.Object,
                _loggerMock.Object);
        }

        // ===============================
        // GET FUNDS - SUCCESS
        // ===============================
        [Test]
        public async Task GetFunds_Returns_Ok()
        {
            var funds = new List<FundDto>
            {
                new FundDto { Name = "Fund1" }
            };

            _serviceMock
                .Setup(s => s.GetFundsAsync(It.IsAny<FundQueryParameters>()))
                .ReturnsAsync(funds);

            var result = await _controller.GetFunds(
                new FundQueryParameters(),
                CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.EqualTo(funds));
        }

        // ===============================
        // GET BY MARKET CODE - SUCCESS
        // ===============================
        [Test]
        public async Task GetByMarketCode_Returns_Ok()
        {
            var fund = new FundDto { Name = "Test" };

            _serviceMock
                .Setup(s => s.GetByMarketCodeAsync("ABC"))
                .ReturnsAsync(fund);

            var result = await _controller.GetByMarketCode(
                "ABC",
                CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.EqualTo(fund));
        }

        // ===============================
        // GET BY MARKET CODE - NOT FOUND
        // ===============================
        [Test]
        public async Task GetByMarketCode_Returns_NotFound()
        {
            _serviceMock
                .Setup(s => s.GetByMarketCodeAsync("ABC"))
                .ReturnsAsync((FundDto?)null);

            var result = await _controller.GetByMarketCode(
                "ABC",
                CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        // ===============================
        // REFRESH - SUCCESS
        // ===============================
        [Test]
        public async Task Refresh_Returns_NoContent()
        {
            _serviceMock
                .Setup(s => s.RefreshFundsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        // ===============================
        // REFRESH - SERVICE CALLED
        // ===============================
        [Test]
        public async Task Refresh_Calls_Service()
        {
            _serviceMock
                .Setup(s => s.RefreshFundsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _controller.Refresh(CancellationToken.None);

            _serviceMock.Verify(
                s => s.RefreshFundsAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}