using Lamar;
using Xunit;

namespace WithoutMediatR.ExceptionHandling.WithIoC.Without;

public class Ping
{
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

public interface IPingProtectedResourceHandler
{
    Task<Pong> Handle(PingProtectedResource request);
}

public class PingProtectedResourceHandler : IPingProtectedResourceHandler
{
    Task<Pong> IPingProtectedResourceHandler.Handle(PingProtectedResource request) =>
        throw new ForbiddenException();
}

public class AccessDeniedExceptionHandler : IPingProtectedResourceHandler
{
    private readonly TextWriter _writer;
    private readonly IPingProtectedResourceHandler _handler;

    public AccessDeniedExceptionHandler(IPingProtectedResourceHandler handler, TextWriter writer)
    {
        _handler = handler;
        _writer = writer;
    }

    async Task<Pong> IPingProtectedResourceHandler.Handle(PingProtectedResource request)
    {
        try
        {
            return await _handler.Handle(request);
        }
        catch (Exception e)
        {
            await _writer.WriteLineAsync($"---- Exception Handler: '{typeof(AccessDeniedExceptionHandler).FullName}'").ConfigureAwait(false);
            return new Pong();
        }
    }
}

public class Without
{
    private readonly Container _container;
    private readonly StringWriter _stringWriter;

    public Without()
    {
        _stringWriter = new StringWriter();
        _container = new Container(cfg =>
        {
            cfg.For<TextWriter>().Add(_stringWriter);
            cfg.For<IPingProtectedResourceHandler>().Use<PingProtectedResourceHandler>();
            cfg.For<IPingProtectedResourceHandler>().DecorateAllWith<AccessDeniedExceptionHandler>();
        });
    }

    [Fact]
    async Task exceptions_are_handled()
    {
        var handler = _container.GetInstance<IPingProtectedResourceHandler>();

        await handler.Handle(new PingProtectedResource());

        Assert.Equal("---- Exception Handler: 'WithoutMediatR.ExceptionHandling.WithIoC.Without.AccessDeniedExceptionHandler'\r\n", _stringWriter.ToString());
    }

    [Fact]
    async Task not_over_engineered()
    {
        async Task<Pong> JustHandleExceptions(IPingProtectedResourceHandler pingProtectedResourceHandler)
        {
            try
            {
                return await pingProtectedResourceHandler.Handle(new PingProtectedResource());
            }
            catch (ConnectionException e)
            {
                return new Pong();
            }
        }

        var handler = new PingProtectedResourceHandler();

        var result = await JustHandleExceptions(handler);

        Assert.NotNull(result);
    }

    [Fact]
    async Task with_an_extension_method()
    {
        IPingProtectedResourceHandler handler = new PingProtectedResourceHandler();

        var result = await Try(() => 
            handler.Handle(new PingProtectedResource()), 
            new Pong());

        Assert.NotNull(result);
    }

    private static Task<T> Try<T>(Func<Task<T>> f, T fallback)
    {
        try
        {
            return f();
        }
        catch (Exception e)
        {
            return Task.FromResult(fallback);
        }
    }
}
