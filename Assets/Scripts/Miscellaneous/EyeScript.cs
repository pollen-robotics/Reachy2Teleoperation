using UnityEngine;


namespace TeleopReachy
{
    public class EyeScript : MonoBehaviour
    {
        private bool needColorUpdate = false;

        Renderer rend;

        float alpha = 1.0f;

        private ControllersManager controllers;

        void Start()
        {
            controllers = ActiveControllerManager.Instance.ControllersManager;
            if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            {
                Debug.Log("Oculus 2 detected");
                transform.position = new Vector3(-159.0f, -595.0f, 18473.0f);
            }
            else
            {
                Debug.Log("Oculus 3 or other detected");
            }
        }

        public void SetImageTransparent()
        {
            alpha = 0.5f;
            needColorUpdate = true;
        }

        public void SetImageOpaque()
        {
            alpha = 1.0f;
            needColorUpdate = true;
        }

        void Update()
        {
            if (needColorUpdate)
            {
                rend = GetComponent<Renderer>();
                Color color = new Color(1, 1, 1, alpha);
                rend.material.SetColor("_Color", color);
                needColorUpdate = false;
            }
        }
    }
}