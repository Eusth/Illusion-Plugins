using H_Voice;
using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AnanNaitegoran
{


    public class AnanNaitegoran : IPlugin
    {
        H_VoiceControl voiceControl;

        FieldInfo setsInfo = typeof(H_VoiceControl).GetField("sets", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo voicesInfo = typeof(VoiceSet).GetField("voices", BindingFlags.NonPublic | BindingFlags.Instance);

        VoiceFinder finder = new VoiceFinder("Audio");


        public string Name
        {
            get { return "Anan-Naitegoran"; }
        }

        public string Version
        {
            get { return "0.2"; }
        }

        public void OnApplicationStart() {
        }

        public void OnApplicationQuit()
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
            voiceControl = GameObject.FindObjectOfType<H_VoiceControl>();

            if (voiceControl != null)
            {
                new GameObject().AddComponent<GUIHelper>();
                var finder = new VoiceFinder("UserData\\voices");

                var sets = setsInfo.GetValue(voiceControl) as VoiceSet[];
                int i = 0;
                foreach (var set in sets)
                {
                    var detail = (DETAIL)i++;
                    var voices = voicesInfo.GetValue(set) as List<OneVoice>;
                    var myOneVoice = AnanOneVoice.CreateInstance();
                    myOneVoice.Fallbacks = voices;
                    myOneVoice.Detail    = detail;
                    myOneVoice.Finder = finder;

                    myOneVoice.BuildHasVoicesTable();

                    voicesInfo.SetValue(set, new List<OneVoice>()
                    {
                        myOneVoice
                    });
                }
            }
        }

        //public OneVoice[] GetVoices(VoiceFinder finder, DETAIL detail)
        //{
        //    List<AnanOneVoice> oneVoices = new List<AnanOneVoice>();

        //    foreach(var cult in Enum.GetValues(typeof(Human.CULTIVATE)).Cast<Human.CULTIVATE>()) {
        //        foreach(var chara in Enum.GetValues(typeof(Human.CHARATYPE)).Cast<Human.CHARATYPE>()) {
        //            int i = 0;
        //            foreach (var voice in finder.FindVoices(detail, cult, chara))
        //            {
        //                if (oneVoices.Count <= i)
        //                {
        //                    oneVoices.Add(new AnanOneVoice());
        //                }
        //            }
        //        }
        //    }
        //}

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}
