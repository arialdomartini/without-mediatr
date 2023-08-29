using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.OneWay;

public class OneWay : IRequest { }

// ReSharper disable once ClassNeverInstantiated.Global
public class OneWayHandler : IRequestHandler<OneWay>
{
    internal static bool WasInvoked;
    public Task Handle(OneWay request, CancellationToken cancellationToken)
    {
        // do work
        WasInvoked = true;
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
                scanner.IncludeNamespaceContainingType<OneWay>();
                scanner.WithDefaultConventions();
           
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
            });

            cfg.For<IMediator>().Use<Mediator>();
        });
    }

    void IDisposable.Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    async void one_way_test()
    {
        var mediator = _container.GetInstance<IMediator>();

        await mediator.Send(new OneWay());
        
        Assert.Equal(true, OneWayHandler.WasInvoked);
    }
}
