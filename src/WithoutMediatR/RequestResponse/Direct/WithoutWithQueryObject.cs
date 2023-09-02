using Xunit;

namespace WithoutMediatR.RequestResponse.Direct;

file class Ping{}


file interface IPingHandler
{
    string Ping(Ping ping);
}                           


file class PingHandler : IPingHandler
{
    string IPingHandler.Ping(Ping ping) => "Pong";
}


public class WithoutWithQueryObject
{
    [Fact]
    void ping_request_response()
    {
        IPingHandler pingHandler = new PingHandler();
        var response = pingHandler.Ping(new Ping());
        
        Assert.Equal("Pong", response);
    }
}
