using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CameraModifications
{
    public class UI_ShowCanvasGroup : MonoBehaviour
    {
        private CanvasGroup group;
        private bool show = false;
        private List<UI_ShowCanvasGroup> childs = new List<UI_ShowCanvasGroup>();
        private bool parentShow = true;
        public bool IsShow
        {
            get
            {
                return this.show;
            }
        }
        private void Awake()
        {
            this.group = base.gameObject.GetComponent<CanvasGroup>();
        }
        private void Start()
        {
            childs = GetComponentsInChildren<UI_ShowCanvasGroup>().Where(group => group != this).ToList();

            this.Show(this.show);
        }
        public void Show(bool flag)
        {
            this.show = flag;
            this.group.alpha = ((!flag) ? 0f : 1f);
            this.group.blocksRaycasts = flag;
            this.group.interactable = flag;

            foreach (var canvasGroup in group.GetComponentsInChildren<CanvasGroup>())
            {
                canvasGroup.alpha = group.alpha;
                canvasGroup.interactable = flag;
                canvasGroup.blocksRaycasts = flag;
            }

            //foreach (UI_ShowCanvasGroup current in this.childs)
            //{
            //    current.Show_FromParent(flag);
            //}
        }
        public void Show_FromParent(bool flag)
        {
            this.parentShow = flag;
            bool flag2 = this.parentShow && this.show;
            this.group.alpha = ((!flag2) ? 0f : 1f);
            this.group.blocksRaycasts = flag2;
            this.group.interactable = flag2;
            foreach (UI_ShowCanvasGroup current in this.childs)
            {
                current.Show_FromParent(flag2);
            }
        }
    }

}
