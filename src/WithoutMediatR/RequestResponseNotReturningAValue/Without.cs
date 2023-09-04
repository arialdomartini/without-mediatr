using Xunit;

namespace WithoutMediatR.RequestResponseNotReturningAValue;

file interface IPingHandler
{
    void Ping();
}

file class PingHandler : IPingHandler
{
    internal static bool HasBeenCalled { get; private set; }
    
    void IPingHandler.Ping()
    {
        // do work
        HasBeenCalled = true;
    }
}

file class Client
{
    private readonly IPingHandler _pingHandler;

    internal Client(IPingHandler pingHandler)
    {
        _pingHandler = pingHandler;
    }

    internal void UsePingHandler()
    {
        // do work
        _pingHandler.Ping();
    }
}

public class Without
{
    [Fact]
    void ping_request_response_via_client()
    {
        var client = new Client(new PingHandler());
 
        client.UsePingHandler();
        
        Assert.True(PingHandler.HasBeenCalled);
    }
}
