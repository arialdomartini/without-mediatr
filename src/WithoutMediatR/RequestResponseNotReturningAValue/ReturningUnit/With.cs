using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.RequestResponseNotReturningAValue.ReturningUnit;

file class Ping : IRequest<Unit> { }

file class PingHandler : IRequestHandler<Ping, Unit>
{
    internal static bool HasBeenCalled { get; set; }
    
    public Task<Unit> Handle(Ping request, CancellationToken cancellationToken)
    {
        HasBeenCalled = true;
        return Task.FromResult(Unit.Value);
    }
}

public class With
{
    private readonly Mediator _mediator;

    public With()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddTransient<IRequestHandler<Ping, Unit>, PingHandler>()
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
