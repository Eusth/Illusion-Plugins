using System;
using System.Collections.Generic;
using System.Text;

namespace IllusionInjector
{
    public class Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = Activator.CreateInstance<T>();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }
}
