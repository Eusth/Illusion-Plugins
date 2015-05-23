using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace HaremOVR
{
    public class Settings
    {
        private static Settings settings;
        private Settings oldSettings;

        public float Distance = 0.3f;
        public float Angle = 170f;
        public float IPDScale = 1f;
        public float OffsetY = 0f;
        
        private readonly string path;


        private Settings(string path)
        {
            this.path = path;

            if (!File.Exists(path))
            {
                new XDocument(
                    new XElement("Setting")
                ).Save(path);
            }
            // File found. Load settings.
            var setupFile = XDocument.Load(path).Root;
                
            var angle = setupFile.Elements("ScreenAngle").FirstOrDefault();
            var ipd = setupFile.Elements("IPDScale").FirstOrDefault();
            var distance = setupFile.Elements("Distance").FirstOrDefault();
            var offset = setupFile.Elements("OffsetY").FirstOrDefault();

            if (angle != null) Angle = ParseFloat(angle.Value, Angle);
            if (ipd != null) IPDScale = ParseFloat(ipd.Value, IPDScale);
            if (distance != null) Distance = ParseFloat(distance.Value, Distance);
            if (offset != null) OffsetY = ParseFloat(offset.Value, OffsetY);

            //Console.WriteLine("Loaded values: {0} {1} {2}", Angle, IPDScale, Distance);
            
            oldSettings = this.MemberwiseClone() as Settings;
        }

        public void Save()
        {
            var setupFile = XDocument.Load(path).Root;
            
            UpdateOrCreateNode(setupFile, "ScreenAngle", Angle);
            UpdateOrCreateNode(setupFile, "IPDScale", IPDScale);
            UpdateOrCreateNode(setupFile, "Distance", Distance);
            UpdateOrCreateNode(setupFile, "OffsetY", OffsetY);

            setupFile.Save(path);

            oldSettings = settings.MemberwiseClone() as Settings;
        }

        public void Reset()
        {
            Distance = oldSettings.Distance;
            Angle = oldSettings.Angle;
            IPDScale = oldSettings.IPDScale;
            OffsetY = oldSettings.OffsetY;
        }

        private void UpdateOrCreateNode(XElement root, string name, float @value)
        {
            var node = root.Elements(name).FirstOrDefault();
            if (node == null)
            {
                node = new XElement(name, @value);
                root.Add(node);
            }
            else
            {
                node.SetValue(@value);
            }
        }

        private float ParseFloat(string val, float @default) {
            float result;
            if (!float.TryParse(val, out result))
            {
                return @default;
            }
            return result;
        }

        public static Settings Instance
        {
            get
            {
                if (settings == null) settings = new Settings(Path.Combine(Helper.DocRoot, "UserData/ovr.xml"));
                return settings;
            }
        }
    }
}
