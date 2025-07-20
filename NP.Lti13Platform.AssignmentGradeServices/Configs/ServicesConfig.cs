namespace NP.Lti13Platform.AssignmentGradeServices.Configs
{
    /// <summary>
    /// Represents the configuration for assignment grade services.
    /// </summary>
    public record ServicesConfig
    {
        /// <summary>
        /// Gets or sets the base address of the service.
        /// </summary>
        public Uri ServiceAddress { get; set; } = DefaultUri;

        /// <summary>
        /// The default URI for the service.
        /// </summary>
        internal readonly static Uri DefaultUri = new("x://x.x.x");
    }
}
