namespace NP.Lti13Platform.Core
{
    public interface ILti13Claim
    {
        IEnumerable<(string Key, object Value)> GetClaims();
    }
}
