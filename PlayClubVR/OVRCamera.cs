using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlayClubVR
{
    public class OVRCamera : GuardedBehaviour
    {
        public delegate void CameraOperation(Camera camera);

        public const string QUAD_LAYER = "Ignore Raycast";

        private IllusionCamera illusionCamera;
        private FieldInfo illusionCameraRotation = typeof(IllusionCamera).GetField("rotate", BindingFlags.NonPublic | BindingFlags.Instance);


        private static OVRCamera instance;
        public static OVRCamera Instance
        {
            get
            {
                if (instance == null)
                {
                    //Console.WriteLine("Create instance");
                    instance = new GameObject("Oculus Camera").AddComponent<OVRCamera>();
                }
                return instance;
            }
        }


        public GameObject LeftEye;
        public GameObject RightEye;
        public GameObject CenterEye;
        public GameObject MainCamera;
        public bool lockRotation = true;


        public GUIQuad GUI;

        private Transform Base;
        private Transform follow;
        private H_Scene hscene;
        private Scene scene;

        private FieldInfo mapTypeIndex;
        private int prevMapTypeIndex = 0;
        private Transform posLock;
        public Vector3 lockOffset = new Vector3(0, 0.15f, 0.15f);
        private Quaternion lockOffsetRot = Quaternion.identity;

        public bool fullLock = false;
        private LockMode lockMode;


        public enum LockMode
        {
            Loose,
            Full,
            Initial
        }

        public bool IsLocked()
        {
            return posLock != null;
        }

        protected override void OnStart()
        {
            //var cam = GetComponent<Camera>();
            //if (cam != null)
            //{
            //    cam.cullingMask = LayerMask.GetMask("UI");
            //}
        }

        protected override void OnAwake()
        {
            //Console.WriteLine("Created instance");

            // Keep this camera

            gameObject.AddComponent<OVRCameraRig>();
            gameObject.AddComponent<OVRManager>();

            Base = transform;
            CenterEye = GetComponentsInChildren<Transform>().First(c => c.name == "CenterEyeAnchor").gameObject;
            LeftEye = GetComponentsInChildren<Camera>().First(c => c.name == "LeftEyeAnchor").gameObject;
            RightEye = GetComponentsInChildren<Camera>().First(c => c.name == "RightEyeAnchor").gameObject;
            //MainCamera = new GameObject();
            
            //MainCamera.AddComponent<Camera>();
            //MainCamera.camera.cullingMask = 0;
            //MainCamera.camera.nearClipPlane = MainCamera.camera.farClipPlane = 1;
            //MainCamera.transform.parent = CenterEye.transform;
            //MainCamera.transform.localPosition = new Vector3(0, 0, -0.5f);
            //MainCamera.transform.localRotation = Quaternion.identity;
            //MainCamera.tag = "MainCamera"; // Needed so that Illusion will find the camera as Camera.main
            //MainCamera.name = "Main Camera Anchor"; // Needed so that the GUI script ignores the camera

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(LeftEye);
            DontDestroyOnLoad(RightEye);
            DontDestroyOnLoad(CenterEye);
            //DontDestroyOnLoad(MainCamera);

            CreateGUI();
            UpdateScale();

            //OVRManager.display.mirrorMode = false;

            Mimic(null);


          //  mapTypeIndex = typeof(HSceneMenu).GetField("mapTypeIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void UpdateScale()
        {
            transform.localScale = Vector3.one * Settings.Instance.IPDScale;
        }

        private void CreateGUI()
        {
            GUI = new GameObject("GUIQuad").AddComponent<GUIQuad>();
            GUI.transform.parent = gameObject.transform;
        }

        public void ToggleLock(LockMode lockMode)
        {

            this.lockMode = lockMode;

            Transform head = GetCurrentHead();
            if (head != null)
            {
                if (posLock != null && posLock == head)
                {
                    // Disable
                    posLock = null;
                    if (head != null) SetHead(head, true);
                }
                else
                {
                    if (posLock != null && posLock != head) {
                        // Different head selected, activate old one
                        SetHead(posLock, true);
                    }

                    posLock = head;
                    
                    var cleansedRotation = Quaternion.Euler(0, head.transform.rotation.eulerAngles.y, 0);
                    lockOffsetRot = Quaternion.Inverse(cleansedRotation) * head.transform.rotation;

                    SetHead(head, false);
                }
           }

        }

        public void ToggleHead()
        {

            var head = GetCurrentHead();
            if (head != null)
            {
                var invisibleHead = head.GetComponent<InvisibleHead>();
                if (invisibleHead == null)
                {
                    head.gameObject.AddComponent<InvisibleHead>();
                }
                else
                {
                    invisibleHead.enabled = !invisibleHead.enabled;
                }
            }
        }

        public Transform GetCurrentHead()
        {

            if (hscene != null)
            {
                Transform head = hscene.Members[0].headObjRoot.GetComponentsInParent<Transform>().First(t => t.name.StartsWith("c") && t.name.Contains("J_Head"));
                //Console.WriteLine(head.name);
                //if (head != null && head.name == "cf_head") head = head.parent;

                return head;

            }
            return null;
        }


        private void SetHead(Transform head, bool visible)
        {
            var invisibleHead = head.GetComponent<InvisibleHead>() ?? head.gameObject.AddComponent<InvisibleHead>();

            invisibleHead.enabled = !visible;
        }

        protected override void OnUpdate()
        {
            if (IsLocked())
            {
                transform.rotation = posLock.rotation;
                transform.position = posLock.position + transform.TransformDirection(lockOffset);

                if (lockMode != LockMode.Full)
                {
                    var e = transform.rotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(0, e.y, 0);
                    if (lockMode == LockMode.Initial)
                    {
                        transform.rotation *= lockOffsetRot;
                    }
                }
            }
            else
            {
                if (follow != null)
                {

                    if (lockRotation && illusionCamera)
                    {
                        var e = follow.rotation.eulerAngles;
                        var rotate = new Vector3(0, e.y, 0);
                        illusionCameraRotation.SetValue(illusionCamera, rotate);

                        transform.rotation = Quaternion.Euler(rotate);

                        Vector3 b = transform.rotation * (Vector3.back * illusionCamera.Distance);
                        transform.position = illusionCamera.Focus + b;
                    }
                    else
                    {
                        transform.position = follow.position;
                        transform.rotation = follow.rotation;
                    }
                }
            }
            
        }

        public void Mimic(Camera cam)
        {
            scene = GameObject.FindObjectOfType<Scene>();
            if (cam == null)
            {
                // Create camera from nothing
                ApplyToCameras(ovrCamera =>
                {
                    ovrCamera.nearClipPlane = Mathf.Clamp(0.01f, 0.001f, 0.01f);
                    ovrCamera.farClipPlane = Mathf.Clamp(50f, 50f, 200f);
                    ovrCamera.cullingMask = LayerMask.GetMask(QUAD_LAYER);
                    ovrCamera.backgroundColor = Color.white;
                    ovrCamera.clearFlags = CameraClearFlags.Skybox;
                });

                transform.parent = null;
                GUI.UpdateGUI(false, false);
                follow = null;
                Base = transform;
            }
            else
            {
                // Copy camera values
                ApplyToCameras(ovrCamera =>
                {
                    ovrCamera.nearClipPlane = Mathf.Clamp(0.01f, 0.001f, 0.01f);
                    ovrCamera.farClipPlane = Mathf.Clamp(100f, 50f, 200f);
                    ovrCamera.cullingMask = cam.camera.cullingMask & ~LayerMask.GetMask("UI");
                    ovrCamera.clearFlags = CameraClearFlags.Color;
                    ovrCamera.backgroundColor = cam.backgroundColor;
                    //Console.WriteLine(ovrCamera.clearFlags);
                    //var skybox = cam.GetComponent<Skybox>();
                    //if (skybox != null)
                    //{
                    //    var ovrSkybox = ovrCamera.gameObject.GetComponent<Skybox>();
                    //    if (ovrSkybox == null) ovrSkybox = ovrSkybox.gameObject.AddComponent<Skybox>();

                    //    ovrSkybox.material = skybox.material;
                    //}
                });

                hscene = GameObject.FindObjectOfType<H_Scene>();
                
                transform.parent = null;
                cam.camera.cullingMask = 0;
                GUI.UpdateGUI(true, true);
                follow = cam.transform;
                Base = follow;
            }

            illusionCamera = GameObject.FindObjectOfType<IllusionCamera>();

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            SetRotationLock(lockRotation);
        }

        public void ApplyToCameras(CameraOperation operation)
        {
            operation(LeftEye.camera);
            operation(RightEye.camera);
        }


        public void SetRotationLock(bool val)
        {
            lockRotation = val;
        }

        internal void AlignTo(Transform head, Vector3 offset)
        {
            if (illusionCamera)
            {
                var origin = head.position + offset;
                var distance = illusionCamera.Distance;
                illusionCamera.SetFocus(origin + Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * illusionCamera.Distance);

                var yRot = Vector3.Angle( (origin - illusionCamera.Focus).normalized , Vector3.back);
                illusionCameraRotation.SetValue(illusionCamera, new Vector3(0, -yRot, 0));
            }
        }

        internal void Recenter()
        {
            OVRManager.capiHmd.RecenterPose();
        }
    }
}
