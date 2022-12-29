namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Controllers
{
    using System;

    public class HateoasLink
    {
        public Uri Ref { get; set; }
        public string Rel { get; set; }
        public string Type { get; set; }

        public HateoasLink(Uri @ref, string rel, string type)
        {
            Ref = @ref;
            Rel = rel;
            Type = type;
        }
    }
}
