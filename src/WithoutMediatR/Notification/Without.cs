using Lamar;
using Xunit;

namespace WithoutMediatR.Notification.Without;

internal interface IPingNotificationHandler
{
    Task NotifyPing();
}

file class Pong1 : IPingNotificationHandler
{
    internal static string WasInvoked;

    Task IPingNotificationHandler.NotifyPing()
    {
        // do work
        WasInvoked = "Pong 1";
        return Task.CompletedTask;
    }
}

file class Pong2 : IPingNotificationHandler
{
    internal static string WasInvoked;

    Task IPingNotificationHandler.NotifyPing()
    {
        // do work
        WasInvoked = "Pong 2";
        return Task.CompletedTask;
    }
}

// ReSharper disable once ClassNeverInstantiated.Local
file class PingNotificationPublisher : IPingNotificationPublisher
{
    private readonly IEnumerable<IPingNotificationHandler> _handlers;

    public PingNotificationPublisher(IEnumerable<IPingNotificationHandler> handlers)
    {
        _handlers = handlers;
    }

    Task IPingNotificationPublisher.Ping()
    {
        _handlers.ToList().ForEach(h => h.NotifyPing());

        return Task.CompletedTask;
    }
}

internal interface IPingNotificationPublisher
{
    Task Ping();
}

public class Without : IDisposable
{
    private readonly Container _container;

    public Without()
    {
        _container = new Container(cfg =>
        {
            cfg.For<IPingNotificationHandler>().Add(new Pong1());
            cfg.For<IPingNotificationHandler>().Add(new Pong2());
            
            cfg.For<IPingNotificationPublisher>().Use<PingNotificationPublisher>();
        });
    }

    void IDisposable.Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    async void notification_test()
    {
        var handler = _container.GetInstance<IPingNotificationPublisher>();

        await handler.Ping();
        
        Assert.Equal("Pong 1", Pong1.WasInvoked);
        Assert.Equal("Pong 2", Pong2.WasInvoked);
    }
}
