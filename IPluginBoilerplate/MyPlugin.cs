using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IPluginBoilerplate
{
    public class MyPlugin : IPlugin
    {
        H_Scene scene;
        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
            scene = GameObject.FindObjectOfType<H_Scene>();
        }

        public void OnUpdate()
        {
            if (scene)
            {
                // Do something with the scene, its members, etc...
            }
        }

        public void OnFixedUpdate()
        {
        }

        public string Name
        {
            get { return "MyPlugin"; }
        }

        public string Version
        {
            get { return "0.0.0"; }
        }
    }
}
