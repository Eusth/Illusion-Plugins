using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DebugPlugin
{
    public static class Helper 
    {
        public static Texture2D LoadImage(string filePath)
        {
            string ovrDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Images");
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

        public static string[] GetLayerNames(int mask)
        {
            List<string> masks = new List<string>();
            for (int i = 0; i <= 31; i++) //user defined layers start with layer 8 and unity supports 31 layers
            {
                if ((mask & (1 << i)) != 0) masks.Add(LayerMask.LayerToName(i));
            }
            return masks.Select(m => m.Trim()).Where(m => m.Length > 0).ToArray();
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

        public static void ExportToCollada(string path, Transform root)
        {
            using (var writer = File.CreateText(path))
            {
                // HEADER
                writer.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>
<COLLADA version=""1.5.0"" xmlns=""http://www.collada.org/2008/03/COLLADASchema"">
	<asset>
		<contributor>
			<authoring_tool>mdlanim</authoring_tool>
		</contributor>
		<created>2015-03-05T22:34:03Z</created>
		<modified>2015-03-05T22:34:03Z</modified>
		<unit />
		<up_axis>Y_UP</up_axis>
	</asset>
	<library_visual_scenes>
		<visual_scene id=""RootNode"" name=""RootNode"">");

                WriteNode(writer, root);

                // FOOTER
                writer.Write(@"
        </visual_scene>
	</library_visual_scenes>
	<scene>
		<instance_visual_scene url=""#RootNode"" />
	</scene>
</COLLADA>");

            }

        }

        private static void WriteNode(TextWriter writer, Transform node)
        {
            bool isJoint = node.name.Contains("_J_");
           // if (isJoint)
            {
                writer.Write(@"<node id=""{0}"" name=""{0}"" sid=""{0}"" type=""JOINT"">
<translate sid=""translate"">{1} {2} {3}</translate>
<rotate sid=""rotateY"">0 1 0 {5}</rotate>
<rotate sid=""rotateX"">1 0 0 {4}</rotate>
<rotate sid=""rotateZ"">0 0 1 {6}</rotate>
<scale sid=""scale"">1 1 1</scale>", node.name, node.localPosition.x, node.localPosition.y, node.localPosition.z,
                                                    node.localEulerAngles.x, node.localEulerAngles.y, node.localEulerAngles.z);
            }

            for (int i = 0; i < node.childCount; i++)
            {
                WriteNode(writer, node.GetChild(i));
            }

           // if(isJoint)
                writer.Write("</node>");
        }



        public static void Analyze(Transform node, int depth, TextWriter writer)
        {
            writer.Write(@"{{ ""name"": ""{0}"", ""tag"": ""{1}"", ""layer"": ""{2}"", ""active"": {3}, ""position"": ""{4}""", node.name, node.tag, LayerMask.LayerToName(node.gameObject.layer), node.gameObject.activeInHierarchy.ToString().ToLower(), node.localPosition);

            if (node.GetComponent<Camera>() != null)
            {
                var camera = node.GetComponent<Camera>();
                writer.Write(@", ""culling mask"": ""{0}""", string.Join(", ", Helper.GetLayerNames(camera.cullingMask)));
            }
            if (node.GetComponent<Canvas>() != null)
            {
                var canvas = node.GetComponent<Canvas>();
                writer.Write(@", ""render mode"": ""{0}""", canvas.renderMode);
            }

            if (node.GetComponent<Text>() != null)
            {
                writer.Write(@", ""text"":""{0}""", node.GetComponent<Text>().text);
            }
            if (node.GetComponent<Image>() != null && node.GetComponent<Image>().sprite != null && node.GetComponent<Image>().sprite.texture != null)
            {
                writer.Write(@", ""image"":""{0}""", node.GetComponent<Image>().sprite.texture.name); 
            }

            writer.Write(@", ""components"": [");
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
