namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using ConnectedProjections;
    using FluentAssertions;
    using Xunit;

    public class ConnectedProjectionNameTests
    {
        [Fact]
        public void When_creating_connected_projection_name_then_name_is_fully_qualified()
        {
            var name = new ConnectedProjectionName(typeof(ConnectedProjectionNameTests));
            name.Should().Be("Be.Vlaanderen.Basisregisters.Projector.Tests.ConnectedProjectionNameTests");
        }
    }
}
