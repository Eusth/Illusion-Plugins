using Ovr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HaremOVR
{

    class Shortcuts : MonoBehaviour
    {
        enum PlaneState
        {
            Flat,
            Curved,
            Spherical
        }

        private static float scale = 1;
        public CursorBlocker cursor;
        private IEnumerator curvinessAnimator;
        private bool curvy = true;
        private PlaneState guiState;
        private int curviness = 1;

        void Start()
        {
        }

        private IEnumerator AnimateCurviness(float endCurviness)
        {
            var gui = OVRCamera.Instance.GUI;
            float t = 0;
            float duration = 0.5f;
            float startCurviness = gui.curviness;

            while (t < duration)
            {
                gui.curviness = Mathf.Lerp(startCurviness, endCurviness, t / duration);
                gui.RebuildPlane();
                t += Time.deltaTime;
                yield return 0;
            }
            gui.curviness = endCurviness;
            gui.RebuildPlane();

            curvinessAnimator = null;
        }

        private bool TestInput(KeyCode key, bool alt, bool ctrl, bool shift)
        {
            return Input.GetKey(KeyCode.LeftAlt) == alt
                 && Input.GetKey(KeyCode.LeftControl) == ctrl
                 && Input.GetKey(KeyCode.LeftShift) == shift
                 && Input.GetKey(key);
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.F3))
            {
                if (curvinessAnimator != null) StopCoroutine(curvinessAnimator);
                //guiState = (guiState + 1) % Enum.GetNames(typeof(PlaneState)).Length;
                curviness = (curviness + 1) % 3;
                curvinessAnimator = AnimateCurviness(curviness);
                Console.WriteLine(curviness);
                StartCoroutine(curvinessAnimator);
            }

            if (Input.GetKeyUp(KeyCode.Delete) || Input.GetMouseButtonUp(2))
            {
                // Toggle interface
                //renderer.enabled = !renderer.enabled;
                OVRCamera.Instance.GUI.renderer.enabled = !OVRCamera.Instance.GUI.renderer.enabled;
            }

            if (Input.GetKeyUp(KeyCode.F12))
            {
                OVRManager.capiHmd.RecenterPose();
                scale = 1;
            }
            if (Input.GetKeyUp(KeyCode.F4))
            {
                // Lock / Unlock axis
                OVRCamera.Instance.SetRotationLock(!OVRCamera.Instance.lockRotation);
            }

            if (Input.GetKey(KeyCode.F9) || TestInput(KeyCode.KeypadMinus, true, false, false))
            {
                Settings.Instance.IPDScale = Mathf.Clamp(Settings.Instance.IPDScale - Time.deltaTime, 0.01f, 5);
                OVRCamera.Instance.UpdateScale();
            }
            if (Input.GetKey(KeyCode.F10) || TestInput(KeyCode.KeypadPlus, true, false, false))
            {
                Settings.Instance.IPDScale = Mathf.Clamp(Settings.Instance.IPDScale + Time.deltaTime, 0.01f, 5);
                OVRCamera.Instance.UpdateScale();
            }
            if (Input.GetKey(KeyCode.F7) || TestInput(KeyCode.KeypadMinus, false, false, true))
            {
                Settings.Instance.Distance = Mathf.Clamp(Settings.Instance.Distance - 0.1f * Time.deltaTime, 0.1f, 10f);
                OVRCamera.Instance.GUI.RebuildPlane();
            }
            if (Input.GetKey(KeyCode.F8) || TestInput(KeyCode.KeypadPlus, false, false, true))
            {
                Settings.Instance.Distance = Mathf.Clamp(Settings.Instance.Distance + 0.1f * Time.deltaTime, 0.1f, 10f);
                OVRCamera.Instance.GUI.RebuildPlane();
            }

            if (Input.GetKey(KeyCode.F5) || TestInput(KeyCode.KeypadMinus, false, true, false))
            {
                Settings.Instance.Angle = Mathf.Clamp(Settings.Instance.Angle - 50f * Time.deltaTime, 50f, 360f);
                OVRCamera.Instance.GUI.RebuildPlane();
            }
            if (Input.GetKey(KeyCode.F6) || TestInput(KeyCode.KeypadPlus, false, true, false))
            {
                Settings.Instance.Angle = Mathf.Clamp(Settings.Instance.Angle + 50f * Time.deltaTime, 50f, 360f);
                OVRCamera.Instance.GUI.RebuildPlane();
            }

            if(Input.GetKeyUp(KeyCode.S) && Input.GetKey(KeyCode.LeftAlt)) {
                // Save settings
                Settings.Instance.Save();

            }
            if (Input.GetKeyUp(KeyCode.D) && Input.GetKey(KeyCode.LeftAlt))
            {
                Settings.Instance.Reset();

                OVRCamera.Instance.UpdateScale();
                OVRCamera.Instance.GUI.RebuildPlane();
                OVRCamera.Instance.GUI.UpdatePosition();
            }

            if (TestInput(KeyCode.KeypadPlus, false, false, false))
            {
                Settings.Instance.OffsetY -= 0.5f * Time.deltaTime;
                OVRCamera.Instance.GUI.RebuildPlane();

            }
            if (TestInput(KeyCode.KeypadMinus, false, false, false))
            {
                Settings.Instance.OffsetY += 0.5f * Time.deltaTime;
                OVRCamera.Instance.GUI.RebuildPlane();

            }

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.P))
            {
                using (var writer = System.IO.File.CreateText("dump.json"))
                    Helper.Dump(writer);
                Console.WriteLine("Created dump.");
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                //var males = GameObject.FindObjectsOfType<RootMotion.FinalIK.FullBodyBipedIK>().Where(obj => obj.name == "cm_base");
                HaremOVR.OVRCamera.LockMode mode = OVRCamera.LockMode.Loose;
                if (Input.GetKey(KeyCode.LeftControl)) mode = OVRCamera.LockMode.Full;
                if (Input.GetKey(KeyCode.LeftShift)) mode = OVRCamera.LockMode.Initial;

                OVRCamera.Instance.ToggleLock(mode);
            }

            if (Input.GetKeyUp(KeyCode.Backspace))
            {
                OVRCamera.Instance.ToggleHead();

            }
            if (Input.GetKeyUp(KeyCode.Insert))
            {
                var head = OVRCamera.Instance.GetCurrentHead();
                if (head != null)
                {
                    OVRCamera.Instance.AlignTo(head, OVRCamera.Instance.lockOffset);
                }
            }



            //if (Input.GetKey(KeyCode.LeftAlt))
            //{
            //    if (Input.GetKeyUp(KeyCode.Keypad1)) ToggleCamera(0);
            //    if (Input.GetKeyUp(KeyCode.Keypad2)) ToggleCamera(1);
            //    if (Input.GetKeyUp(KeyCode.Keypad3)) ToggleCamera(2);
            //    if (Input.GetKeyUp(KeyCode.Keypad4)) ToggleCamera(3);
            //    if (Input.GetKeyUp(KeyCode.Keypad5)) ToggleCamera(4);
            //    if (Input.GetKeyUp(KeyCode.Keypad6)) ToggleCamera(5);
            //    if (Input.GetKeyUp(KeyCode.Keypad7)) ToggleCamera(6);
            //    if (Input.GetKeyUp(KeyCode.Keypad8)) ToggleCamera(7);
            //    if (Input.GetKeyUp(KeyCode.Keypad9)) ToggleCamera(8);
            //}


        }

        //private void ToggleCamera(int num)
        //{
        //    if (cameras.Count > num)
        //    {
        //        Console.WriteLine("Toggle Camera {0} ({1})", num + 1, cameras[num]);
        //        cameras[num].enabled = !cameras[num].enabled;
        //    }
        //}
    }
}
