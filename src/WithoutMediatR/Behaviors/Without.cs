using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.Behaviors;

file class Ping : IRequest<Pong>
{
}

file class Pong
{
    internal string Message { get; set; }
}

file interface IPingHandler
{
    Task<Pong> Handle(Ping request);
}

file class PingHandler : IPingHandler
{
    Task<Pong> IPingHandler.Handle(Ping request) =>
        Task.FromResult(new Pong { Message = "pong" });
}

internal interface ILogger
{
    void LogInformation(string message);
    List<string> Messages { get; }
}

file class Logger : ILogger
{
    List<string> ILogger.Messages { get; } = new();

    public void LogInformation(string message)
    {
        ((ILogger)this).Messages.Add(message);
    }
}

file class LoggingBehavior : IPingHandler
{
    private readonly ILogger _logger;
    private readonly IPingHandler _next;

    public LoggingBehavior(ILogger logger, IPingHandler next)
    {
        _logger = logger;
        _next = next;
    }

    async Task<Pong> IPingHandler.Handle(Ping request)
    {
        _logger.LogInformation($"Handling Ping");
        var response = await _next.Handle(request);
        _logger.LogInformation($"Handled Pong");

        return response;
    }
}

public class Without
{
    private readonly ILogger _logger;
    private readonly Container _container;

    public Without()
    {
        _logger = new Logger();
        _container = new Container(cfg =>
        {
            cfg.For<ILogger>().Add(_logger);
            cfg.For<IPingHandler>().Add(new PingHandler());
            cfg.For<IPingHandler>().DecorateAllWith<LoggingBehavior>();
            
            // otherwise
            // cfg.For<IPingHandler>().Add(new LoggingBehavior(_logger, new PingHandler()));

        });
    }

    [Fact]
    async Task request_response()
    {
        var pingHandler = _container.GetInstance<IPingHandler>();

        var response = await pingHandler.Handle(new Ping());

        Assert.Equal("pong", response.Message);

        Assert.Equal(new[] { "Handling Ping", "Handled Pong" }, _logger.Messages);
    }
}
