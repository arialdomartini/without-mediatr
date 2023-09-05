using Lamar;
using MediatR;
using Xunit;

namespace WithoutMediatR.RequestResponseSubtyping;

public class SubtypeOfPing : Ping { };

public class Ping : IRequest<string>
{
    public string Message { get; set; }
};

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handler received {request.Message}");
    }
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

            cfg.For<IMediator>().Use<Mediator>();
        });
    }

    [Fact]
    async Task subtypes_of_ping_request_are_delivered()
    {
        var mediator = _container.GetInstance<IMediator>();

        var response = await mediator.Send(new SubtypeOfPing { Message = "some message" });

        Assert.Equal("Handler received some message", response);
    }
}
