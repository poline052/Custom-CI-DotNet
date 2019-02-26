namespace Com.CI.Infrastructure
{
    public interface ICommandHandler<Command, Response>
    {
        Response Handle(Command command);
    }
}
