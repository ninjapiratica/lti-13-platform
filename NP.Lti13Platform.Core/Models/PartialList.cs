namespace NP.Lti13Platform.Core.Models;

/// <summary>
/// Represents a partial list of items, typically used for pagination.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PartialList<T>
{
    /// <summary>
    /// Gets or sets the items in the partial list.
    /// </summary>
    public required IEnumerable<T> Items { get; set; }

    /// <summary>
    /// Gets or sets the total number of items available.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets an empty partial list.
    /// </summary>
    public static PartialList<T> Empty => new() { Items = [], TotalItems = 0 };
}
