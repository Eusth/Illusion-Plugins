using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StudioVR
{
    class StudioPlugin : IPlugin
    {

        public void OnApplicationStart()
        {
        }

        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
            var onMouseChecker = GameObject.FindObjectOfType<OnMouseChecker>();
            //var studio = GameObject.Find("studio");
            if (onMouseChecker != null)
            {
                Console.WriteLine("FOund");
                onMouseChecker.gameObject.AddComponent<OVRCursor>();
                GameObject.DestroyImmediate(onMouseChecker);
            }
            else Console.WriteLine("Not found");


            new GameObject().AddComponent<ShisenCorrecter>();
        }

        public bool Debug
        {
            get { return false; }
        }


        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public string Name
        {
            get { return "StudioVR"; }
        }

        public string Version
        {
            get { return "0.9.2";  }
        }
    }
}
