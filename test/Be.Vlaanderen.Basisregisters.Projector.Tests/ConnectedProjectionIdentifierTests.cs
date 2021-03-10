namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using ConnectedProjections;
    using FluentAssertions;
    using Xunit;

    public class ConnectedProjectionIdentifierTests
    {
        [Fact]
        public void When_creating_connected_projection_identifier_from_a_type_then_identifier_is_the_fully_qualified_name()
        {
            var identifier = new ConnectedProjectionIdentifier(typeof(ConnectedProjectionIdentifierTests));
            identifier.Should().Be("Be.Vlaanderen.Basisregisters.Projector.Tests.ConnectedProjectionIdentifierTests");
        }
    }
}
