using H_Voice;
using IllusionPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace AnanNaitegoran
{
    public class WWWHelper : Singleton<WWWHelper>
    {
        private AudioSource watchedVoice;
        private Action watcherCallback;

        public void Watch(AudioSource voice, Action callback)
        {
            watcherCallback = callback;
            watchedVoice = voice;
        }

        public void Unwatch() {
            watchedVoice = null;
        }

        private void Update()
        {
            if (watchedVoice != null && !watchedVoice.isPlaying)
            {
                watchedVoice = null;
                watcherCallback();
            }
        }

        public static void Play(string file, Human human, float delay, bool inHeart, bool isOneShot)
        {

            Instance.StartCoroutine(PlayCoroutine(file, human, delay, inHeart, isOneShot));
        }

        private static IEnumerator PlayCoroutine(string file, Human human, float delay, bool inHeart, bool isOneShot)
        {
            AudioClip clip = null;

            if (file.ToLower().EndsWith(".ogg") || file.ToLower().EndsWith(".wav"))
            {
                var www = new WWW(new System.Uri(file).AbsoluteUri);
                yield return www;
                clip = www.audioClip;
            }
            else
            {
                // Fallback to CSCore
                clip = CSCAudioClip.GetClip(file);
            }

            try
            {
                if (inHeart)
                    human.voice.PlayInHeart(clip, !isOneShot, 0f);
                else if (delay > 0f)
                    human.voice.PlayVoiceDelay(clip, !isOneShot, 0f, delay);
                else
                    human.voice.PlayVoice(clip, !isOneShot, 0f);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
    }

    public class AnanOneVoice : OneVoice
    {
        public static string InfoText = "";

        public DETAIL Detail;
        public List<OneVoice> Fallbacks = new List<OneVoice>();
        public VoiceFinder Finder;

        private static bool mergeWithOriginalVoices = ModPrefs.GetBool("Voices", "bMergeWithOriginalVoices", false, true);
        private static bool randomMoaning = ModPrefs.GetBool("Voices", "bRandomMoaning", false, true);


        private AnanOneVoice() : base(null, 0) { }

        public override bool PlayInHeart(Human human)
        {
            WWWHelper.Instance.Unwatch();

            bool nativeLoop = IsLoop && !randomMoaning;
            bool hackedLoop = IsLoop && randomMoaning;

            var voice = ChooseVoice(human);
            if (voice is string)
            {
                WWWHelper.Play(voice as string, human, 0f, true, !nativeLoop);
            }
            else if (voice is OneVoice)
            {
                ((OneVoice)voice).loop = nativeLoop;
                ((OneVoice)voice).PlayInHeart(human);
            }
            else
            {
                return false;
            }

            if (hackedLoop)
            {
                WWWHelper.Instance.Watch(human.voice.heartSource, delegate
                {
                    PlayInHeart(human);
                });
            }

            return true;
        }

        public override bool PlayVoice(Human human, float delay)
        {
            WWWHelper.Instance.Unwatch();

            bool nativeLoop = IsLoop && !randomMoaning;
            bool hackedLoop = IsLoop && randomMoaning;

            var voice = ChooseVoice(human);
            if (voice is string)
            {
                WWWHelper.Play(voice as string, human, delay, false, !nativeLoop);
            }
            else if (voice is OneVoice)
            {
                ((OneVoice)voice).loop = nativeLoop;
                ((OneVoice)voice).PlayVoice(human, delay);
            }
            else
            {
                return false;
            }

            if (hackedLoop)
            {
                WWWHelper.Instance.Watch(human.voice.voiceSource, delegate
                {
                    PlayVoice(human, delay);
                });
            }

            return true;
        }

        private bool IsLoop
        {
            get
            {
                return Detail.ToString().Contains("LOOP");
            }
        }

        private object ChooseVoice(Human human)
        {
            AnanOneVoice.InfoText = String.Format("{0}/{1}/{2}", human.CharaType, human.Cultivate, Detail.ToString());

            string[] voices = Finder.FindVoices(Detail, human.Cultivate, human.CharaType);
            var fallbacks = GetFallbacks(human.Cultivate);

                                                                    // ↓ essentially a weighted random determination
            if (voices.Length > 0 && (!mergeWithOriginalVoices || UnityEngine.Random.Range(0, voices.Length + fallbacks.Count()) < voices.Length ) )
            {
                //System.Console.WriteLine("NO FALLBACK");
                var clip = voices[UnityEngine.Random.Range(0, voices.Length)];
                return clip;
            }
            else
            {
                var fallbacksArr = fallbacks.ToArray();
                if (fallbacksArr.Length > 0)
                {
                    return fallbacksArr[UnityEngine.Random.Range(0, fallbacksArr.Length)];
                }
                else
                {
                    return false;
                }
            }
        }

        private IEnumerable<OneVoice> GetFallbacks(Human.CULTIVATE cultivate)
        {
            return Fallbacks.Where(v => v.hasVoice[(int)cultivate]);
        }

        public void BuildHasVoicesTable()
        {
            hasVoice = new bool[7];
            for (int i = 0; i < 7; i++)
            {
                hasVoice[i] = Fallbacks.Any(v => v.hasVoice[i]) || Finder.FindVoices(Detail, (Human.CULTIVATE)i, Human.CHARATYPE.AKANE).Length > 0;
            }
        }



        public static AnanOneVoice CreateInstance()
        {
            return FormatterServices.GetUninitializedObject(typeof(AnanOneVoice)) as AnanOneVoice;
        }


    }
}
