namespace NP.Lti13Platform.Core
{
    public interface ILti13Claim
    {
        IDictionary<string, object> GetClaims();
    }
}
