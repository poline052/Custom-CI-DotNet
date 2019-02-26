namespace Com.CI.Infrastructure
{
    public interface IDataMapper
    {
        Target Map<Target, Source>(Source source);
    }
}
