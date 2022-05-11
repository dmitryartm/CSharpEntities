using CSlns.Entities.Internal;


namespace CSlns.Entities;


public readonly record struct Option<T> {


    public Option(T value) {
        this._hasValue = true;
        this._value = value;
    }


    public bool IsSome => this._hasValue;
    public bool IsNone => !this._hasValue;


    public bool TryGetValue(out T value) {
        value = this._value;
        return this._hasValue;
    }


    public T GetSome() {
        if (this._hasValue) {
            return this._value;
        }
        else {
            throw new InvalidOperationException("Cannot get Some. Option is None");
        }
    }


    public static implicit operator Option<T>(OptionNone _) {
        return default;
    }


    public override string ToString() {
        return this.TryGetValue(out var value) ? $"Some({value})" : "None";
    }


    private readonly bool _hasValue;
    private readonly T _value;

}


public static class Option {

    public static Option<T> Some<T>(T value) => new Option<T>(value);


    public static readonly OptionNone None = OptionNone.Default;


    public static Option<T> NoneOf<T>(T witness = default) => new Option<T>();

}


public static class _Option {

    public static TOut Switch<T, TOut>(this Option<T> @this, Func<T, TOut> Some, Func<TOut> None) {
        return @this.TryGetValue(out var value) ? Some(value) : None();
    }


    public static void Switch<T>(this Option<T> @this, Action<T> Some, Action None) {
        if (@this.TryGetValue(out var value)) {
            Some(value);
        }
        else {
            None();
        }
    }


    public static Option<T> Do<T>(this Option<T> @this, Action<T> Some) {
        @this.Switch(Some, () => {
        });
        return @this;
    }


    public static Option<T> Do<T>(this Option<T> @this, Action<T> Some, Action None) {
        @this.Switch(Some, None);
        return @this;
    }


    public static Option<TRes> Map<TArg, TRes>(this Option<TArg> @this, Func<TArg, TRes> map) {
        return @this.TryGetValue(out var value) ? Option.Some(map(value)) : Option.None;
    }


    public static Option<TRes> Bind<TArg, TRes>(this Option<TArg> @this, Func<TArg, Option<TRes>> bind) {
        return @this.TryGetValue(out var value) ? bind(value) : Option.None;
    }


    public static Option<T> Unwrap<T>(this Option<Option<T>> @this) {
        return @this.TryGetValue(out var value) ? value : Option.None;
    }


    public static T IfNone<T>(this Option<T> @this, T @default) =>
        @this.TryGetValue(out var value) ? value : @default;


    public static T IfNoneDefault<T>(this Option<T> @this) =>
        @this.TryGetValue(out var value) ? value : default;


    public static T IfNoneEval<T>(this Option<T> @this, Func<T> getDefault) =>
        @this.TryGetValue(out var value) ? value : getDefault();


    public static T IfNoneThrow<T>(this Option<T> @this, string message = "Option is None") {
        if (@this.TryGetValue(out var value)) {
            return value;
        }
        else {
            throw new InvalidOperationException(message);
        }
    }

}