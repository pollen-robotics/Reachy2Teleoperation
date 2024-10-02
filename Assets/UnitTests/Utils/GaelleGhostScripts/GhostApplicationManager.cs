using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class GhostApplicationManager : MonoBehaviour
    {
        public UnityEvent event_BaseSceneLoaded;
        void Start()
        {
            QualitySettings.vSyncCount = 0;  // Disable VSync
            Application.targetFrameRate = 120;

            Debug.LogError("VSync Count: " + QualitySettings.vSyncCount);
            Debug.LogError("Target Frame Rate: " + Application.targetFrameRate);

            // By default soft team asked for mobile base off
            bool mobility_default_mode = false;
            PlayerPrefs.SetString("mobility_on", mobility_default_mode.ToString());

            StartCoroutine(LoadBaseScene());
        }

        IEnumerator LoadBaseScene()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BaseScene", LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            event_BaseSceneLoaded.Invoke();
            EventManager.StartListening(EventNames.QuitMirrorScene, OnConnectToRobot);
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, LoadMirrorScene);
            EventManager.StartListening(EventNames.BackToMirrorScene, ReturnToMirrorScene);
        
            EventManager.StartListening(EventNames.LoadConnectionScene, ReturnToConnectionScene);
        }

        private void LoadMirrorScene()
        {
            StartCoroutine(LoadTransitionRoom());
        }

        private void ReturnToMirrorScene()
        {
            StartCoroutine(BackToMirrorScene());
        }

        IEnumerator BackToMirrorScene()
        {
            SceneManager.UnloadSceneAsync("Test_GaelleGhostTeleoperationScene");
            SceneManager.LoadScene("Test_GaelleGhostMirrorScene", LoadSceneMode.Additive);
            yield return null;
        }

        private void OnConnectToRobot()
        {
            SceneManager.UnloadSceneAsync("Test_GaelleGhostMirrorScene");
            SceneManager.LoadScene("Test_GaelleGhostTeleoperationScene", LoadSceneMode.Additive);
        }

        IEnumerator LoadTransitionRoom()
        {
            SceneManager.LoadScene("Test_GaelleGhostMirrorScene", LoadSceneMode.Additive);
            yield return null;
        }

        private void ReturnToConnectionScene()
        {
            SceneManager.UnloadSceneAsync("Test_GaelleGhostMirrorScene");
        }
    }
}
