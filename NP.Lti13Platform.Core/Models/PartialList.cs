namespace NP.Lti13Platform.Core.Models
{
    public class PartialList<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }

        public static PartialList<T> Empty => new() { Items = [], TotalItems = 0 };
    }

}
