namespace NP.Lti13Platform.Web
{
    public class AuthenticationRequest
    {
        public string? Scope { get; set; }

        public string? Response_Type { get; set; }

        public string? Response_Mode { get; set; }

        public string? Prompt { get; set; }

        public string? Nonce { get; set; }

        public string? State { get; set; }

        public string? Client_Id { get; set; }

        public string? Redirect_Uri { get; set; }

        public string? Login_Hint { get; set; }

        public string? Lti_Message_Hint { get; set; }
    }
}
