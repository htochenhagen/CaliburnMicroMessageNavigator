﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CaliburnMicroMessageNavigator
{
    /// <summary>
    /// Enumerator that returns Items async by delegating MoveNext to the thread pool
    /// </summary>
    class ThreadPoolAsyncEnumerator<T> : IDisposable
    {
        readonly IEnumerator<T> _inner;

        public ThreadPoolAsyncEnumerator(IEnumerable<T> enumerable)
        {
            _inner = enumerable.GetEnumerator();
        }

        public ThreadPoolAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        /// <summary>
        ///  Advances the enumerator to the next element of the collection on a Thread Pool Thread
        /// </summary>
        public Task<bool> MoveNextAsync()
        {
            return Task.Run(() =>
            {
                var result = false;

                try
                {
                    result = _inner.MoveNext();
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }

                return result;
            });
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        public T Current => _inner.Current;

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}