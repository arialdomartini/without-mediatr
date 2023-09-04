using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.RequestResponseSubtyping;

file record SubTypeOfPing : Ping;
file record Ping : IRequest<string>;

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
    async Task Ping_is_delivered()
    {
        var response = await _mediator.Send(new Ping());
        
        Assert.Equal("Pong", response);
    }
    
    [Fact]
    async Task SubTypeOfPing_is_not_delivered()
    {
        await Assert.ThrowsAsync(
            typeof(InvalidOperationException), 
            async () => await _mediator.Send(new SubTypeOfPing())); 
    }
}
