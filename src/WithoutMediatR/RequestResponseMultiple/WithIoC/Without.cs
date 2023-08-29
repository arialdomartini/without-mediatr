using Lamar;
using Xunit;

namespace WithoutMediatR.RequestResponseMultiple.WithIoC;

public interface IPingHandler
{
    string Ping();
    string Echo(string message);
}

file class PingHandler : IPingHandler
{
    string IPingHandler.Ping() => "Pong";
    string IPingHandler.Echo(string message) => message;
}

public class ControllerWithout
{
    private readonly IPingHandler _pingHandler;

    public ControllerWithout(IPingHandler pingHandler)
    {
        _pingHandler = pingHandler;
    }

    internal string SomeMethod() => 
        _pingHandler.Ping();
    
    internal string AnotherMethod(string message) => 
        _pingHandler.Echo(message);
}

public class Without
{
    private readonly Container _container;

    public Without()
    {
        _container = new Container(cfg =>
        {
            cfg.For<IPingHandler>().Add(new PingHandler());
            cfg.For<Controller>().Use<Controller>();
        });
    }
    
    [Fact]
    void request_response_through_a_controller()
    {
        var controller = _container.GetInstance<ControllerWithout>();

        var pingResponse = controller.SomeMethod();
        Assert.Equal("Pong", pingResponse);
        
        var echoResponse = controller.AnotherMethod("some message");
        Assert.Equal("some message", echoResponse);
    }
    
    [Fact]
    void echo_request_response()
    {
        var pingHandler = _container.GetInstance<IPingHandler>();

        var pingResponse = pingHandler.Echo("some message");
        Assert.Equal("some message", pingResponse);

        var echoResponse = pingHandler.Echo("some message");
        Assert.Equal("some message", echoResponse);
    }
}
