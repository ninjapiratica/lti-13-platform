namespace NP.Lti13Platform.Core.Populators
{
    public abstract class Populator
    {
        public abstract Task PopulateAsync(object obj, MessageScope scope, CancellationToken cancellationToken = default);
    }

    public abstract class Populator<T> : Populator
    {
        public abstract Task PopulateAsync(T obj, MessageScope scope, CancellationToken cancellationToken = default);

        public override async Task PopulateAsync(object obj, MessageScope scope, CancellationToken cancellationToken = default)
        {
            await PopulateAsync((T)obj, scope, cancellationToken);
        }
    }
}
