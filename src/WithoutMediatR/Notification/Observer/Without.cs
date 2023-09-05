using Lamar;
using Xunit;

namespace WithoutMediatR.Notification.Observer;

record Ping(string Message);

internal interface ISubject : IObservable<Ping>
{
    void SendMessage(Ping message);
}

internal class Subject : ISubject
{
    private readonly ISet<IObserver<Ping>> _observers = new HashSet<IObserver<Ping>>();

    IDisposable IObservable<Ping>.Subscribe(IObserver<Ping> observer)
    {
        _observers.Add(observer);

        return new Unsubscriber(observer, _observers);
    }

    void ISubject.SendMessage(Ping message)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(message);
        }
    }
}

internal class Unsubscriber : IDisposable
{
    private readonly IObserver<Ping> _observer;
    private readonly ICollection<IObserver<Ping>> _observers;

    internal Unsubscriber(
        IObserver<Ping> observer,
        ICollection<IObserver<Ping>> observers)
    {
        _observer = observer;
        _observers = observers;
    }

    void IDisposable.Dispose()
    {
        _observers.Remove(_observer);
    }
}

internal class Pong1 : IObserver<Ping>
{
    internal static string Received;

    internal string Message { get; set; }

    void IObserver<Ping>.OnCompleted() { }

    void IObserver<Ping>.OnError(Exception error) { }

    void IObserver<Ping>.OnNext(Ping value)
    {
        Received = $"Pong1 received {value.Message}";
    }
}

internal class Pong2 : IObserver<Ping>
{
    internal static string Received;

    internal string Message { get; set; }

    void IObserver<Ping>.OnCompleted() { }

    void IObserver<Ping>.OnError(Exception error) { }

    void IObserver<Ping>.OnNext(Ping value)
    {
        Received = $"Pong2 received {value.Message}";
    }
}

public class Without
{

    [Fact]
    void notify_all_observers()
    {
        ISubject subject = new Subject();
        subject.Subscribe(new Pong1());
        subject.Subscribe(new Pong2());

        subject.SendMessage(new Ping(Message: "some message"));

        Assert.Equal("Pong1 received some message", Pong1.Received);
        Assert.Equal("Pong2 received some message", Pong2.Received);
    }
}
