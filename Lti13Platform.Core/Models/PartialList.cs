namespace NP.Lti13Platform.Core.Models
{
    public class PartialList<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
    }

}
