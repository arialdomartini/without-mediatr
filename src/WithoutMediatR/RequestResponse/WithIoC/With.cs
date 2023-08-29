using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.RequestResponse.WithIoC;

public class Ping : IRequest<string>
{
}

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}

file class Controller
{
    private readonly IMediator _mediator;

    internal Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    internal async Task<string> SomeMethod() => 
        await _mediator.Send(new Ping());
}

public class With
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
            
            cfg.For<Controller>().Use<Controller>();
            cfg.For<IMediator>().Use<Mediator>();
        });
    }

    [Fact]
    async Task ping_request_response_through_a_controller()
    {
        var controller = _container.GetInstance<Controller>();

        var response = await controller.SomeMethod();

        Assert.Equal("Pong", response);
    }
    
    [Fact]
    async Task ping_request_response_directly_invoking_mediatr()
    {
        var mediator = _container.GetInstance<IMediator>();

        var response = await mediator.Send(new Ping());

        Assert.Equal("Pong", response);
    }
}
