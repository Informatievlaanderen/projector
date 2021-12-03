namespace Be.Vlaanderen.Basisregisters.Projector.Query.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Handlers;
    using Xunit;
    using Xunit.Abstractions;

    public class QueryEventsHandlerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public QueryEventsHandlerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        // use connectionString "Server=tcp:127.0.0.1,19001;Database=REGISTRY-DATABASE;User=basisregisters;Password=ySbATyGpjfPzd4XH7nNs2SNYPrV7sKEvcmnGt6FmETD8rSkbxqEyJ32U2gafEqxn7H3WHpP6uvM7NxA3dm9YECqugx3w4r9fhxxF"

        [Theory]
        [InlineData("", "address-registry", "8a4b70c4-7582-5bef-b634-7bea4ccc74cc")]
        [InlineData("", "municipality-registry", "86fc4ce0-7291-553f-ba81-b07b9d03e553")]
        public async Task PerformQuery(string connectionString, string registryName, string businessId)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            var request = new QueryEventsRequest(connectionString, registryName, businessId);
            var handler = new QueryEventsHandler();
            var response = await handler.Handle(request);

            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            _testOutputHelper.WriteLine($"Number of results: {response.Values?.Count()}");
        }
    }
}
