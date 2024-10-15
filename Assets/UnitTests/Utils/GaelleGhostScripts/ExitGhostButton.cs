using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class ExitGhostButton : MonoBehaviour
    {
        [SerializeField]
        public Button exitButton;

        private MirrorSceneManager sceneManager;

        void Start()
        {
            Button btn = exitButton.GetComponent<Button>();
		    btn.onClick.AddListener(ExitMirrorScene);

            sceneManager = MirrorSceneManager.Instance;
        }

        void ExitMirrorScene()
        {
            sceneManager.BackToConnectionScene();
        }
    }
}