namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ProjectionHandling.Runner;

    internal interface IMigrationHelper
    {
        void RunMigrations();
    }

    internal class MigrationHelper : IMigrationHelper
    {
        private readonly IEnumerable<IRunnerDbContextMigrator> _migrators;

        public MigrationHelper(IEnumerable<IRunnerDbContextMigrator> migrators)
            => _migrators = migrators ?? throw new ArgumentNullException(nameof(migrators));

        public void RunMigrations()
        {
            var cancellationToken = CancellationToken.None;

            Task.WaitAll(
                _migrators
                    .Select(helper => helper.MigrateAsync(cancellationToken))
                    .ToArray(),
                cancellationToken);
        }
    }
}
