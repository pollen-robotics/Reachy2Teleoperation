using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Data.Acquisition;


namespace DataAcquisition
{
    public class TagModifier : MonoBehaviour
    {
        [SerializeField]
        private Texture uploaded;
        [SerializeField]
        private Texture partiallyUploaded;
        [SerializeField]
        private Texture notUploaded;

        private bool needUpdate = false;
        private Texture tag;

        void Update()
        {
            if (needUpdate)
            {
                needUpdate = false;
                transform.GetComponent<RawImage>().texture = tag;
            }
        }

        public void SelectTag(DatasetPushState state)
        {
            switch (state)
            {   
                case DatasetPushState.Pushed:
                    SetUploaded();
                    break;
                case DatasetPushState.LocalOnly:
                    SetNotUploaded();
                    break;
                case DatasetPushState.PartiallyPushed:
                    SetPartiallyUploaded();
                    break;
            }
        }

        private void SetUploaded()
        {
            needUpdate = true;
            tag = uploaded;
        }

        private void SetPartiallyUploaded()
        {
            needUpdate = true;
            tag = partiallyUploaded;
        }

        private void SetNotUploaded()
        {
            needUpdate = true;
            tag = notUploaded;
        }
    }
}
