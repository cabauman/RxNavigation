using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Sample.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> WhereNotNull<T>(this IObservable<T> @this)
        {
            return @this.Where(x => x != null);
        }

        public static IObservable<Unit> ToSignal<T>(this IObservable<T> @this)
        {
            return @this.Select(_ => Unit.Default);
        }
    }
}
