namespace NP.Lti13Platform.AssignmentGradeServices.Configs
{
    public record ServicesConfig
    {
        public Uri ServiceAddress { get; set; } = DefaultUri;

        internal readonly static Uri DefaultUri = new("x://x.x.x");
    }
}
