using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.Behaviors;

public class Ping : IRequest<Pong> { }

public class Pong
{
    internal string Message { get; set; }
}

public class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> Handle(Ping request, CancellationToken cancellationToken) => 
        Task.FromResult(new Pong{Message = "pong"});
}

public interface ILogger<T>
{
    void LogInformation(string message);
    List<string> Messages { get; }
}

public class Logger<T> : ILogger<T>
{
    public List<string> Messages { get; } = new();
    
    public void LogInformation(string message)
    {
        Messages.Add(message);
    }
}

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling {typeof(TRequest).Name}");
        var response = await next();
        _logger.LogInformation($"Handled {typeof(TResponse).Name}");

        return response;
    }
}

public class With
{
    private readonly ILogger<LoggingBehavior<Ping, Pong>> _logger;
    private readonly Container _container;

    public With()
    {
        _logger = new Logger<LoggingBehavior<Ping, Pong>>();
        _container = new Container(cfg =>
        {
            cfg.Scan(scanner =>
            {
                scanner.AssemblyContainingType(typeof(With));
                scanner.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                scanner.WithDefaultConventions();
                scanner.AddAllTypesOf(typeof(IRequestHandler<>));
            });

            cfg.For<IPipelineBehavior<Ping, Pong>>().Add(new LoggingBehavior<Ping, Pong>(_logger));
            cfg.For<IMediator>().Use<Mediator>().Transient();
        });
    }

    [Fact]
    async Task request_response()
    {
        var mediator = _container.GetInstance<IMediator>();
        
        var response = await mediator.Send(new Ping());
        
        Assert.Equal("pong", response.Message);

        Assert.Equal(new []{"Handling Ping", "Handled Pong"}, _logger.Messages);
    }
}
