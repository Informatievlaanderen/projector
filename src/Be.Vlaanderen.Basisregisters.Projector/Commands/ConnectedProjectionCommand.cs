namespace Be.Vlaanderen.Basisregisters.Projector.Commands
{
    using Newtonsoft.Json;

    public abstract class ConnectedProjectionCommand
    {
        public string Command => GetType().Name;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
