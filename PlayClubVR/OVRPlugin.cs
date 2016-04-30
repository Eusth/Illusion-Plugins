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
                int uiMask = LayerMask.GetMask("UI");
                var subCams = Camera.allCameras.Where(
                    cam => (cam.cullingMask & uiMask) == 0 
                        && !cam.name.Contains("Anchor") 
                        && !cam.name.Contains("Oculus")
                        && cam.clearFlags != CameraClearFlags.Color).ToArray();
                //var cullingMask = Camera.allCameras.Where(cam => (cam.cullingMask & uiMask) == 0).Aggregate(0, (a, b) => a | b.cullingMask);
                //Console.WriteLine("Found {0} cameras which gives {1}", Camera.allCameras.Length, cullingMask);

                foreach (var subCam in subCams)
                {
                    Console.WriteLine("{0}: {1}", subCam.name, subCam.depth);
                }
                ovrCamera.Mimic(Camera.main, subCams);
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
