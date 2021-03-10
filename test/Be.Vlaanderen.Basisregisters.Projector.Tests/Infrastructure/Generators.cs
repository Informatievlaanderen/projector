namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using ConnectedProjections;
    using Internal.Commands;
    using Internal.Commands.CatchUp;
    using Internal.Commands.Subscription;
    using TestProjections.OtherProjections;
    using TestProjections.Projections;

    public class Generators
    {
        internal static IReadOnlyList<Func<IFixture, CatchUpCommand>> CatchUpCommand = new Func<IFixture, CatchUpCommand>[]
        {
            fixture => new RemoveStoppedCatchUp(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new StartCatchUp(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new StopAllCatchUps(),
            fixture => new StopCatchUp(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new CustomCatchUpCommand()
        };

        internal class CustomCatchUpCommand : CatchUpCommand
        {
            public string CreatedAt { get; }

            public CustomCatchUpCommand()
            {
                var now = DateTime.UtcNow;
                CreatedAt = $"{now.ToLongTimeString()} <{now.Ticks}>";
            }
        }

        internal static IReadOnlyList<Func<IFixture, SubscriptionCommand>> SubscriptionCommand = new Func<IFixture, SubscriptionCommand>[]
        {
//            fixture => new ProcessStreamEvent(), 
            fixture => new Subscribe(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new SubscribeAll(),
            fixture => new Unsubscribe(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new UnsubscribeAll(), 
            fixture => new CustomSubscriptionCommand()
        };

        internal class CustomSubscriptionCommand : SubscriptionCommand
        {
            public string CreatedAt { get; }

            public CustomSubscriptionCommand()
            {
                var now = DateTime.UtcNow;
                CreatedAt = $"{now.ToLongTimeString()} <{now.Ticks}>";
            }
        }

        internal static IReadOnlyList<Func<IFixture, ConnectedProjectionCommand>> ProjectionCommand = new Func<IFixture, ConnectedProjectionCommand>[]
        {
            fixture => fixture.Create<CatchUpCommand>(),
            fixture => fixture.Create<SubscriptionCommand>(),
            fixture => new StartAll(),
            fixture => new Start(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new StopAll(),
            fixture => new Stop(fixture.Create<ConnectedProjectionIdentifier>()),
            fixture => new CustomCommand()
        };

        internal class CustomCommand : ConnectedProjectionCommand
        {
            public string CreatedAt { get; }

            public CustomCommand()
            {
                var now = DateTime.UtcNow;
                CreatedAt = $"{now.ToLongTimeString()} <{now.Ticks}>";
            }
        }

        internal static IReadOnlyList<Func<IFixture, ConnectedProjectionIdentifier>> ProjectionIdentifier => new Func<IFixture, ConnectedProjectionIdentifier>[]
        {
            fixture => new ConnectedProjectionIdentifier(typeof(OtherSlowProjections)),
            fixture => new ConnectedProjectionIdentifier(typeof(OtherRandomProjections)),
            fixture => new ConnectedProjectionIdentifier(typeof(FastProjections)),
            fixture => new ConnectedProjectionIdentifier(typeof(SlowProjections)),
            fixture => new ConnectedProjectionIdentifier(typeof(TrackHandledEventsProjection))
        };
    }
}
