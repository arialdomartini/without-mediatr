using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.RequestResponseChain;

public record Echo(string Message) : IRequest<string>;

public class WillBeIgnored : IRequestHandler<Echo, string>
{
    public Task<string> Handle(Echo request, CancellationToken cancellationToken) => Task.FromResult(request.Message);
}

public class PingHandler : IRequestHandler<Echo, string>
{
    public Task<string> Handle(Echo request, CancellationToken cancellationToken) => Task.FromResult("doh!");
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
                scanner.IncludeNamespaceContainingType<Echo>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
            });
            
            cfg.For<IMediator>().Use<Mediator>();
        });
    }
    
    [Fact]
    async Task request_response_directly_invoking_mediatr()
    {
        var mediator = _container.GetInstance<IMediator>();
        
        var echoResponse = await mediator.Send(new Echo("some message"));
        
        Assert.Equal("doh!", echoResponse);
    }
}
