using System;
using System.Reactive;


namespace glTFViewer.Utils;


public static class DisposableRef {
    
    public static void Dispose<T>(ref T? disposable) where T : class, IDisposable {
        disposable?.Dispose();
        disposable = null;
    }
    
}


public static class _Disposable {
    
    public static IDisposable DisposeWith(this IDisposable disposable, IObservable<Unit> onDispose) {
        return onDispose.Subscribe(_ => disposable.Dispose());
    }
    
}