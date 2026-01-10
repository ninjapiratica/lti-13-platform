using NP.Lti13Platform.Core.Models;
using System.Text.Encodings.Web;
using System.Web;

namespace NP.Lti13Platform.Core.Services;

/// <summary>
/// Represents the data required to initiate an LTI (Learning Tools Interoperability) launch flow.
/// </summary>
/// <remarks>This record encapsulates the parameters necessary for initiating an OpenID Connect (OIDC) login flow
/// as part of an LTI launch. It includes details such as the tool configuration, issuer, target link URI, client ID,
/// deployment ID, and hints for login and LTI message handling.</remarks>
/// <param name="Tool">The tool configuration to launch. This contains the tool's OIDC initiation URL, launch URL, client identifier, and other metadata required to initiate an LTI launch.</param>
/// <param name="Issuer">The platform issuer URI. This value is used as the 'iss' parameter in the OIDC authentication request to identify the platform to the tool.</param>
/// <param name="TargetLinkUri">The target link URI. This is the destination within the tool that the platform requests the user be directed to after a successful OIDC login (the 'target_link_uri' parameter).</param>
/// <param name="DeploymentId">The deployment identifier for the platform-tool installation. This value identifies the specific integration/installation and is sent as 'lti_deployment_id'.</param>
/// <param name="LoginHint">The computed login hint. This value is transmitted as the 'login_hint' parameter to correlate the login to a platform user (it may encode impersonation and anonymity flags).</param>
/// <param name="LtiMessageHint">The LTI message hint. This value is sent as 'lti_message_hint' to convey the LTI message context (message type, deployment, context, resource link, and optional message hint) to the tool.</param>
public record Lti13Launch(Tool Tool, Uri Issuer, Uri TargetLinkUri, DeploymentId DeploymentId, string LoginHint, string LtiMessageHint)
{
    /// <summary>
    /// Constructs a URI with query parameters required for OIDC initiation.
    /// </summary>
    /// <remarks>The resulting URI includes the base OIDC initiation URL and appends query parameters such as 
    /// issuer, login hint, target link URI, client ID, LTI message hint, and LTI deployment ID.  This method is
    /// typically used to initiate an OpenID Connect (OIDC) login flow.</remarks>
    /// <returns>A <see cref="Uri"/> representing the OIDC initiation URL with the required query parameters.</returns>
    public Uri AsUri()
    {
        var builder = new UriBuilder(Tool.OidcInitiationUrl);

        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Add("iss", Issuer.OriginalString);
        query.Add("login_hint", LoginHint);
        query.Add("target_link_uri", TargetLinkUri.OriginalString);
        query.Add("client_id", Tool.ClientId.ToString());
        query.Add("lti_message_hint", LtiMessageHint);
        query.Add("lti_deployment_id", DeploymentId.ToString());
        builder.Query = query.ToString();

        return builder.Uri;
    }

    /// <summary>
    /// Generates an HTML form string for initiating an OpenID Connect (OIDC) login flow.
    /// </summary>
    /// <remarks>The generated form is intended to be used in scenarios where an OIDC login flow needs to be
    /// initiated via a POST request. The form includes a `noscript` block with a submit button to ensure
    /// functionality in environments where JavaScript is disabled.</remarks>
    /// <param name="formId">The ID to assign to the generated HTML form. This value is HTML-encoded to ensure safety. Should be used for submitting the form via javascript.</param>
    /// <returns>A string containing the HTML representation of a form configured for OIDC login initiation. The form includes
    /// hidden input fields for required parameters such as issuer, login hint, and client ID.</returns>
    public string AsForm(string formId) => $@"
<form id=""{HtmlEncoder.Default.Encode(formId)}"" action=""{HtmlEncoder.Default.Encode(Tool.OidcInitiationUrl.OriginalString)}"" method=""post"">
  <input type=""hidden"" name=""iss"" value=""{HtmlEncoder.Default.Encode(Issuer.OriginalString)}"" />
  <input type=""hidden"" name=""login_hint"" value=""{HtmlEncoder.Default.Encode(LoginHint)}"" />
  <input type=""hidden"" name=""target_link_uri"" value=""{HtmlEncoder.Default.Encode(TargetLinkUri.OriginalString)}"" />
  <input type=""hidden"" name=""client_id"" value=""{HtmlEncoder.Default.Encode(Tool.ClientId.ToString())}"" />
  <input type=""hidden"" name=""lti_message_hint"" value=""{HtmlEncoder.Default.Encode(LtiMessageHint)}"" />
  <input type=""hidden"" name=""lti_deployment_id"" value=""{HtmlEncoder.Default.Encode(DeploymentId.ToString())}"" />
  <noscript><button type=""submit"">Continue</button></noscript>
</form>".Trim();
}