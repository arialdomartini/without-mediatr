using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.Async;

public class Ping : IRequest<Pong> { }
public class Pong { }

// ReSharper disable once ClassNeverInstantiated.Global
public class PingHandler : IRequestHandler<Ping, Pong>
{
    internal static string WasInvoked;
    public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        WasInvoked = "Pong";
        return await DoPong();
    }

    private async Task<Pong> DoPong() => await Task.FromResult(new Pong());
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
                scanner.IncludeNamespaceContainingType<Ping>();
                scanner.WithDefaultConventions();
           
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
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

        await mediator.Send(new Ping());
        
        Assert.Equal("Pong", PingHandler.WasInvoked);
    }
}
