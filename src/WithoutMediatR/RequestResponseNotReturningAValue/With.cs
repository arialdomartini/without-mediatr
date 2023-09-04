using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.RequestResponseNotReturningAValue;

file class Ping : IRequest { }

file class PingHandler : IRequestHandler<Ping>
{
    internal static bool HasBeenCalled { get; set; }
    
    public Task Handle(Ping request, CancellationToken cancellationToken)
    {
        HasBeenCalled = true;
        return Task.CompletedTask;
    }
}

public class With
{
    private readonly Mediator _mediator;

    public With()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddTransient<IRequestHandler<Ping>, PingHandler>()
                .BuildServiceProvider();

        _mediator = new Mediator(serviceProvider);
    }

    [Fact]
    async Task ping_request_response()
    {
        await _mediator.Send(new Ping());

        Assert.True(PingHandler.HasBeenCalled);
    }
}
