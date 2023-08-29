using Lamar;
using MediatR;
using MediatR.NotificationPublishers;
using Xunit;

namespace WithoutMediatR.CustomNotificationPublisher.Without;

internal interface IPingNotificationHandler
{
    Task NotifyPing(string message);
}

public class Pong1 : IPingNotificationHandler
{
    internal static string WasInvoked;
    public Task NotifyPing(string message)
    {
        // do work
        WasInvoked = $"Pong1 was notified with {message}";
        return Task.CompletedTask;
    }
}

public class Pong2 : IPingNotificationHandler
{
    internal static string WasInvoked;
    public Task NotifyPing(string message)
    {
        // do work
        WasInvoked = $"Pong2 was notified with {message}";
        return Task.CompletedTask;
    }
}


internal interface IPingNotificationPublisher
{
    Task Ping(string message);
}

// ReSharper disable once ClassNeverInstantiated.Local
file class PingNotificationPublisher : IPingNotificationPublisher
{
    private readonly IEnumerable<IPingNotificationHandler> _handlers;

    public PingNotificationPublisher(IEnumerable<IPingNotificationHandler> handlers)
    {
        _handlers = handlers;
    }

    Task IPingNotificationPublisher.Ping(string message)
    {
        _handlers.ToList().ForEach(h => h.NotifyPing(message));

        return Task.CompletedTask;
    }
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
        var pingNotificationPublisher = _container.GetInstance<IPingNotificationPublisher>();

        await pingNotificationPublisher.Ping("some message");
        
        Assert.Equal("Pong1 was notified with some message", Pong1.WasInvoked);
        Assert.Equal("Pong2 was notified with some message", Pong2.WasInvoked);
    }
}
