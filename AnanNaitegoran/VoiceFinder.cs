using H_Voice;
using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnanNaitegoran
{
    public class VoiceFinder
    {
        private bool cacheResults = ModPrefs.GetBool("Voices", "bCacheFolders", true, true);
        private bool useFallback = ModPrefs.GetBool("Voices", "bUseFallbackMechanism", true, true);

        private string root;
        private static readonly string[] ALLOWED_EXTENSIONS = {".mp3", ".wav", ".flac", ".aac", ".wma", ".ogg" };
        public VoiceFinder(string rootDirectory)
        {
            this.root = rootDirectory;
        }

        private static Dictionary<string, string[]> cache = new Dictionary<string, string[]>();


        public string[] FindVoices(DETAIL detail, Human.CULTIVATE cultivate, Human.CHARATYPE charaType)
        {
            string chara = FirstCharToUpper(charaType.ToString());
            string cult  = FirstCharToUpper(cultivate.ToString());
            string detailStr = detail.ToString();
            string lookupString = String.Format("{0}-{1}-{2}", chara, cult, detailStr);

            if (cacheResults && cache.ContainsKey(lookupString))
                return cache[lookupString];


            var results = new List<string>();

            int i = 0;
            foreach (var folder in GetFolderList(chara, cult, detailStr))
            {
                results.AddRange(GetMusicFiles(folder, i++ < 4));

                if (results.Count > 0 && useFallback)
                    break;
            }

            var resultsArr = results.ToArray();

            if (cacheResults)
                cache[lookupString] = resultsArr;

            return resultsArr;
        }

        private string[] GetFolderList(string chara, string cult, string detailStr)
        {
            return  new string[] {
                  Path.Combine(Path.Combine(chara, cult), detailStr)
                , Path.Combine(chara, detailStr)
                , Path.Combine(cult, detailStr)
                , detailStr

                , Path.Combine(chara, cult)
                , chara
                , cult

                , ""
            };

        }

        private string[] GetMusicFiles(string directory, bool useWildcards = false)
        {
            try
            {
                var fullPath = Path.Combine(root, directory);

                if (useWildcards)
                {
                    var parentDirectory = new DirectoryInfo(fullPath).Parent.FullName;
                    //Console.WriteLine(parentDirectory);
                    var thisDirectory = new DirectoryInfo(fullPath).Name;
                    if (Directory.Exists(parentDirectory))
                    {
                        return Directory.GetDirectories(parentDirectory)
                                .Where(dir => Regex.IsMatch(thisDirectory, "^" + Regex.Escape(new DirectoryInfo(dir).Name).Replace("%", ".*") + "$", RegexOptions.IgnoreCase))
                                .SelectMany(dir => GetMusicFiles(dir, false)).ToArray();
                    }
                }
                else if (Directory.Exists(fullPath))
                {
                    return Directory.GetFiles(fullPath)
                        .Where(d => ALLOWED_EXTENSIONS.Contains(Path.GetExtension(d).ToLower())).Select(path => Path.GetFullPath(path))
                        .ToArray();
                }
            }
            catch (Exception) { }

            // Fallback
            return new string[0];
        }


        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
        }
    }
}
