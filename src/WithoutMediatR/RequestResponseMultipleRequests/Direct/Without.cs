using Xunit;

namespace WithoutMediatR.RequestResponseMultipleRequests.Direct;

file interface IMyHandler
{
    string Ping();
    string Echo(string message);
}

file class MyHandler : IMyHandler
{
    string IMyHandler.Ping() => "Pong";
    string IMyHandler.Echo(string message) => message;
}

public class Without
{
    [Fact]
    void ping_request_response()
    {
        IMyHandler myHandler = new MyHandler();
        var response = myHandler.Ping();
        
        Assert.Equal("Pong", response);
    }
    
    [Fact]
    void echo_request_response()
    {
        IMyHandler myHandler = new MyHandler();
        var response = myHandler.Echo("some message");
        
        Assert.Equal("some message", response);
    }
}
