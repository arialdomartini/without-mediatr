using Xunit;

namespace WithoutMediatR.RequestResponseMultipleDispatch.Collection;

file record SomeRequest(string Message);

file interface IMyHandler
{
    Task DoSomething(SomeRequest request);
}

file class HandlerA : IMyHandler
{
    internal static bool HasBeenInvoked { get; private set; }

    Task IMyHandler.DoSomething(SomeRequest request)
    {
        // do work
        HasBeenInvoked = true;
        return Task.CompletedTask;
    }
}

file class HandlerB : IMyHandler
{
    internal static bool HasBeenInvoked { get; private set; }

    Task IMyHandler.DoSomething(SomeRequest request)
    {
        // do work
        HasBeenInvoked = true;
        return Task.CompletedTask;
    }
}

file class Client
{
    private readonly IEnumerable<IMyHandler> _handlers;

    internal Client(IEnumerable<IMyHandler> handlers)
    {
        _handlers = handlers;
    }

    internal void DispatchToAll()
    {
        var message = new SomeRequest("my message");

        var dispatchAll = _handlers.Select(h => h.DoSomething(message));

        Task.WhenAll(dispatchAll).Wait();
    }
}

public class Without
{
    [Fact]
    void dispatch_all_requests()
    {
        var handlers = new IMyHandler[]{ new HandlerA(), new HandlerB()};
        var client = new Client(handlers);
        
        client.DispatchToAll();
        
        Assert.True(HandlerA.HasBeenInvoked);
        Assert.True(HandlerB.HasBeenInvoked);
    }
}
