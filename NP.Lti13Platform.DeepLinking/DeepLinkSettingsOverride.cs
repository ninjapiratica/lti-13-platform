namespace NP.Lti13Platform.DeepLinking
{
    public record DeepLinkSettingsOverride(string? DeepLinkReturnUrl, IEnumerable<string>? AcceptTypes, IEnumerable<string>? AcceptPresentationDocumentTargets, IEnumerable<string>? AcceptMediaTypes, bool? AcceptMultiple, bool? AcceptLineItem, bool? AutoCreate, string? Title, string? Text, string? Data);
}
