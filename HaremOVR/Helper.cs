using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HaremOVR
{
    public static class Helper 
    {
        public static Texture2D LoadImage(string filePath)
        {
            string ovrDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HaremOVR");
            filePath = Path.Combine(ovrDirectory, filePath);

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            else
            {
                Console.WriteLine("File " + filePath + " does not exist");
            }
            return tex;
        }

        public static string DocRoot
        {
            get
            {
                return Environment.CurrentDirectory;
            }
        }

        public static Material GetMaterial()
        {
            return new Material(@"Shader ""Unlit/Transparent"" {
                Properties {
                    _MainTex (""Base (RGB)"", 2D) = ""white"" {}
                }
                SubShader {
	                Tags {""Queue""=""Overlay"" ""IgnoreProjector""=""True"" ""RenderType""=""Transparent""}
                    Pass {
                        ZTest Always
                        Lighting Off
                        SetTexture [_MainTex] { combine texture }
                    }
                }
            }");
        }

        public static Material GetColorMaterial()
        {
            return new Material(@"Shader ""Unlit/Transparent Color"" {
                Properties {
                    _Color (""Main Color"", COLOR) = (1,1,1,1)
                }
                SubShader {
	                Tags {""Queue""=""Overlay"" ""IgnoreProjector""=""True"" ""RenderType""=""Transparent""}
                    
                    Color [_Color]
                    Pass {
                        ZTest Always
                        Lighting Off
                        Blend SrcAlpha OneMinusSrcAlpha
                    }
                }
            }");
        }


        public static Material GetTransparentMaterial()
        {
            return new Material(@"Shader ""Unlit/Transparent"" {
                Properties {
                    _MainTex (""Base (RGB)"", 2D) = ""white"" {}
                }
                SubShader {
	                Tags {""Queue""=""Overlay+1000"" ""IgnoreProjector""=""True"" ""RenderType""=""Transparent""}
                    Pass {
                        ZTest Always
                        Lighting Off
                        AlphaTest Greater 0.1
                        Blend SrcAlpha OneMinusSrcAlpha
                        SetTexture [_MainTex] { combine texture }
                    }
                }
            }");
        }

        public static Material GetTransparentMaterial2()
        {
            return new Material(@"Shader ""Unlit/Transparent"" {
                Properties {
                    _MainTex (""Base (RGB)"", 2D) = ""white"" {}
                    _SubTex (""Base (RGB)"", 2D) = ""white"" {}
                }
                SubShader {
	                Tags {""Queue""=""Overlay+1000"" ""IgnoreProjector""=""True"" ""RenderType""=""Transparent""}
                    Pass {
                        ZTest Always
                        Lighting Off
                        AlphaTest Greater 0.1
                        Blend SrcAlpha OneMinusSrcAlpha
                        SetTexture [_MainTex] { combine texture, texture + texture }
                        SetTexture [_SubTex] { combine texture lerp(texture) previous }

                    }
                }
            }");
        }

        public static void Dump(TextWriter writer)
        {
            //Find topmost parent
            writer.Write("[");

            // find all root transforms
            foreach (var gameObject in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (gameObject.transform.parent == null)
                {
                    Analyze(gameObject.transform, 0, writer);
                }
            }

            //Analyze(cam.transform.root, 0, writer);

            writer.Write("]");
        }


        public static void Analyze(Transform node, int depth, TextWriter writer)
        {
            writer.Write(@"{{ ""name"": ""{0}"", ""tag"": ""{1}"", ""layer"": ""{2}"", ""active"": {3}, ""position"": ""{4}"", ""components"": [", node.name, node.tag, LayerMask.LayerToName(node.gameObject.layer), node.gameObject.activeInHierarchy.ToString().ToLower(), node.localPosition);


            // Analyze components
            foreach (var component in node.GetComponents<Component>())
            {
                writer.Write("\"{0}\", ", component.GetType().Name);
            }
            writer.Write("], \"children\": [");

            for (int i = 0; i < node.childCount; i++)
            {
                var child = node.GetChild(i);
                Analyze(child, depth + 1, writer);
            }

            writer.Write("]},");
        }

    }
}
