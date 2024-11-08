using UnityEngine;

namespace TeleopReachy
{
    public class LoaderBeforeStartManager : MonoBehaviour
    {
        [SerializeField]
        private Transform loaderA;

        private MirrorSceneManager sceneManager;

        void Start()
        {
            sceneManager = MirrorSceneManager.Instance;
        }  

        void Update()
        {
            loaderA.GetComponent<UnityEngine.UI.Image>().fillAmount = sceneManager.indicatorTimer;
        }
    }
}

