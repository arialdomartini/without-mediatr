using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.OneWay;

file interface IOneWayHandler
{
    Task Handle();
}

file class OneWayHandler : IOneWayHandler
{
    internal static bool WasInvoked;

    Task IOneWayHandler.Handle()
    {
        // do work
        WasInvoked = true;
        return Task.CompletedTask;
    }
}

public class Without
{
    private readonly Container _container;

    public Without()
    {
        _container = new Container(cfg =>
        {
            cfg.For<IOneWayHandler>().Add(new OneWayHandler());
        });

    }
    
    [Fact]
    async void one_way_test()
    {
        var handler = _container.GetInstance<IOneWayHandler>();

        await handler.Handle();
        
        Assert.Equal(true, OneWayHandler.WasInvoked);
    }
}
