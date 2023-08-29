using Xunit;

namespace WithoutMediatR.RequestResponseMultiple.Direct;

file interface IPingHandler
{
    string Ping();
    string Echo(string message);
}

file class PingHandler : IPingHandler
{
    string IPingHandler.Ping() => "Pong";
    string IPingHandler.Echo(string message) => message;
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
    void echo_request_response()
    {
        IPingHandler pingHandler = new PingHandler();
        var response = pingHandler.Echo("some message");
        
        Assert.Equal("some message", response);
    }
}
