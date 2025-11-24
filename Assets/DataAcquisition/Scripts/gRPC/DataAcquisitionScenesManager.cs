using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace DataAcquisition
{
    public class DataAcquisitionScenesManager : Singleton<DataAcquisitionScenesManager>
    {
        public UnityEvent event_DataAcquisitionSceneLoaded;

        void Start()
        {
            SessionType.Instance.event_onDataAcquisitionSessionLaunched.AddListener(LoadDataAcquisitionScene);
            SessionType.Instance.event_onDataAcquisitionSessionAborted.AddListener(UnloadDataAcquisitionScene);
            SessionType.Instance.event_onBasicControlSessionSelected.AddListener(UnloadDataAcquisitionScene);

            TeleopReachy.EventManager.StartListening(TeleopReachy.EventNames.EnterConnectionScene, UnloadDataAcquisitionScene);
        }

        public void LoadDataAcquisitionScene()
        {
            StartCoroutine(LoadDataAcquisitionSceneCoroutine());
        }

        IEnumerator LoadDataAcquisitionSceneCoroutine()
        {
            SceneManager.LoadScene("DataAcquisitionScene", LoadSceneMode.Additive);
            yield return null;
            event_DataAcquisitionSceneLoaded.Invoke();
        }

        private void UnloadDataAcquisitionScene()
        {
            if (SceneManager.GetSceneByName("DataAcquisitionScene").isLoaded) 
            {
                SceneManager.UnloadSceneAsync("DataAcquisitionScene");
            }
        }
    }
}
