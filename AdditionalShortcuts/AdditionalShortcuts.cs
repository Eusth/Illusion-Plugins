using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
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
                { new KeyStroke( ModPrefs.GetString("Additional Shortcuts", "sFreeze", "M", true)), Freeze }
            };
        }

        public string Name
        {
            get { return "Additional Shortcuts"; }
        }

        public string Version
        {
            get { return "0.2"; }
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
            var piston = GameObject.Find("Piston").GetComponent<Toggle>();
            piston.isOn = !piston.isOn;
        }

        private void ToggleGrind() {
            var grind = GameObject.Find("Grind").GetComponent<Toggle>();
            grind.isOn = !grind.isOn;
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
