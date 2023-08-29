using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.RequestResponseMultiple;

file class Ping : IRequest<string>
{
}

file record Echo(string Message) : IRequest<string>
{
}

file class PingHandler :
    IRequestHandler<Ping, string>,
    IRequestHandler<Echo, string>

{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }

    public Task<string> Handle(Echo request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Message);
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
                .AddTransient<IRequestHandler<Echo, string>, PingHandler>()
                .BuildServiceProvider();

        _mediator = new Mediator(serviceProvider);
    }

    [Fact]
    async Task ping_request_response()
    {
        var response = await _mediator.Send(new Ping());

        Assert.Equal("Pong", response);
    }

    [Fact]
    async Task echo_request_response()
    {
        var response = await _mediator.Send(new Echo(Message: "some message"));

        Assert.Equal("some message", response);
    }
}
