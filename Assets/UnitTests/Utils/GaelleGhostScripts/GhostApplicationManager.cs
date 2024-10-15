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
            EventManager.StartListening(EventNames.EnterMirrorScene, LoadGhostMirrorScene);
            EventManager.StartListening(EventNames.QuitMirrorScene, UnloadGhostMirrorScene);

            EventManager.StartListening(EventNames.EnterTeleoperationScene, LoadGhostTeleoperationScene);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, UnloadGhostTeleoperationScene);
        }

        private void UnloadGhostMirrorScene()
        {
            SceneManager.UnloadSceneAsync("Test_GaelleGhostMirrorScene");
        }

        private void UnloadGhostTeleoperationScene()
        {
            SceneManager.UnloadSceneAsync("Test_GaelleGhostTeleoperationScene");
        }

        private void LoadGhostMirrorScene()
        {
            StartCoroutine(LoadGhostMirrorSceneCo());
        }

        IEnumerator LoadGhostMirrorSceneCo()
        {
            while(!SceneManager.GetSceneByName("RobotDataScene").isLoaded)
            {
                yield return null;
            }
            SceneManager.LoadScene("Test_GaelleGhostMirrorScene", LoadSceneMode.Additive);
            yield return null;
        }

        private void LoadGhostTeleoperationScene()
        {
            SceneManager.LoadScene("Test_GaelleGhostTeleoperationScene", LoadSceneMode.Additive);
        }
    }
}
