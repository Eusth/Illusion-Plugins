using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AnanNaitegoran
{
    public class GUIHelper : MonoBehaviour
    {
        void OnGUI()
        {
            if (Input.GetKey(KeyCode.F8))
            {
                GUI.Label(new Rect(10, 10, 500, 60), AnanOneVoice.InfoText);
            }
            if (Input.GetKey(KeyCode.F7))
            {
                GUI.Label(new Rect(10f, 100f, 500f, 60f), "test");
            }
        }
    }
}
