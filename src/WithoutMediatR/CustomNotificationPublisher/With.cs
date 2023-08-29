using Lamar;
using MediatR;
using MediatR.NotificationPublishers;
using Xunit;

namespace WithoutMediatR.CustomNotificationPublisher;

public record PingNotification(string Message) : INotification;

// ReSharper disable once ClassNeverInstantiated.Global
public class Pong1 : INotificationHandler<PingNotification>
{
    internal static string WasInvoked;
    public Task Handle(PingNotification request, CancellationToken cancellationToken)
    {
        // do work
        WasInvoked = $"Pong1 was notified with {request.Message}";
        return Task.CompletedTask;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Pong2 : INotificationHandler<PingNotification>
{
    internal static string WasInvoked;
    public Task Handle(PingNotification request, CancellationToken cancellationToken)
    {
        // do work
        WasInvoked = $"Pong2 was notified with {request.Message}";
        return Task.CompletedTask;
    }
}


class CustomNotificationPublisher : INotificationPublisher
{
    async Task INotificationPublisher.Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
    {
        foreach (var handler in handlerExecutors)
        {
            await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}



public class With : IDisposable
{
    private readonly Container _container;

    public With()
    {
        _container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(With));
                scanner.IncludeNamespaceContainingType<PingNotification>();
                scanner.WithDefaultConventions();
           
                scanner.AddAllTypesOf(typeof(INotificationHandler<>));
            });

            // INotificationPublisher a = new ForeachAwaitPublisher();
            // INotificationPublisher b = new TaskWhenAllPublisher();
            INotificationPublisher c = new CustomNotificationPublisher();
            
            cfg.For<IMediator>().Add(s => new Mediator(s, c));
        });
    }

    void IDisposable.Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    async void notification_test()
    {
        var mediator = _container.GetInstance<IMediator>();

        await mediator.Publish(new PingNotification(Message: "some message"));
        
        Assert.Equal("Pong1 was notified with some message", Pong1.WasInvoked);
        Assert.Equal("Pong2 was notified with some message", Pong2.WasInvoked);
    }
}
