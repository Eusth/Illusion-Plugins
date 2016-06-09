using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IllusionInjector
{
    class PluginComponent : MonoBehaviour
    {
        private CompositePlugin plugins;
        private bool freshlyLoaded = false;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Environment.CommandLine.Contains("--verbose") && !Screen.fullScreen)
            {
                Windows.GuiConsole.CreateConsole();
            }

            plugins = new CompositePlugin(PluginManager.LoadPlugins());
            plugins.OnApplicationStart();
        }

        void Start()
        {
            OnLevelWasLoaded(Application.loadedLevel);
        }

        void Update()
        {
            if (freshlyLoaded)
            {
                freshlyLoaded = false;
                plugins.OnLevelWasInitialized(Application.loadedLevel);
            }
            plugins.OnUpdate();
        }

        void LateUpdate()
        {
            plugins.OnLateUpdate();
        }

        void FixedUpdate()
        {
            plugins.OnFixedUpdate();
        }

        void OnDestroy()
        {
            plugins.OnApplicationQuit();
        }

        void OnLevelWasLoaded(int level)
        {
            plugins.OnLevelWasLoaded(level);
            freshlyLoaded = true;
        }

    }
}
