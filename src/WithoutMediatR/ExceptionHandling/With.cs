using Lamar;
using MediatR;
using MediatR.Pipeline;
using Xunit;

namespace WithoutMediatR.ExceptionHandling;

public class Ping : IRequest<Pong>
{
    public string Message { get; set; }
}

public class PingResource : Ping { }

public class PingProtectedResource : PingResource
{
}

public class Pong{}

public class ConnectionException : Exception { }

public class ForbiddenException : ConnectionException { }

// ReSharper disable once UnusedType.Global
public class PingProtectedResourceHandler : IRequestHandler<PingProtectedResource, Pong>
{
    private readonly TextWriter _writer;

    public PingProtectedResourceHandler(TextWriter writer)
    {
        _writer = writer;
    }

    public Task<Pong> Handle(PingProtectedResource request, CancellationToken cancellationToken)
    {
        throw new ForbiddenException();
    }
}

public class AccessDeniedExceptionHandler : IRequestExceptionHandler<PingResource, Pong, ForbiddenException>
{
    private readonly TextWriter _writer;

    public AccessDeniedExceptionHandler(TextWriter writer) => _writer = writer;

    public async Task Handle(PingResource request,
        ForbiddenException exception,
        RequestExceptionHandlerState<Pong> state,
        CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(AccessDeniedExceptionHandler).FullName}'").ConfigureAwait(false);
        state.SetHandled(new Pong());
    }
}

public class With
{
    private readonly Container _container;
    private readonly StringWriter _stringWriter;

    public With()
    {
        _stringWriter = new StringWriter();
        _container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(With));
                scanner.IncludeNamespaceContainingType<PingProtectedResource>();
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<,>));
                
                scanner.ConnectImplementationsToTypesClosing(typeof(IRequestExceptionHandler<,,>));
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(RequestExceptionProcessorBehavior<,>));
            });
            
            cfg.For<IMediator>().Use<Mediator>();
            cfg.For<TextWriter>().Add(_stringWriter);
        });
    }
    
    [Fact]
    async Task exceptions_are_handled()
    {
        var mediator = _container.GetInstance<IMediator>();

        var response = await mediator.Send(new PingProtectedResource());

        Assert.Equal("---- Exception Handler: 'WithoutMediatR.ExceptionHandling.AccessDeniedExceptionHandler'\r\n", _stringWriter.ToString());
        Assert.NotNull(response);
    }
}
