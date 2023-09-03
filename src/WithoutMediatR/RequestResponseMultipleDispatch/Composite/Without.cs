using Xunit;

namespace WithoutMediatR.RequestResponseMultipleDispatch.Decorator;

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

file class Handlers : IMyHandler
{
    private readonly IEnumerable<IMyHandler> _handlers;

    internal Handlers(IEnumerable<IMyHandler> handlers)
    {
        _handlers = handlers;
    }
    
    Task IMyHandler.DoSomething(SomeRequest request)
    {
        var dispatchAll = _handlers.Select(h => h.DoSomething(request));
        Task.WhenAll(dispatchAll).Wait();
        return Task.CompletedTask;
    }
}

file class Client
{
    private readonly IMyHandler _handler;

    internal Client(IMyHandler handler)
    {
        _handler = handler;
    }

    internal async Task DispatchToAll()
    {
        var message = new SomeRequest("my message");

        await _handler.DoSomething(message);
    }
}

public class Without
{
    [Fact]
    async void dispatch_all_requests()
    {
        var handlers = new Handlers(
            new IMyHandler[]{ new HandlerA(), new HandlerB()});
        
        var client = new Client(handlers);
        
        await client.DispatchToAll();
        
        Assert.True(HandlerA.HasBeenInvoked);
        Assert.True(HandlerB.HasBeenInvoked);
    }
}
