namespace NP.Lti13Platform.Core.Populators
{
    public abstract class Populator
    {
        public abstract Task PopulateAsync(object obj, Lti13MessageScope scope);
    }

    public abstract class Populator<T> : Populator
    {
        public abstract Task PopulateAsync(T obj, Lti13MessageScope scope);

        public override async Task PopulateAsync(object obj, Lti13MessageScope scope)
        {
            await PopulateAsync((T)obj, scope);
        }
    }
}
