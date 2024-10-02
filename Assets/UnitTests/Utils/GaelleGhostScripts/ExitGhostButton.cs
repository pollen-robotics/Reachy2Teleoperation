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

        private TransitionRoomManager transitionRoomManager;

        void Start()
        {
            Button btn = exitButton.GetComponent<Button>();
		    btn.onClick.AddListener(ExitMirrorScene);

            transitionRoomManager = TransitionRoomManager.Instance;
        }

        void ExitMirrorScene()
        {
            transitionRoomManager.BackToConnectionScene();
        }
    }
}