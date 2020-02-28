using Flurl.Http.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Threading.Tasks;
using Ticketbooth.Scanner.Domain.Data.Dtos;
using Ticketbooth.Scanner.Domain.Interfaces;
using Ticketbooth.Scanner.Infrastructure.Services;
using Ticketbooth.Scanner.Shared;

namespace Ticketbooth.Scanner.Infrastructure.Tests.Services
{
    public class BlockStoreServiceTests
    {
        private Mock<IOptions<NodeOptions>> _nodeOptions;
        private Mock<ILogger<BlockStoreService>> _logger;
        private HttpTest _httpTest;
        private IBlockStoreService _blockStoreService;

        [SetUp]
        public void SetUp()
        {
            _nodeOptions = new Mock<IOptions<NodeOptions>>();
            _logger = new Mock<ILogger<BlockStoreService>>();
            _nodeOptions.Setup(callTo => callTo.Value).Returns(new NodeOptions { ApiUri = "http://190.178.5.293" });
            _httpTest = new HttpTest();
        }

        [TearDown]
        public void TearDown()
        {
            _httpTest.Dispose();
        }

        [Test]
        public async Task GetBlockData_200_ReturnsResponse()
        {
            // Arrange
            var receipt = new Receipt<BlockDto, object>
            {
                ReturnValue = new BlockDto { Height = 1000 }
            };
            _httpTest.RespondWithJson(receipt, status: 200);
            _blockStoreService = new BlockStoreService(_nodeOptions.Object, _logger.Object);

            // Act
            var result = await _blockStoreService.GetBlockDataAsync("hx78s8dj3uuiwejfuew98f8wef8");

            // Assert
            var expected = JsonConvert.SerializeObject(receipt.ReturnValue);
            var actual = JsonConvert.SerializeObject(result);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public async Task GetBlockData_400_LogsErrorReturnsNull()
        {
            // Arrange
            _httpTest.RespondWith(status: 400);
            _blockStoreService = new BlockStoreService(_nodeOptions.Object, _logger.Object);

            // Act
            var result = await _blockStoreService.GetBlockDataAsync("hx78s8dj3uuiwejfuew98f8wef8");

            // Assert
            _logger.VerifyLog(LogLevel.Error);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBlockData_500_LogsErrorReturnsNull()
        {
            // Arrange
            _httpTest.RespondWith(status: 500);
            _blockStoreService = new BlockStoreService(_nodeOptions.Object, _logger.Object);

            // Act
            var result = await _blockStoreService.GetBlockDataAsync("hx78s8dj3uuiwejfuew98f8wef8");

            // Assert
            _logger.VerifyLog(LogLevel.Error);
            Assert.That(result, Is.Null);
        }
    }
}
