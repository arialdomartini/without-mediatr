using Lamar;
using WithoutMediatR.RequestResponse.WithIoC;
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
{it s
    internal static string WasInvoked;

    Task IPingNotificationHandler.NotifyPing()
    {
        // do work
        WasInvoked = "Pong 2";
        return Task.CompletedTask;
    }
}

file class PingNotificationPublisher : IPingNotificationHandler
{
    private readonly IEnumerable<IPingNotificationHandler> _handlers;

    public PingNotificationPublisher(IEnumerable<IPingNotificationHandler> handlers)
    {
        _handlers = handlers;
    }

    Task IPingNotificationHandler.NotifyPing()
    {
        _handlers.ToList().ForEach(h => h.NotifyPing());

        return Task.CompletedTask;
    }
}

file class Client
{
    private readonly IPingNotificationHandler _handler;

    internal Client(IPingNotificationHandler handler)
    {
        _handler = handler;
    }

    internal void DoWork()
    {
        _handler.NotifyPing();
    }
}

public class Without
{
    [Fact]
    void notification_test()
    {
        IPingNotificationHandler[] handlers =
        {
            new Pong1(), 
            new Pong2()
        };
        
        var client = new Client(new PingNotificationPublisher(handlers));

        client.DoWork();
        
        Assert.Equal("Pong 1", Pong1.WasInvoked);
        Assert.Equal("Pong 2", Pong2.WasInvoked);
    }
}
