namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading.Tasks;

    internal static class TaskRunner
    {
        public static Task Dispatch(Func<Task> asyncAction)
        {
            return Dispatch(() => asyncAction().GetAwaiter());
        }

        public static Task Dispatch(Action action)
        {
            void SilenceErrorContinuationAction(Task task)
            {
                try { task.Wait(); }
                catch { /* Swallow the exception */ }
            }

            return Dispatch(action, SilenceErrorContinuationAction);
        }

        public static Task Dispatch(Action action, Action<Exception> exceptionHandler)
        {
            if(null == exceptionHandler)
                throw new ArgumentNullException(nameof(exceptionHandler));

            return Dispatch(
                action,
                task =>
                {
                    var exception = task?.Exception?.GetBaseException();
                    if (null != exception)
                        exceptionHandler(exception);
                });
        }

        private static Task Dispatch(Action action, Action<Task> continuationAction)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var task = new Task(action);
            var continuation = task.ContinueWith(
                continuationAction,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);

            task.Start();

            return continuation;
        }
    }
}
