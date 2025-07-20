namespace NP.Lti13Platform.Core.Populators
{
    /// <summary>
    /// Base class for populators.
    /// </summary>
    public abstract class Populator
    {
        /// <summary>
        /// Populates the specified object.
        /// </summary>
        /// <param name="obj">The object to populate.</param>
        /// <param name="scope">The message scope.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task PopulateAsync(object obj, MessageScope scope, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Base class for typed populators.
    /// </summary>
    /// <typeparam name="T">The type of object to populate.</typeparam>
    public abstract class Populator<T> : Populator
    {
        /// <summary>
        /// Populates the specified object.
        /// </summary>
        /// <param name="obj">The object to populate.</param>
        /// <param name="scope">The message scope.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task PopulateAsync(T obj, MessageScope scope, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public override async Task PopulateAsync(object obj, MessageScope scope, CancellationToken cancellationToken = default)
        {
            await PopulateAsync((T)obj, scope, cancellationToken);
        }
    }
}
