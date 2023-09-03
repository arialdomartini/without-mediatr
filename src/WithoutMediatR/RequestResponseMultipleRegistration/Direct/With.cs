using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.RequestResponseMultipleRegistration.Direct;

public record Echo(string Message) : IRequest<string>;

public class EchoHandler : IRequestHandler<Echo, string>
{
    public Task<string> Handle(Echo request, CancellationToken cancellationToken) => Task.FromResult(request.Message);
}

public class AnotherHandler : IRequestHandler<Echo, string>
{
    public Task<string> Handle(Echo request, CancellationToken cancellationToken) => Task.FromResult("doh!");
}

public class With
{
    private readonly Mediator _mediator;

    public With()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddTransient<IRequestHandler<Echo, string>, EchoHandler>()
                .AddTransient<IRequestHandler<Echo, string>, AnotherHandler>()
                .BuildServiceProvider();

        _mediator = new Mediator(serviceProvider);
    }

    [Fact]
    async Task ping_request_response()
    {
        var response = await _mediator.Send(new Echo("some message"));

        Assert.Equal("doh!", response);
    }
}
