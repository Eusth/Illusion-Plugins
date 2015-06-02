using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AdditionalShortcuts
{
    public class AdditionalShortcuts : IPlugin
    {
        private H_Scene scene;

        private Dictionary<KeyStroke, Action> defaultShortcuts;

        public AdditionalShortcuts()
        {
            defaultShortcuts =  new Dictionary<KeyStroke, Action>() {
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sToggleOrgasmLock", "CapsLock", true)) , ToggleOrgasmLock },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sTogglePiston", "A", true)), TogglePiston },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sToggleGrind", "S", true)), ToggleGrind },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sDecreaseActionSpeed", "D", true)), DecreaseActionSpeed },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sIncreaseActionSpeed", "F", true)), IncreaseActionSpeed },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sSaySomething", "V", true)), SaySomething },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sEjaculate", "C", true)), Ejaculate },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sFreeze", "M", true)), Freeze },

                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sNextPose", "Alt+Right", true)), () => ChangePose(1) },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sPrevPose", "Alt+Left", true)), () => ChangePose(-1) },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sRandPose", "Alt+Down", true)),  () => RandomPose(false) },
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sRandPoseFromAll", "", true)), () => RandomPose(true) },

            };

        }

        public string Name
        {
            get { return "Additional Shortcuts"; }
        }

        public string Version
        {
            get { return "0.3"; }
        }


        public void OnLevelWasInitialized(int level)
        {
            if (level == 3)
            {
                scene = GameObject.FindObjectOfType<H_Scene>();
            }
        }

        public void OnUpdate()
        {
            if (scene)
            {
                foreach (var shortcut in defaultShortcuts)
                {
                    if(shortcut.Key.Check())
                    {
                        shortcut.Value();
                    }
                }
            }
        }

        private void ToggleOrgasmLock()
        {
            //Console.WriteLine(!scene.FemaleGage.Lock ? "Lock" : "Unlock");
            // This also updates the GUI!
            GameObject.Find("XtcLock").GetComponent<Toggle>().isOn = !scene.FemaleGage.Lock;
            //scene.FemaleGage.ChangeLock(!scene.FemaleGage.Lock);
        }

        private void TogglePiston() {
            scene.Pad.pistonToggle.OnPointerClick(new PointerEventData(EventSystem.current));

        }

        private void ToggleGrind() {
            scene.Pad.grindToggle.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private void IncreaseActionSpeed()
        {
            var speedSlider = GameObject.Find("Speed").GetComponentInChildren<Slider>();
            speedSlider.value = Mathf.Clamp(speedSlider.value + (speedSlider.maxValue - speedSlider.minValue) / 5, speedSlider.minValue, speedSlider.maxValue);
        }

        private void DecreaseActionSpeed()
        {
            var speedSlider = GameObject.Find("Speed").GetComponentInChildren<Slider>();
            speedSlider.value = Mathf.Clamp(speedSlider.value - (speedSlider.maxValue - speedSlider.minValue) / 5, speedSlider.minValue, speedSlider.maxValue);
        }

        private void SaySomething()
        {
            scene.Talk();
            //GameObject.Find("Talk").
        }

        private void Ejaculate()
        {
            if (scene.FemaleGage.IsHigh())
            {
                var synced = GameObject.Find("Button_Sync").GetComponent<Button>();
                synced.onClick.Invoke();
            }
            else
            {
                var sotodashi = GameObject.Find("Button_Out").GetComponent<Button>();
                sotodashi.onClick.Invoke();
            }
        }

        private void ChangePose(int direction)
        {
            int currentIndex = scene.StyleMgr.StyleList.IndexOf(scene.StyleMgr.nowStyle);
            int nextIndex = ((currentIndex + scene.StyleMgr.StyleList.Count) + direction) % scene.StyleMgr.StyleList.Count;

            ChangeStyle(scene.StyleMgr.StyleList[nextIndex].file);
        }

        private void RandomPose(bool chooseFromAll = false)
        {
            // the dictionary contains positions from all characters.
            var list = chooseFromAll
                ? scene.StyleMgr.StyleDictionary.Values.ToList()
                : scene.StyleMgr.StyleList;

            // Pick random one
            ChangeStyle(list[UnityEngine.Random.Range(0, list.Count)].file);
        }

        private void ChangeStyle(string name)
        {
            scene.ChangeStyle(name);
            scene.StyleToGUI(name);
            scene.CrossFadeStart();
        }


        #region stubs

        public void OnApplicationStart()
        {
        }

        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
            Time.timeScale = 1;
            //Console.WriteLine("HE");
            //var test = ModPrefs.GetString("Shortcuts", "Test", "Lol", true);
            //Console.WriteLine(test);

        }

        private void Freeze()
        {
            Time.timeScale = Time.timeScale == 1 ? 0 : 1;
        }

        public void OnFixedUpdate()
        {
        }

        #endregion
    }
}
