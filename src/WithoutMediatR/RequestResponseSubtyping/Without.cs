using Xunit;

namespace WithoutMediatR.RequestResponseSubtyping;

file record SubTypeOfPing : Ping;

internal record Ping;

internal interface IPingHandler
{
    Task<string> Handle(Ping request);
}

file class PingHandler : IPingHandler
{
    public Task<string> Handle(Ping request)
    {
        return Task.FromResult("Pong");
    }
}

internal class Client
{
    private readonly IPingHandler _pingHandler;

    internal Client(IPingHandler pingHandler)
    {
        _pingHandler = pingHandler;
    }

    internal Task<string> UsePingHandler(Ping request) => 
        _pingHandler.Handle(request);
}

public class Without
{
    private readonly Client _client;

    public Without()
    {
        IPingHandler pingHandler = new PingHandler();
        _client = new Client(pingHandler);
    }
    
    [Fact]
    async Task Ping_is_delivered()
    {
        var response = await _client.UsePingHandler(new Ping());

        Assert.Equal("Pong", response);
    }

    [Fact]
    async Task SubTypeOfPing_is_also_delivered()
    {
        var response = await _client.UsePingHandler(new SubTypeOfPing());

        Assert.Equal("Pong", response);
    }
}
