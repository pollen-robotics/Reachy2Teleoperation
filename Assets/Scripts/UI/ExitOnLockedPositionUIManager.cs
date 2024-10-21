using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class ExitOnLockedPositionUIManager : MonoBehaviour
    {
        [SerializeField]
        private Button cancelLeaveRoomButton;
        [SerializeField]
        private Button validateLeaveRoomButton;

        [SerializeField]
        private Transform loader;

        [SerializeField]
        private GameObject beforeValidateElements;

        [SerializeField]
        private GameObject afterValidateElements;

        Coroutine rotateLoader;

        void Awake()
        {
            cancelLeaveRoomButton.onClick.AddListener(Cancel);
            validateLeaveRoomButton.onClick.AddListener(QuitTransitionRoom);
        }

        void OnDestroy()
        {
            if (rotateLoader != null) StopCoroutine(rotateLoader);
        }

        void QuitTransitionRoom()
        {
            beforeValidateElements.SetActive(false);
            afterValidateElements.SetActive(true);
            rotateLoader = StartCoroutine(RotateLoader(3));
        }

        private IEnumerator RotateLoader(float duration)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                loader.transform.Rotate(0, 0, -7, Space.Self);
                yield return null;
            }
        }

        void Cancel()
        {
            transform.ActivateChildren(false);
        }
    }
}