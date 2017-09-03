using System;
using System.Collections.Generic;
using Streams;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class Utils
{

    public static float WrapFloat(float val, float min, float max)
    {
        val = val - (float)Math.Round((val - min) / (max - min)) * (max - min);
        if (val < 0)
            val = val + max - min;
        return val;
    }
}

namespace Streams
{
    public class ActionDisposable : IDisposable
    {
        Action onDispose;

        public ActionDisposable(Action act)
        {
            onDispose = act;
        }

        public void Dispose()
        {
            onDispose();
        }
    }

    public interface IEmptyStream
    {
        IDisposable Listen(Action act);
    }

    public interface IStream<T>
    {
        IDisposable Listen(Action<T> callback);
        void ClearCallbacks();
    }

    public class AnonymousStream<T> : IStream<T>
    {
        private readonly Func<Action<T>, IDisposable> _transformSubscription;

        public AnonymousStream(Func<Action<T>, IDisposable> subscriber)
        {
            _transformSubscription = subscriber;
        }

        public IDisposable Listen(Action<T> act)
        {
            return _transformSubscription(act);
        }

        public void ClearCallbacks()
        {
            
        }
    }

    public class Stream<T> : IStream<T>
    {
        private List<Action<T>> _callbacks;

        public IDisposable Listen(Action<T> callback)
        {
            if (_callbacks == null)
                _callbacks = new List<Action<T>> { callback };
            else
                _callbacks.Add(callback);

            //not using Remove directly to avoid removing callback during foreach
            return new ActionDisposable(() => _callbacks[_callbacks.IndexOf(callback)] = null);
        }

        public void ClearCallbacks()
        {
            _callbacks.Clear();
        }

        public void Send(T val)
        {
            if (_callbacks != null)
            {
                _callbacks.RemoveAll(c => c == null);

                foreach (var c in _callbacks)
                {
                    c(val);
                }
            }
        }


        public IStream<T2> Select<T2>(Func<T, T2> map)
        {
            return new AnonymousStream<T2>(action =>
            {
                return Listen(v => action(map(v)));
            });
        }
    }

    public class EmptyStream
    {
        protected List<Action> _callbacks;

        public IDisposable Listen(Action callback)
        {
            if (_callbacks == null)
                _callbacks = new List<Action> { callback };
            else
                _callbacks.Add(callback);

            return new ActionDisposable(() => _callbacks[_callbacks.IndexOf(callback)] = null);
        }

        public virtual void Send()
        {
            if (_callbacks != null)
            {
                _callbacks.RemoveAll(c => c == null);

                foreach (var c in _callbacks)
                {
                    c();
                }
            }
        }
    }

    public class OnceEmptyStream : EmptyStream
    {
        public override void Send()
        {
            base.Send();
            _callbacks.Clear();
        }
    }

    public class AnonymousEmptyStream : IEmptyStream
    {
        readonly Func<Action, IDisposable> listen;

        public AnonymousEmptyStream(Func<Action, IDisposable> subscribe)
        {
            this.listen = subscribe;
        }

        public IDisposable Listen(Action observer)
        {
            return listen(observer);
        }
    }

    [Serializable]
    public class Cell<T>
    {
        [NonSerialized]
        private Stream<T> innerStream;
        private T currentValue;

        public T value
        {
            get { return currentValue; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, currentValue) == false)
                {
                    currentValue = value;

                    if(innerStream != null)
                        innerStream.Send(value);
                }
            }
        }

        public Cell()
        {

        }

        public Cell(T initialValue)
        {
            currentValue = initialValue;
        }

        public IDisposable Bind(Action<T> reaction)
        {
            if(innerStream == null)
                innerStream = new Stream<T>();

            reaction(currentValue);

            return innerStream.Listen(reaction);
        }
    }


    public class DisposableCollector : IDisposable
    {
        private List<IDisposable> connections;

        public static DisposableCollector operator +(DisposableCollector c, IDisposable item)
        {
            c.Add(item);
            return c;
        }

        public void Add(IDisposable disposable)
        {
            if(connections == null)
                connections = new List<IDisposable>();

            connections.Add(disposable);
        }

        public void Dispose()
        {
            if (connections != null)
            {
                for(var i = 0; i < connections.Count; i++)
                    connections[i].Dispose();

                connections.Clear();
            }
        }
    }
}

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}

public static class Extensions
{
    public static int IndexOf<T>(this T[] array, T value) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == value)
            {
                return i;
            }
        }

        return -1;
    }

    public static bool PutIntoFreeSlot<T>(this T[] array, T value) where T : class
    {
        int index = array.IndexOf(null);
        if (index != -1)
        {
            array[index] = value;
            return true;
        }

        return false;
    }

    public static void Clear<T>(this T[] array) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = null;
        }
    }

    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
        return list;
    }

    //avoiding iterator allocation
    public static T FirstOrDefaultFast<T>(this IList<T> list, Func<T, bool> predicate) where T : class 
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
                return list[i];
        }

        return null;
    }

    class UnityActionDisposable : IDisposable
    {
        public UnityEvent e;
        public UnityAction action;

        public void Dispose()
        {
            if (e != null)
            {
                e.RemoveListener(action);
                e = null;
                action = null;
            }
        }
    }

    public static Streams.IEmptyStream ClickStream(this Button button)
    {
        return new Streams.AnonymousEmptyStream((Action reaction) =>
        {
            var ua = new UnityAction(reaction);
            button.onClick.AddListener(ua);
            return new UnityActionDisposable { action = ua, e = button.onClick };
        });
    }

    public static IDisposable OnClickDo(this Button button, Action reaction)
    {
        return button.ClickStream().Listen(reaction);
    }

    public static IStream<T> Filter<T>(this IStream<T> stream, Func<T, bool> filter)
    {
        return new AnonymousStream<T>((Action<T> reaction) =>
        {
            return stream.Listen(val =>
            {
                if (filter(val)) reaction(val);
            });
        });
    }
}
