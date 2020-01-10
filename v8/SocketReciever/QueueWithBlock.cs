using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace v8.SocketReciever
{
    public class QueueWithBlock<T> : Queue<T> where T : class
    {
        private object lockObject = new object();

        public new void Enqueue(T item)
        {
            lock (lockObject)
            {
                if (Count == 0)
                {
                    base.Enqueue(item);
                    Monitor.PulseAll(lockObject);
                }
                else
                {
                    base.Enqueue(item);

                }
            }
        }

        public new T Dequeue()
        {
            lock (lockObject)
            {
                if (Count == 0)
                {
                    Monitor.Wait(lockObject);
                }

                if (Count == 0)
                {
                    return null;
                }

                return base.Dequeue();
            }
        }

        public void Release()
        {
            lock (lockObject)
            {
                Monitor.PulseAll(lockObject);
            }
        }
    }
}
