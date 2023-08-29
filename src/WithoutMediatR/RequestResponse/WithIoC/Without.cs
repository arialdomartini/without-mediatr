using Lamar;
using Xunit;

namespace WithoutMediatR.RequestResponse.WithIoC;

internal interface IPingHandler
{
    string Ping();
}

file class PingHandler : IPingHandler
{
    string IPingHandler.Ping() => "Pong";
}

file class Controller
{
    private readonly IPingHandler _pingHandler;

    internal Controller(IPingHandler pingHandler)
    {
        _pingHandler = pingHandler;
    }

    internal string SomeMethod() => 
        _pingHandler.Ping();
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
    void ping_request_response_through_a_controller()
    {
        var controller = _container.GetInstance<Controller>();
        
        var response = controller.SomeMethod();
        
        Assert.Equal("Pong", response);
    }
    
    [Fact]
    void ping_request_response_invoking_the_handler()
    {
        var handler = _container.GetInstance<IPingHandler>();
        
        var response = handler.Ping();
        
        Assert.Equal("Pong", response);
    }
}
