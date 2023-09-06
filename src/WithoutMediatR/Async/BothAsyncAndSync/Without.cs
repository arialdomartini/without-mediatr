using Lamar;
using Xunit;

namespace WithoutMediatR.Async.BothAsyncAndSync;

file class Ping { }
file class Pong { }

file interface IPingHandler
{
    Task<Pong> HandleAsync(Ping request);
    Pong Handle(Ping request);
}

file class PingHandler : IPingHandler
{
    internal static string WasInvoked;

    async Task<Pong> IPingHandler.HandleAsync(Ping request)
    {
        WasInvoked = "Pong async";
        return await Task.FromResult(new Pong());
    }
    
    Pong IPingHandler.Handle(Ping request)
    {
        WasInvoked = "Pong sync";
        return new Pong();
    }
}

public class Without : IDisposable
{
    private readonly Container _container;

    public Without()
    {
        _container = new Container(cfg =>
        {
            cfg.For<IPingHandler>().Add(new PingHandler());
        });
    }

    void IDisposable.Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    async void invoke_asynchronously()
    {
        var pingHandler = _container.GetInstance<IPingHandler>();

        await pingHandler.HandleAsync(new Ping());
        
        Assert.Equal("Pong async", PingHandler.WasInvoked);
    }
    
    [Fact]
    void invoke_synchronously()
    {
        var pingHandler = _container.GetInstance<IPingHandler>();

        pingHandler.Handle(new Ping());
        
        Assert.Equal("Pong sync", PingHandler.WasInvoked);
    }
}
