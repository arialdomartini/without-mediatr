using Xunit;

namespace WithoutMediatR.Notification.Events;

record Ping(string Message);


internal interface ISubject
{
    void SendMessage(Ping message);
    void Subscribe(EventHandler<Ping> zop);
}

internal class Subject : ISubject
{
    internal event EventHandler<Ping> Handlers;

    void ISubject.Subscribe(EventHandler<Ping> zop)
    {
        Handlers += zop;
    }

    void ISubject.SendMessage(Ping message)
    {
        Handlers.Invoke(this, message);
    }
}

internal class Pong1
{
    internal string Received { get; private set; }

    internal void Notify(object? sender, Ping ping)
    {
        Received = $"Pong1 received {ping.Message}";
    }
}

internal class Pong2
{
    internal string Received { get; private set; }

    internal void Notify(object? sender, Ping ping)
    {
        Received = $"Pong2 received {ping.Message}";
    }
}

public class Without
{

    [Fact]
    void notify_all_observers()
    {
        var pong1 = new Pong1();
        var pong2 = new Pong2();

        ISubject subject = new Subject();
        subject.Subscribe(pong1.Notify);
        subject.Subscribe(pong2.Notify);
        
        subject.SendMessage(new Ping("some message"));

        Assert.Equal("Pong1 received some message", pong1.Received);
        Assert.Equal("Pong2 received some message", pong2.Received);
    }
}
