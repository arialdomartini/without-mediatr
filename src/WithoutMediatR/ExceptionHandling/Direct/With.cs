using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MediatR;
using MediatR.Pipeline;

namespace WithoutMediatR.ExceptionHandling.Direct;

public class Ping : IRequest<Pong>
{
    public string Message { get; set; }
}

public class PingResource : Ping
{
}

public class PingProtectedResource : PingResource
{
}

public class Pong
{
}

public class ConnectionException : Exception
{
}

public class ForbiddenException : ConnectionException
{
}

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
    private readonly Mediator _mediator;
    private readonly StringWriter _stringWriter;
    
    public With()
    {
        _stringWriter = new StringWriter();
        
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton<TextWriter>(_stringWriter)
                .AddTransient<IRequestHandler<PingProtectedResource, Pong>, PingProtectedResourceHandler>()
                .AddScoped(typeof(IRequestExceptionHandler<PingProtectedResource, Pong, ForbiddenException>), typeof(AccessDeniedExceptionHandler))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestExceptionProcessorBehavior<,>))

                .BuildServiceProvider();

        _mediator = new Mediator(serviceProvider);
    }

    [Fact]
    async Task ping_request_response()
    {
        var response = await _mediator.Send(new PingProtectedResource());

        Assert.Equal("---- Exception Handler: 'WithoutMediatR.ExceptionHandling.Direct.AccessDeniedExceptionHandler'\r\n", _stringWriter.ToString());
    }
}
