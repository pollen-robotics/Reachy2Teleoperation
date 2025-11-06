using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace DataAcquisition
{
    public class DataAcquisitionScenesManager : Singleton<DataAcquisitionScenesManager>
    {
        void Start()
        {
            SessionType.Instance.event_onDataAcquisitionSessionLaunched.AddListener(LoadDataAcquisitionScene);
            SessionType.Instance.event_onBasicControlSessionSelected.AddListener(UnloadDataAcquisitionScene);

            TeleopReachy.EventManager.StartListening(TeleopReachy.EventNames.EnterConnectionScene, UnloadDataAcquisitionScene);
        }

        public void LoadDataAcquisitionScene()
        {
            SceneManager.LoadSceneAsync("DataAcquisitionScene", LoadSceneMode.Additive);
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
