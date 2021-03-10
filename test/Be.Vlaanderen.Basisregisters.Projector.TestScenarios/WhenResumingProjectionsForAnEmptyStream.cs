namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Internal;
    using SqlStreamStore;
    using TestProjections.Projections;
    using Xunit;

    public class WhenResumingProjectionsForAnEmptyStream : Scenario
    {
        private  readonly ConnectedProjectionIdentifier  _projectionToResume =  new ConnectedProjectionIdentifier(typeof(FastProjections));
        private IRegisteredProjections _registeredProjections;

        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder
                .RegisterProjections<SlowProjections, ProjectionContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<FastProjections, ProjectionContext>(ConnectedProjectionSettings.Default);
        }

        protected override async Task Setup()
        {
            _registeredProjections = Resolve<IRegisteredProjections>();

            await _registeredProjections
                .GetProjection(_projectionToResume)
                .UpdateUserDesiredState(UserDesiredState.Started, CancellationToken.None);
        }

        [Fact]
        public async Task VerifySetup()
        {
            (await Resolve<IReadonlyStreamStore>().ReadHeadPosition())
                .Should()
                .Be(HeadPosition.NoMessages);

            (await _registeredProjections
                    .GetProjection(_projectionToResume)
                    .ShouldResume(CancellationToken.None))
                .Should()
                .BeTrue();

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .OnlyContain(connectedProjection => connectedProjection.State == ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_all_projections_that_should_resume_are_subscribed()
        {
            await ProjectionManager.Resume(CancellationToken.None);

            await Task.Delay(1000);
            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(projection =>
                    projection.Id == _projectionToResume
                    && projection.State == ConnectedProjectionState.Subscribed);
        }

        [Fact]
        public async Task Then_all_projections_that_should_not_resume_are_still_stopped()
        {
            await ProjectionManager.Resume(CancellationToken.None);

            await Task.Delay(1000);
            ProjectionManager
                .GetRegisteredProjections()
                .Where(projection => projection.Id != _projectionToResume)
                .Should()
                .OnlyContain(projection => projection.State == ConnectedProjectionState.Stopped);
        }
    }
}
