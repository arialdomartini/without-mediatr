using Xunit;

namespace WithoutMediatR.Stream;

file interface IStreamHandler
{
    IAsyncEnumerable<string> Ping();
}

file class StreamHandler : IStreamHandler
{
    async IAsyncEnumerable<string> IStreamHandler.Ping()
    {
        yield return "foo";
        yield return "bar";
    }
}

public class Without
{
    [Fact]
    void ping_request_response()
    {
        IStreamHandler pingHandler = new StreamHandler();
        
        var values = pingHandler.Ping();
        
        Assert.Equal(new []{"foo", "bar"}, values.ToBlockingEnumerable().ToArray());
    }
}
