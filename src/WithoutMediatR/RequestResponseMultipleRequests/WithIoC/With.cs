using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.RequestResponseMultipleRequests.WithIoC;

public class Ping : IRequest<string>
{
}

public record Echo(string Message) : IRequest<string>;

public class PingHandler :
    IRequestHandler<Ping, string>,
    IRequestHandler<Echo, string>

{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }

    public Task<string> Handle(Echo request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Message);
    }
}

public class Controller
{
    private readonly IMediator _mediator;

    public Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    internal async Task<string> SomeMethod() => 
        await _mediator.Send(new Ping());
    
    internal async Task<string> AnotherMethod(string message) => 
        await _mediator.Send(new Echo(message));
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
    async Task request_response_through_a_controller()
    {
        var controller = _container.GetInstance<Controller>();

        var pingResponse = await controller.SomeMethod();
        Assert.Equal("Pong", pingResponse);

        var echoResponse = await controller.AnotherMethod("some message");
        Assert.Equal("some message", echoResponse);
    }
    
    [Fact]
    async Task request_response_directly_invoking_mediatr()
    {
        var mediator = _container.GetInstance<IMediator>();

        var pingResponse = await mediator.Send(new Ping());
        Assert.Equal("Pong", pingResponse);
        
        var echoResponse = await mediator.Send(new Echo("some message"));
        Assert.Equal("some message", echoResponse);
    }
}
