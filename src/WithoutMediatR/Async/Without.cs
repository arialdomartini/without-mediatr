using Lamar;
using Xunit;

namespace WithoutMediatR.Async;

file class Ping { }
file class Pong { }

file interface IPingHandler
{
    Task<Pong> Handle(Ping request);
}

file class PingHandler : IPingHandler
{
    internal static string WasInvoked;

    async Task<Pong> IPingHandler.Handle(Ping request)
    {
        WasInvoked = "Pong";
        return await DoPong();
    }

    private async Task<Pong> DoPong() => await Task.FromResult(new Pong());
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
    async void notification_test()
    {
        var pingHandler = _container.GetInstance<IPingHandler>();

        await pingHandler.Handle(new Ping());
        
        Assert.Equal("Pong", PingHandler.WasInvoked);
    }
}
