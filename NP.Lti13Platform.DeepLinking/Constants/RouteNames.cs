namespace NP.Lti13Platform.DeepLinking.Constants;

/// <summary>
/// Route names need to be globally unique. To avoid the possibility of overlapping with other endpoints (outside of this library), the route names are made globally unique.
/// </summary>
internal static class RouteNames
{
    /// <summary>
    /// Route name for the deep linking response endpoint as defined in the IMS Global LTI Deep Linking specification.
    /// </summary>
    internal static readonly string DEEP_LINKING_RESPONSE = "cca8466e-eb1f-4001-b290-0606c90f9f22";
}
