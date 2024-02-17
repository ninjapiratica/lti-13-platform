namespace NP.Lti13Platform.Core
{
    public interface ILti13Message: ILti13Claim
    {
        static abstract string MessageType { get; }
    }
}
