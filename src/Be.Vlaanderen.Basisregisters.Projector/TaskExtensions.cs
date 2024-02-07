namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable NoContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<TResult> NoContext<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}
