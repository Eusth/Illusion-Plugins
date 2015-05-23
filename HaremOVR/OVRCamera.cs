using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HaremOVR
{
    public class OVRCamera : MonoBehaviour
    {
        public delegate void CameraOperation(Camera camera);

        public const string QUAD_LAYER = "Ignore Raycast";

        private static OVRCamera instance;
        public static OVRCamera Instance
        {
            get
            {
                if (instance == null)
                {
                    //Console.WriteLine("Create instance");
                    instance = new GameObject("Main Camera").AddComponent<OVRCamera>();
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
        private HScene hscene;
        private BaseScene scene;

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

        void Awake()
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
                Transform head = hscene.hSceneMenu.targetCharacter.GetTopObj().GetComponentsInChildren<Animator>().FirstOrDefault(a => a.name == "cm_head" || a.name == "cf_head").transform;
                if (head != null && head.name == "cf_head") head = head.parent;

                return head;

            }
            return null;
        }


        private void SetHead(Transform head, bool visible)
        {
            var invisibleHead = head.GetComponent<InvisibleHead>() ?? head.gameObject.AddComponent<InvisibleHead>();

            invisibleHead.enabled = !visible;
        }

        void Update()
        {
            //if (Camera.main != null)
            //    Console.WriteLine(Camera.main.name);
            //else
            //    Console.WriteLine("No main camera!");

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
                    transform.position = follow.position;
                    transform.rotation = follow.rotation;

                    if (lockRotation)
                    {

                        var e = transform.rotation.eulerAngles;
                        transform.rotation = Quaternion.Euler(0, e.y, 0);
                    }
                }
            }
        }

        public void Mimic(GameObject cam)
        {
            scene = GameObject.FindObjectOfType<BaseScene>();
            if (cam == null)
            {
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
                var backCamera = GameObject.Find("BackCamera");
                ApplyToCameras(ovrCamera =>
                {
                    ovrCamera.nearClipPlane = Mathf.Clamp(0.01f, 0.001f, 0.01f);
                    ovrCamera.farClipPlane = Mathf.Clamp(100f, 50f, 200f);
                    ovrCamera.cullingMask = cam.camera.cullingMask;
                    ovrCamera.clearFlags = CameraClearFlags.Color;

                    if(backCamera != null)
                        ovrCamera.backgroundColor = backCamera.camera.backgroundColor;

                    //Console.WriteLine(ovrCamera.clearFlags);
                    //var skybox = cam.GetComponent<Skybox>();
                    //if (skybox != null)
                    //{
                    //    var ovrSkybox = ovrCamera.gameObject.GetComponent<Skybox>();
                    //    if (ovrSkybox == null) ovrSkybox = ovrSkybox.gameObject.AddComponent<Skybox>();

                    //    ovrSkybox.material = skybox.material;
                    //}
                });

                hscene = GameObject.FindObjectOfType<HScene>();
                
                transform.parent = null;
                cam.camera.cullingMask = 0;
                GUI.UpdateGUI(true, hscene == null);
                follow = cam.transform;
                Base = follow;

            }

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
            var controls = GameObject.FindObjectOfType<CameraControl>();
            if (controls != null)
            {
                if (lockRotation)
                {
                    controls.yRotSpeed = 0;
                    Base.position = new Vector3(Base.position.x, OVRManager.profile.eyeHeight, Base.position.z);
                    controls.targetObj.position = new Vector3(controls.targetObj.position.x, OVRManager.profile.eyeHeight, controls.targetObj.position.z);
                    controls.TargetSet(controls.targetObj, true);

                }
                else
                    // TODO: Make dynamic
                    controls.yRotSpeed = 5;
                //  

            }
        }

        internal void AlignTo(Transform head, Vector3 offset)
        {
            var controls = GameObject.FindObjectOfType<CameraControl>();
            if (controls != null)
            {
                var distance = Vector3.Distance(controls.targetObj.position, Base.position);

                if (lockRotation)
                    Base.rotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);
                else
                    Base.rotation = head.rotation;
                
                Base.position = head.position + Base.transform.TransformDirection(offset);

                controls.targetObj.position = Base.position + Base.forward * distance;
               // controls.CameraDir = (controls.TargetPos - Base.position).normalized;
                
                //controls.targetObj.position = Base.forward * difference.magnitude;
                //controls.CameraAngle = head.rotation.eulerAngles;
                controls.TargetSet(controls.targetObj, true);
            }
        }
    }
}
