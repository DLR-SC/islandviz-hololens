using System;
using System.Reflection;

namespace HoloIslandVis
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        private static Lazy<T> _instance;

        public static T Instance {
            get { return _instance.Value; }
            private set { }
        }

        static Singleton()
        {
            _instance = new Lazy<T>(() => {
                BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var constructor = typeof(T).GetConstructor(flags, null, Type.EmptyTypes, null);
                return (T) constructor.Invoke(null);
            });
        }
    }
}
