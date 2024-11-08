using UnityEngine;

namespace TeleopReachyXR
{
    public class CanvasInitOverlay : MonoBehaviour
    {
        [SerializeField]
        private float PlaneDistance;

        void Start()
        {
            // Assigne la caméra de Basescene au canva courant
            transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            transform.GetComponent<Canvas>().worldCamera = Camera.main;

            if (PlaneDistance != 0) transform.GetComponent<Canvas>().planeDistance = PlaneDistance;
        }

    }
}
