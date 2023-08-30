using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.PolymorphicDispatch;

public class FooNotification : INotification { }
public class BarNotification : INotification { }

// ReSharper disable once ClassNeverInstantiated.Global
public class CatchAll : INotificationHandler<INotification>
{
    internal static string WasInvoked;
    public Task Handle(INotification request, CancellationToken cancellationToken)
    {
        // do work
        WasInvoked = request.GetType().ToString();
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
                scanner.IncludeNamespaceContainingType<FooNotification>();
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
    async void CatchAll_receives_all_notifications()
    {
        var mediator = _container.GetInstance<IMediator>();

        await mediator.Publish(new FooNotification());
        Assert.Contains("FooNotification", CatchAll.WasInvoked);

        await mediator.Publish(new BarNotification());
        Assert.Contains("BarNotification", CatchAll.WasInvoked);
    }
}
