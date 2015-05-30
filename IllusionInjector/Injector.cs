﻿using System;
using UnityEngine;

namespace IllusionInjector
{
    public static class Injector
    {
        private static bool injected = false;
        public static void Inject()
        {
            if (!injected)
            {
                injected = true;
                var singleton = new GameObject("PluginManager");
                singleton.AddComponent<PluginComponent>();
            }
        }
    }
}
