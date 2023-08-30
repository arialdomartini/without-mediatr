using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.PolymorphicDispatch;

file record MyNotification { }
file record FooNotification : MyNotification { }
file record BarNotification : MyNotification { }


file interface INotificationHandler
{
    Task NotifyPing(in MyNotification notification);
}

file class CatchAll : INotificationHandler
{
    internal static string WasInvoked;

    Task INotificationHandler.NotifyPing(in MyNotification notification)
    {
        // do work
        WasInvoked = notification.GetType().ToString();
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
            cfg.For<INotificationHandler>().Add(new CatchAll());
        });
    }

    void IDisposable.Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    async void CatchAll_receives_all_notifications()
    {
        var handler = _container.GetInstance<INotificationHandler>();

        await handler.NotifyPing(new FooNotification());
        Assert.Contains("FooNotification", CatchAll.WasInvoked);
        
        await handler.NotifyPing(new BarNotification());
        Assert.Contains("BarNotification", CatchAll.WasInvoked);
    }
}
