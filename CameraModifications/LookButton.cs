using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CameraModifications
{
    public class LookButton : MonoBehaviour
    {
        public bool isHead;

        public H_Scene h_scene;
        public H_EditsUIControl controls;
        private GameObject buttonPrefab;
        public KocchiMitePlugin watchDog;
        private Toggle m_toggle;
        private UI_ShowCanvasGroup group;
        private int currentValue = -1;

        private Dictionary<LookAtRotator.TYPE, Toggle> toggles;

        private bool initialShutup = true;

        private void Start()
        {
            // Make buttons
            m_toggle = GetComponent<Toggle>();
            m_toggle.onValueChanged.AddListener(HandleClick);
            controls.GetComponent<ToggleGroup>().RegisterToggle(m_toggle);

            try
            {
                MakeGroup();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public int CurrentMode
        {
            get
            {
                return isHead ? watchDog.currentHeadType : watchDog.currentType;
            }
        }
        private void Update()
        {
            if (CurrentMode != currentValue)
            {
                currentValue = CurrentMode;
                toggles[(LookAtRotator.TYPE)currentValue].isOn = true;
            }

            initialShutup = false;
        }

        private void HandleClick(bool isOn)
        {
            try
            {
                if (isOn)
                {
                    m_toggle.image.color = Color.blue;
                    // Show
                    h_scene.GC.SystemSE.Play_Click();
                }
                else
                {
                    m_toggle.image.color = Color.white;

                    // Hide
                    h_scene.GC.SystemSE.Play_Cancel();
                }
                group.Show(isOn);
                
                Console.WriteLine(isOn);
              
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void MakeGroup()
        {
            var container = controls.transform.FindChild("EditStep2");
            Console.WriteLine("C {0}", container);
            var exampleGroup = container.GetChild(0);
            Console.WriteLine("E {0}", exampleGroup);


            buttonPrefab = GameObject.Instantiate(container.GetComponentInChildren<Toggle>().gameObject) as GameObject;
            buttonPrefab.SetActive(false);

            group = new GameObject().AddComponent<CanvasGroup>().gameObject.AddComponent<UI_ShowCanvasGroup>();
            group.gameObject.layer = LayerMask.NameToLayer("UI");
            group.gameObject.AddComponent<ToggleGroup>().allowSwitchOff = false;
            group.gameObject.AddComponent<RectTransform>(exampleGroup.GetComponent<RectTransform>());
            group.gameObject.AddComponent<VerticalLayoutGroup>(exampleGroup.GetComponent<VerticalLayoutGroup>()).childForceExpandHeight = false;
            group.transform.SetParent(container, false);

            //group.GetComponent<RectTransform>().GetCopyOf(exampleGroup.GetComponent<RectTransform>());

            // Make buttons
            toggles = new Dictionary<LookAtRotator.TYPE, Toggle>() {
                { LookAtRotator.TYPE.NO, MakeButton(watchDog.useEnglish ? "None" : "無設定", LookAtRotator.TYPE.NO) },
                { LookAtRotator.TYPE.AWAY, MakeButton(watchDog.useEnglish ? "Away" : "あっち向け", LookAtRotator.TYPE.AWAY)},
                { LookAtRotator.TYPE.FORWARD, MakeButton(watchDog.useEnglish ? "Forward" : "正面向け", LookAtRotator.TYPE.FORWARD)},
                { LookAtRotator.TYPE.TARGET, MakeButton(watchDog.useEnglish ? "Camera" : "こっち向け", LookAtRotator.TYPE.TARGET)}
            };

            foreach (var button in toggles.Values)
            {
                button.transform.SetParent(group.transform, false);
            }

            //group.gameObject.AddComponent<Image>().color = Color.red;
        }

        private Toggle MakeButton(string text, LookAtRotator.TYPE type)
        {
            var buttonObj = Instantiate(buttonPrefab) as GameObject;
            buttonObj.SetActive(true);

            GameObject.DestroyImmediate(buttonObj.GetComponent<global::UI_ShowCanvasGroup>());
            buttonObj.AddComponent<UI_ShowCanvasGroup>();
            var buttonEl = buttonObj.GetComponent<Toggle>();
            Console.WriteLine(buttonEl.group);
            buttonEl.onValueChanged = new Toggle.ToggleEvent();
            buttonEl.group = group.GetComponent<ToggleGroup>();

            // set text
            Console.WriteLine(buttonPrefab.name);
            buttonEl.GetComponentInChildren<Text>().text = text;
            //buttonEl.GetComponentInChildren<Text>().resizeTextForBestFit = true;
            //buttonEl.GetComponentInChildren<Text>().resizeTextMinSize = 1;

            buttonEl.onValueChanged.AddListener((state) =>
            {
                if (state)
                {
                    if(!initialShutup)
                        h_scene.GC.SystemSE.Play_Click();

                    if (isHead)
                    {
                        watchDog.currentHeadType = (int)type;
                        if(!initialShutup) watchDog.oldHeadLook.IsChecked = false;
                    }
                    else
                    {
                        watchDog.currentType = (int)type;
                        if (!initialShutup) watchDog.oldEyeLook.IsChecked = false;
                    }

                    currentValue = (int)type;

                    buttonEl.image.color = Color.blue;
                }
                else
                {
                    buttonEl.image.color = Color.white;
                }
            });

            return buttonEl;
        }
    }
}
