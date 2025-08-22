using System;

namespace Weariness.Util.Container
{
    [Serializable]
    public class Wrapping<T> where T : new()
    {
        public T data;

        public Wrapping()
        {
            data = new();
        }
        public Wrapping(T _data)
        {
            data = _data;
        }
    }
}