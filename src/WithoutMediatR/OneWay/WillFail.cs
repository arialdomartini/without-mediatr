using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.OneWay;

file class OneWay : IRequest { }

// ReSharper disable once ClassNeverInstantiated.Global
file class OneWayHandler : IRequestHandler<OneWay>
{
    internal static bool WasInvoked;
    public Task Handle(OneWay request, CancellationToken cancellationToken)
    {
        // do work
        WasInvoked = true;
        return Task.CompletedTask;
    }
}


public class WillFail
{
    private readonly Container _container;

    public WillFail()
    {
        _container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(WillFail));
                scanner.IncludeNamespaceContainingType<OneWay>();
                scanner.WithDefaultConventions();
           
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });

            cfg.For<IMediator>().Use<Mediator>();
        });

    }
    
    [Fact]
    void it_fails()
    {
        var mediator = _container.GetInstance<IMediator>();

        Assert.ThrowsAsync<Exception>(async () => await mediator.Send(new OneWay()));
    }
}
