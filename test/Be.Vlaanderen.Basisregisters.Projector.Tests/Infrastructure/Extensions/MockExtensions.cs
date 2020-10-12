namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure.Extensions
{
    using System;
    using Autofac.Features.OwnedInstances;
    using Moq;

    public static class MockExtensions
    {
        public static Owned<T> CreateOwnedObject<T>(this Mock<T> mock)
            where T : class
            => new Owned<T>(mock.Object, Mock.Of<IDisposable>());
    }
}
