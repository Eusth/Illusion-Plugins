using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayClubVR
{
    enum PlayClubGameLevels
    {
        Logo = 0,
        Title = 1,
        Ecchi = 3
    }

    class OVRPlugin : IPlugin
    {
        private OVRCamera ovrCamera;
        private bool enabled = false;

        private bool firstRecenter = false;
        public bool Debug
        {
            get { return Environment.CommandLine.Contains("--verbose"); }
        }

        public void OnApplicationStart() {
            if (Environment.CommandLine.Contains("--vr"))
            {
                enabled = true;
            }

            if (enabled)
            {
                ovrCamera = OVRCamera.Instance;
                if(Environment.CommandLine.Contains("--no-chromatic-aberration"))
                    OVRManager.display.distortionCaps &= ~(uint)Ovr.DistortionCaps.Chromatic;

                var shortcuts = ovrCamera.gameObject.AddComponent<Shortcuts>();
                shortcuts.cursor = GameObject.CreatePrimitive(PrimitiveType.Quad).AddComponent<CursorBlocker>();
            }
        }


        public void OnLevelWasLoaded(int level)
        {
            if (enabled)
            {
                // Menu
                if (level == (int)PlayClubGameLevels.Title && !firstRecenter)
                {
                    firstRecenter = true;
                    ovrCamera.Recenter();
                }
            }
        }

        public void OnLevelWasInitialized(int level)
        {
            if (enabled)
            {
                var mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

                ovrCamera.Mimic(Camera.main);
            }
        }


        #region Method Stubs

        public void OnApplicationQuit() {
        }

        #endregion


        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public string Name
        {
            get { return "PlayClubVR"; }
        }

        public string Version
        {
            get { return "0.2.4"; }
        }
    }
}
