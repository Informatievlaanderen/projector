namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Commands
{
    using System;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal.Commands;
    using Xunit;

    public class When_creating_a_restart_command_with_a_negative_delay
    {
        private readonly Restart _sut;

        public When_creating_a_restart_command_with_a_negative_delay()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers();

            _sut = new Restart(
                fixture.Create<ConnectedProjectionIdentifier>(),
                TimeSpan.FromSeconds(fixture.CreateNegative<int>()));
        }

        [Fact]
        public void Then_the_delay_is_set_to_zero()
        {
            _sut.After.Should().Be(TimeSpan.Zero);
        }
    }
}
