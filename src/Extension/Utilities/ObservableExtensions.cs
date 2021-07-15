using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension
{
    public static class ObservableExtensions
    {
        public static SubscribedList<T> ToSubscribedList<T>(this IObservable<T> source)
        {
            return new SubscribedList<T>(source);
        }

        public class SubscribedList<T> : IReadOnlyList<T>, IDisposable
        {
            private ImmutableArray<T> _list = ImmutableArray<T>.Empty;
            private readonly IDisposable _subscription;

            public SubscribedList(IObservable<T> source)
            {
                _subscription = source.Subscribe(x => { _list = _list.Add(x); });
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)_list).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _list.Length;

            public T this[int index] => _list[index];

            public void Dispose() => _subscription.Dispose();
        }
    }
}
