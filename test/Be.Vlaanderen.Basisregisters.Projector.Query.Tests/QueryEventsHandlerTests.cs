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

        [Theory]
        [InlineData("address-registry", "8a4b70c4-7582-5bef-b634-7bea4ccc74cc", "")]
        [InlineData("building-registry", "6cf9be2f-6361-5cec-b130-3b905aba3ce7", "")]
        [InlineData("municipality-registry", "86fc4ce0-7291-553f-ba81-b07b9d03e553", "")]
        [InlineData("parcel-registry", "10ad670a-ab6e-5fda-9a8a-733e11f59902", "")]
        [InlineData("postal-registry", "9000", "")]
        [InlineData("streetname-registry", "2d0b1f31-8e31-5fc5-b628-fcf5a1382674", "")]
        public async Task PerformQuery(string registryName, string businessId, string connectionString)
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
            _testOutputHelper.WriteLine($"Number of results: {response.Values?.Count()}\n");
            if (response.Values != null)
            {
                foreach (var responseValue in response.Values)
                {
                    _testOutputHelper.WriteLine(responseValue.ToString());
                }
            }
        }
    }
}
