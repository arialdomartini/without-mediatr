using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MediatR;

namespace WithoutMediatR.RequestResponse;

file class Ping : IRequest<string>
{
}

file class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}

public class With
{
    private readonly Mediator _mediator;

    public With()
    {
        var serviceProvider =
                new ServiceCollection()
                    .AddTransient<IRequestHandler<Ping, string>, PingHandler>()
                    .BuildServiceProvider();

        _mediator = new Mediator(serviceProvider);
    }

    [Fact]
    async Task ping_request_response()
    {
        var response = await _mediator.Send(new Ping());

        Assert.Equal("Pong", response);
    }
}
