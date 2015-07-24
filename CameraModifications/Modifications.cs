using IllusionPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CameraModifications
{
    internal enum CameraMode
    {
        None,
        Lock,
        Rotating,
        Motions
    }
    internal class LookInfo
    {
        public Transform Target = null;
        public LookAtRotator.TYPE Type = LookAtRotator.TYPE.NO;
        public bool IsChecked = true;
    }

    public class KocchiMitePlugin : IEnhancedPlugin
    {
        public string Name
        {
            get { return "Camera Modifications"; }
        }

        public string Version
        {
            get { return "0.6"; }
        }


        public string[] Filter
        {
            get { return new string[] { "PlayClub", "PlayClubStudio" }; }
        }
        
        public readonly int LOOK_TYPES = Enum.GetNames(typeof(LookAtRotator.TYPE)).Count();
        public int currentType = 0;
        public int currentHeadType = 0;
        public bool useEnglish = true;

        private H_Scene m_scene;
        private Human m_focus = null;
        private InvisibleHead m_head = null;
        private int m_lockIndex = 0;

        private CameraMotion m_motion = null;
        private CameraMotion m_nextMotion = null;

        private CameraMode m_mode = CameraMode.None;
        private float m_minSpeed = 10;
        private float m_maxSpeed = 180;
        private float m_cameraAcceleration = 5f;
        private float m_speed = 10;
        private bool m_reverseDirection = false;

        internal LookInfo oldHeadLook = new LookInfo();
        internal LookInfo oldEyeLook = new LookInfo();

        private List<Human> _humans = new List<Human>();
        private bool _humanDirty = true;

        public List<Human> Humans
        {
            get
            {
                if (m_scene) return m_scene.Members;
                if (_humanDirty)
                {
                    _humans = GameObject.FindObjectsOfType<Human>().ToList();
                    _humanDirty = false;
                }
                return _humans;
            }
        }

        private IllusionCamera illusionCamera;
        public Vector3 lockOffset = new Vector3(0, 0.1f, 0);

        private FieldInfo _eyeLookTargetInfo = typeof(LookAtRotator).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic);

        Transform m_eyeTarget;

        private KeyStroke m_SwitchLookKey = new KeyStroke(ModPrefs.GetString("Camera", "sSwitchLookKey", "5", true));
        private KeyStroke m_SwitchEyeLookKey = new KeyStroke(ModPrefs.GetString("Camera", "sEyeLookKey", "6", true));
        private KeyStroke m_SwitchHeadLookKey = new KeyStroke(ModPrefs.GetString("Camera", "sHeadLookKey", "7", true));

        private KeyStroke m_SwitchFirstPersonKey = new KeyStroke(ModPrefs.GetString("Camera", "sSwitchFirstPersonKey", "T", true));

        private KeyStroke m_RotateCameraKey = new KeyStroke(ModPrefs.GetString("Camera", "sRotateCameraKey", "Tab", true));
        private KeyStroke m_RotateCameraInverseKey = new KeyStroke(ModPrefs.GetString("Camera", "sRotateCameraInverseKey", "Shift+Tab", true));
        private KeyStroke m_ToggleDynamicCameraKey = new KeyStroke(ModPrefs.GetString("Camera", "sToggleDynamicCameraKey", "Ctrl+Tab", true));


        private LookAtRotator.TYPE LookType
        {
            get { return (LookAtRotator.TYPE)currentType; }
            set { currentType = (int)value; }
        }

        private LookAtRotator.TYPE HeadLookType
        {
            get { return (LookAtRotator.TYPE)currentHeadType; }
            set { currentHeadType = (int)value; }
        }

        public void OnApplicationStart()
        {
            m_minSpeed = ModPrefs.GetFloat("Camera", "fMinSpeed", 10, true);
            m_maxSpeed = ModPrefs.GetFloat("Camera", "fMaxSpeed", 180, true);
            m_cameraAcceleration = ModPrefs.GetFloat("Camera", "fCameraAcceleration", 5, true);
            useEnglish = ModPrefs.GetBool("Camera", "bUseEnglish", Application.systemLanguage != SystemLanguage.Japanese, true);
        }

        public void OnUpdate()
        {
            if (illusionCamera)
            {
                try
                {
                    HandleKeys();
                    UpdateLook();
                    UpdateHeadLock();
                    UpdateRotation();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            _humanDirty = true;
        }

        private void HandleKeys()
        {
            try
            {
                if (m_SwitchLookKey.Check())
                {
                    currentType = (currentType + 1) % LOOK_TYPES;
                    currentHeadType = currentType;

                    oldEyeLook.IsChecked = false;
                    oldHeadLook.IsChecked = false;
                }
                if (m_SwitchHeadLookKey.Check())
                {
                    currentHeadType = (currentHeadType + 1) % LOOK_TYPES;

                    oldHeadLook.IsChecked = false;
                }
                if (m_SwitchEyeLookKey.Check())
                {
                    currentType = (currentType + 1) % LOOK_TYPES;

                    oldEyeLook.IsChecked = false;
                }

                bool toggleDynamicCamera = m_ToggleDynamicCameraKey.Check();
                bool rotateCamera = m_RotateCameraKey.Check();
                bool rotateCameraInv = m_RotateCameraInverseKey.Check();

                if (rotateCamera || rotateCameraInv || toggleDynamicCamera)
                {
                    if (m_mode == CameraMode.Rotating || m_mode == CameraMode.Motions)
                    {
                        // Disable
                        m_mode = CameraMode.None;
                    }
                    else 
                    {
                        if (m_mode == CameraMode.Lock)
                        {
                            if(m_scene)
                                m_scene.camPosMgr.Change(m_scene.StyleMgr.nowStyle);
                            DisableLock();
                        }

                        if (toggleDynamicCamera)
                        {
                            m_mode = CameraMode.Motions;
                            CreateMotions();
                        }
                        else
                        {
                            m_mode = CameraMode.Rotating;
                            m_speed = m_minSpeed;
                            m_reverseDirection = rotateCameraInv;
                        }
                    }
                }
                else if (m_RotateCameraKey.Check(false))
                {
                    m_speed = Mathf.Min(m_maxSpeed, m_speed + Time.deltaTime * m_cameraAcceleration);
                }

                if (m_SwitchFirstPersonKey.Check())
                {
                    if (m_mode == CameraMode.Lock) {
                        m_head.enabled = false;
                        m_lockIndex++;

                        if(m_lockIndex >= Humans.Count) {
                            m_mode = CameraMode.None;

                            if (m_scene)
                                m_scene.camPosMgr.Change(m_scene.StyleMgr.nowStyle);
                            return;
                        }
                    } else {
                        m_lockIndex = 0;
                        m_mode = CameraMode.Lock;
                    }

                    // Get and hide head
                    var member = Humans[m_lockIndex];
                    var head = member.headObjRoot.GetComponentsInParent<Transform>().First(t => t.name.StartsWith("c") && t.name.Contains("J_Head"));
                    m_head = head.GetComponent<InvisibleHead>();
                    if (m_head == null)
                        m_head = head.gameObject.AddComponent<InvisibleHead>();

                    m_focus = member;
                    m_head.enabled = true;

                    MoveCameraToHead(head, true);
                }

                if (m_mode == CameraMode.Lock && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R)))
                {
                    DisableLock();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void UpdateRotation()
        {
            if (m_mode == CameraMode.Rotating)
            {
                // We're gonne rotate this thingy!
                illusionCamera.Set(
                    illusionCamera.Focus,
                    (Quaternion.Euler(0, Time.deltaTime * m_speed * (m_reverseDirection ? -1 : 1), 0) * Quaternion.Euler(illusionCamera.Rotation)).eulerAngles,
                    illusionCamera.Distance);

            }

            if (m_mode == CameraMode.Motions)
            {
                illusionCamera.Set(
                    m_motion.Focus,
                    illusionCamera.Rotation + m_motion.CameraSpeed * Time.deltaTime,
                    illusionCamera.Distance + m_motion.Speed * Time.deltaTime
                );

                //m_motion.Rotation += m_motion.CameraSpeed * Time.deltaTime;
                //m_motion.Distance += m_motion.Speed * Time.deltaTime;
                m_motion.Duration -= Time.deltaTime;

                if (m_motion.Duration <= 0)
                {
                    CreateMotions();
                }
            }
        }


        private void DisableLock()
        {
            if(m_head)
                m_head.enabled = false;
            m_mode = CameraMode.None;
        }

        private void CreateMotions()
        {
            if (m_nextMotion != null)
                m_motion = m_nextMotion;
            m_nextMotion = new CameraMotion(Humans.First(m => m.sex == Human.SEX.FEMALE));

            if (m_motion == null)
                m_motion = new CameraMotion(Humans.First(m => m.sex == Human.SEX.FEMALE));

            illusionCamera.Set(
                m_motion.Focus,
                m_motion.Rotation,
                m_motion.Distance
            );
        }

        private void MoveCameraToHead(Transform origin, bool forceRotation = false)
        {
            illusionCamera.Set(
                origin.position + origin.TransformDirection(lockOffset),
                forceRotation ? Quaternion.LookRotation(origin.forward, origin.up).eulerAngles : illusionCamera.Rotation,
            0);
        }

        private void UpdateHeadLock()
        {
            try
            {
                if (m_mode == CameraMode.Lock)
                {
                    if (!m_head)
                    {
                        // In case the user changed from gangbang -> single mode
                        m_mode = CameraMode.None;
                        return;
                    }
                    
                    if (m_focus.faceTongue != null)
                    {
                        m_focus.faceTongue.enabled = false;
                    }
                    if (m_focus.bodyTongue != null)
                    {
                        m_focus.bodyTongue.enabled = false;
                    }
                    //illusionCamera.transform.rotation = m_focus.headObjRoot.transform.rotation;
                    //illusionCamera.transform.position = m_focus.headObjRoot.transform.position;
                    MoveCameraToHead(m_head.transform, false);
                    //var origin = m_focus.headObjRoot.transform;
                    //var originPos = origin.position + origin.InverseTransformDirection(lockOffset);
                    //var distance = illusionCamera.Distance;
                    //var focus = originPos + origin.forward * illusionCamera.Distance;
                    //var rot = Quaternion.LookRotation(origin.forward, origin.up);
                    //illusionCamera.Set(originPos, rot.eulerAngles, 0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void UpdateLook()
        {
            
            foreach (var female in Humans.Where(m => m.sex == Human.SEX.FEMALE))
            {
              

                if (LookType != LookAtRotator.TYPE.NO)
                {
                    if (female.eyeLook.CalcType != LookType || (Transform)_eyeLookTargetInfo.GetValue(female.eyeLook) != m_eyeTarget)
                    {
                        // Keep track
                        if (oldEyeLook.IsChecked || currentType == 1)
                        {
                            //Console.WriteLine("SAVING {0}", (Transform)_eyeLookTargetInfo.GetValue(female.eyeLook));

                            oldEyeLook.Target = (Transform)_eyeLookTargetInfo.GetValue(female.eyeLook);
                            oldEyeLook.Type = female.eyeLook.CalcType;
                        }

                        female.ChangeEyeLook(LookType, m_eyeTarget, true);
                    }
                }

                if (HeadLookType != LookAtRotator.TYPE.NO)
                {
                    if (female.neckLook.CalcType != HeadLookType || (Transform)_eyeLookTargetInfo.GetValue(female.neckLook) != m_eyeTarget)
                    {
                        // Keep track
                        if (oldHeadLook.IsChecked || currentHeadType == 1)
                        {
                            //Console.WriteLine("SAVING {0}", (Transform)_eyeLookTargetInfo.GetValue(female.neckLook));
                            oldHeadLook.Target = (Transform)_eyeLookTargetInfo.GetValue(female.neckLook);
                            oldHeadLook.Type = female.neckLook.CalcType;
                        }

                        female.ChangeNeckLook(HeadLookType, m_eyeTarget, true);
                    }
                }

                if (LookType == LookAtRotator.TYPE.NO && !oldEyeLook.IsChecked)
                {
                    //Console.WriteLine("RESET LOOK {0} {1}", oldEyeLook.Target, oldEyeLook.Type);
                    female.eyeLook.Change(oldEyeLook.Type, oldEyeLook.Target, true);
                }
                if (HeadLookType == LookAtRotator.TYPE.NO && !oldHeadLook.IsChecked)
                {
                    //Console.WriteLine("RESET LOOK {0} {1}", oldEyeLook.Target, oldEyeLook.Type);
                    female.neckLook.Change(oldHeadLook.Type, oldHeadLook.Target, true);
                }

                oldHeadLook.IsChecked = true;
                oldEyeLook.IsChecked = true;
            }
            
        }



        private void BuildGUI(H_EditsUIControl controls)
        {
            try
            {
                //var canvas = GameObject.Find("H_UI_Canvas");

                //var container = new GameObject().AddComponent<RectTransform>();
                //container.SetParent(canvas.transform, false);

                //container.gameObject.AddComponent<HorizontalLayoutGroup>();
                //container.anchorMin = new Vector2(0.3f, 0);
                //container.anchorMax = new Vector2(0.3f, 0);
                //container.sizeDelta = new Vector2(300, 100);

                //var img = container.gameObject.AddComponent<Image>();
                //img.color = Color.white;

                var layout = controls.transform.FindChild("EditVertical");
                var exampleToggle = layout.GetComponentInChildren<Toggle>();
                var eyeToggle = GameObject.Instantiate(exampleToggle.gameObject) as GameObject;
                var headToggle = GameObject.Instantiate(exampleToggle.gameObject) as GameObject;

                headToggle.SetActive(true);
                eyeToggle.SetActive(true);
                headToggle.transform.SetParent(layout.transform, false);
                eyeToggle.transform.SetParent(layout.transform, false);

                headToggle.GetComponentInChildren<Text>().text = useEnglish ? "Head Control" : "目線・首";
                eyeToggle.GetComponentInChildren<Text>().text = useEnglish ? "Eye Control" : "目線・目";

                headToggle.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
                eyeToggle.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();

                var headLook = headToggle.AddComponent<LookButton>();
                var eyeLook  = eyeToggle.AddComponent<LookButton>();

                headLook.controls = controls;
                headLook.h_scene = m_scene;
                headLook.watchDog = this;
                headLook.isHead = true;

                eyeLook.controls = controls;
                eyeLook.h_scene = m_scene;
                eyeLook.watchDog = this;
                eyeLook.isHead = false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


       

        public void OnLevelWasInitialized(int level)
        {

            if (m_scene)
            {
                // Look initialization
                try
                {
                    var female = Humans.First(h => h.sex == Human.SEX.FEMALE);
                    oldHeadLook.IsChecked = true;
                    oldHeadLook.Target = (Transform)_eyeLookTargetInfo.GetValue(female.neckLook);
                    oldHeadLook.Type = female.neckLook.CalcType;

                    oldHeadLook.IsChecked = true;
                    oldHeadLook.Target = (Transform)_eyeLookTargetInfo.GetValue(female.eyeLook);
                    oldHeadLook.Type = female.eyeLook.CalcType;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    var target = new GameObject().AddComponent<EyeTarget>();
                    target.rootNode = GameObject.Find("cf_J_Eye_ty").transform;

                    m_eyeTarget = target.transform;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    m_eyeTarget = Camera.main.transform;
                }
            }
        }

        public void OnLateUpdate()
        {

        }

        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
            m_scene = GameObject.FindObjectOfType<H_Scene>();

            LookType = LookAtRotator.TYPE.NO;
            HeadLookType = LookAtRotator.TYPE.NO;
            m_mode = CameraMode.None;
            illusionCamera = Camera.main.GetComponent<IllusionCamera>();

            var controls = GameObject.FindObjectOfType<H_EditsUIControl>();
            if (controls)
            {
                BuildGUI(controls);
            }
        }


        public void OnFixedUpdate()
        {
        }
    }
}
