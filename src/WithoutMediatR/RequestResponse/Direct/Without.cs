using Xunit;

namespace WithoutMediatR.RequestResponse.Direct;

file interface IPingHandler
{
    string Ping();
}

file class PingHandler : IPingHandler
{
    string IPingHandler.Ping() => "Pong";
}

file class Client
{
    private readonly IPingHandler _pingHandler;

    internal Client(IPingHandler pingHandler)
    {
        _pingHandler = pingHandler;
    }

    internal string UsePingHandler()
    {
        // do work
        return _pingHandler.Ping();
    }
}

public class Without
{
    [Fact]
    void ping_request_response()
    {
        IPingHandler pingHandler = new PingHandler();
        var response = pingHandler.Ping();

        Assert.Equal("Pong", response);
    }

    [Fact]
    void ping_request_response_via_client()
    {
        var client = new Client(new PingHandler());
 
        var response = client.UsePingHandler();
        
        Assert.Equal("Pong", response);
    }
}
