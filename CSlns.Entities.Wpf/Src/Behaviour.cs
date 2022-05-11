using System;
using System.Collections.Generic;
using System.Reactive.Subjects;


namespace CSlns.Entities.Wpf;


public interface IReadOnlyBehaviour<out T> : IObservable<T> {
    T Value { get; }
}


public interface IBehaviour<T> : IReadOnlyBehaviour<T>, ISubject<T>, IDisposable {
    new T Value { get; set; }
}


public class ReadOnlyBehaviour<T> : IReadOnlyBehaviour<T>, IDisposable {

    public ReadOnlyBehaviour(IObservable<T> observable, T initialValue) {
        this._behaviour = new Behaviour<T>(initialValue);
        this._subscription = observable.Subscribe(this._behaviour);
    }


    public void Dispose() {
        this._subscription.Dispose();
        this._behaviour.Dispose();
    }


    public IDisposable Subscribe(IObserver<T> observer) {
        return this._behaviour.Subscribe(observer);
    }


    public T Value => this._behaviour.Value;


    private readonly IDisposable _subscription;
    private readonly Behaviour<T> _behaviour;
}


public class Behaviour<T> : IBehaviour<T> {

    public Behaviour(T value) {
        this.Subject = new BehaviorSubject<T>(value);
    }


    public void Dispose() {
        this.Subject.OnCompleted();
    }


    public T Value {
        get => this.Subject.Value;
        set {
            if (!EqualityComparer<T>.Default.Equals(this.Subject.Value, value)) {
                this.Subject.OnNext(value);
            }
        }
    }


    public IDisposable Subscribe(IObserver<T> observer) {
        return this.Subject.Subscribe(observer);
    }


    public void OnNext(T value) {
        this.Value = value;
    }


    public void OnError(Exception error) {
        this.Subject.OnError(error);
    }


    public void OnCompleted() {
        this.Subject.OnCompleted();
    }


    private readonly BehaviorSubject<T> Subject;
}


public static class Behaviour {

    public static IBehaviour<T> Create<T>(T value) {
        return new Behaviour<T>(value);
    }


    public static IDisposable ToBehaviour<T>(this IObservable<T> observable, out IReadOnlyBehaviour<T> behaviour,
        T initialValue = default) {
        var bhv = Create<T>(initialValue);
        behaviour = bhv;
        return observable.Subscribe(bhv);
    }


    public static ReadOnlyBehaviour<T> ToBehaviour<T>(this IObservable<T> observable, T initialValue = default) {
        return new ReadOnlyBehaviour<T>(observable, initialValue);
    }

}


public static class _Behaviour {
    
}