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

public class Without
{
    [Fact]
    void ping_request_response()
    {
        IPingHandler pingHandler = new PingHandler();
        var response = pingHandler.Ping();
        
        Assert.Equal("Pong", response);
    }
}
