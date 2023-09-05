using Lamar;
using Xunit;

namespace WithoutMediatR.RequestResponseSubtyping;

file class SubtypeOfPing : Ping{};

file class Ping
{
    public string Message { get; set; }
};

file interface IPingHandler
{
    string Ping(Ping request);
}

file class PingHandler : IPingHandler
{
    string IPingHandler.Ping(Ping request) => $"Handler received {request.Message}";
}

file class Client
{
    private readonly IPingHandler _pingHandler;

    internal Client(IPingHandler pingHandler)
    {
        _pingHandler = pingHandler;
    }

    internal string SomeMethod(Ping request) => 
        _pingHandler.Ping(request);
}


public class Without
{
    private readonly Container _container;

    public Without()
    {
        _container = new Container(cfg =>
        {
            cfg.For<IPingHandler>().Add(new PingHandler());
            cfg.For<Client>().Use<Client>();
        });
    }
    
    [Fact]
    void subtype_of_ping_request_is_delivered()
    {
        var controller = _container.GetInstance<Client>();

        var instanceOfSubtype = new SubtypeOfPing{Message = "some message"};
        
        var response = controller.SomeMethod(instanceOfSubtype);
        
        Assert.Equal("Handler received some message", response);
    }
}
