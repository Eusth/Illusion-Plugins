using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using System.Linq;

namespace DebugPlugin
{
    public class DebugPlugin : IPlugin
    {
        public void OnApplicationStart()
        {
            Application.RegisterLogCallback(HandleLog);

            // Force open a console window
            if (!Environment.CommandLine.Contains("--verbose"))
                Windows.GuiConsole.CreateConsole();

            Application.targetFrameRate = 75;

        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Warning)
                System.Console.ForegroundColor = ConsoleColor.Yellow;
            else if (type == LogType.Error)
                System.Console.ForegroundColor = ConsoleColor.Red;
            else
                System.Console.ForegroundColor = ConsoleColor.White;

            // We're half way through typing something, so clear this line ..
            System.Console.WriteLine(message);
        }

        public void OnApplicationQuit()
        {
            Console.WriteLine("Quitting...");
        }

        public void OnLevelWasLoaded(int level)
        {
            Console.WriteLine("Loaded Level: {0}", level);
            Console.WriteLine(Application.targetFrameRate);
            
        }

        public void OnLevelWasInitialized(int level)
        {
            Console.WriteLine("Initialized Level: {0}", level);
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Dump
                using(var writer = File.CreateText("dump.json"))
                {
                    Helper.Dump(writer);
                }

                Console.WriteLine("DUMPED");
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                var humans = GameObject.FindObjectsOfType<Human>();
                
                if(humans.Length > 0) {

                    int i = (Input.GetKey(KeyCode.LeftShift) ? 1 : 0) % humans.Length;

                    Helper.ExportToCollada("collada.dae", humans[i].transform.GetComponentsInChildren<Transform>().First(t => t.name.Contains("body")));
                    Console.WriteLine("DUMPED COLLADA");
                }
            }

            for (int i = (int)KeyCode.Keypad1; i <= (int)KeyCode.Keypad9; i++)
            {
                if (Input.GetKeyDown((KeyCode)i))
                {
                    int no = i - (int)KeyCode.Keypad0;
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        var GC = GameObject.FindObjectOfType<GameControl>();
                        GC.PlayData.Load(Directory.GetCurrentDirectory() + "/UserData/save/Game/" + (no).ToString("00") + ".gsd");
                        if (GC.PlayData.isAdv)
                        {
                            GC.SceneCtrl.Change("EventScene", "Load", Color.black, 0.1f, 0f, false);
                        }
                        else
                        {
                            GC.SceneCtrl.Change("SelectScene", "Load", Color.black, 0.1f, 0f, false);
                        }
                    }
                    else
                    {
                        Application.LoadLevel(no);
                    }

                    break;
                }

            }

        }

        public void OnFixedUpdate()
        {
        }

        public string Name
        {
            get { return "Debug Plugin"; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }
    }
}
