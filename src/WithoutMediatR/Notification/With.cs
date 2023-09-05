using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.Notification;

public record PingNotification : INotification;

// ReSharper disable once ClassNeverInstantiated.Global
public class Pong1 : INotificationHandler<PingNotification>
{
    internal static string WasInvoked;
    public Task Handle(PingNotification request, CancellationToken cancellationToken)
    {
        // do work
        WasInvoked = "Pong 1";
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
        WasInvoked = "Pong 2";
        return Task.CompletedTask;
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

            cfg.For<IMediator>().Use<Mediator>();
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

        await mediator.Publish(new PingNotification());
        
        Assert.Equal("Pong 1", Pong1.WasInvoked);
        Assert.Equal("Pong 2", Pong2.WasInvoked);
    }
    
    [Fact]
    void sending_a_notifications_fails_at_runtime()
    {
        var mediator = _container.GetInstance<IMediator>();

        Assert.ThrowsAsync<Exception>(async () => await mediator.Send(new PingNotification()));
    }
}
