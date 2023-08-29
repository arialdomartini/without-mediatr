using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WithoutMediatR.Stream;

file class StreamRequest : IStreamRequest<string>
{
}

file class StreamHandler : IStreamRequestHandler<StreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(StreamRequest request, CancellationToken cancellationToken)
    {
        yield return "foo";
        yield return "bar";
    }
}

public class With
{
    private readonly Mediator _mediator;

    public With()
    {
        var serviceProvider =
                new ServiceCollection()
                    .AddTransient<IStreamRequestHandler<StreamRequest, string>, StreamHandler>()
                    .BuildServiceProvider();

        _mediator = new Mediator(serviceProvider);
    }

    [Fact]
    async Task stream_request_response()
    {
        var response = _mediator.CreateStream(new StreamRequest());

        var values = response.ToBlockingEnumerable().ToArray();
        
        Assert.Equal(new []{"foo", "bar"}, values);
    }
}
