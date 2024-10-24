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

        private TeleoperationManager teleoperationManager;

        void Start()
        {
            QualitySettings.vSyncCount = 0;  // Disable VSync
            Application.targetFrameRate = 120;

            Debug.LogError("VSync Count: " + QualitySettings.vSyncCount);
            Debug.LogError("Target Frame Rate: " + Application.targetFrameRate);

            // By default soft team asked for mobile base off
            // bool mobility_default_mode = false;
            // PlayerPrefs.SetString("mobility_on", mobility_default_mode.ToString());

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
            teleoperationManager = TeleoperationManager.Instance;
            EventManager.StartListening(EventNames.EnterConnectionScene, UnloadGhostMirrorScene);
            EventManager.StartListening(EventNames.QuitConnectionScene, LoadGhostMirrorScene);

            EventManager.StartListening(EventNames.EnterTeleoperationScene, UnloadGhostMirrorSceneAndLoadTeleoperationScene);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, UnloadGhostTeleoperationSceneAndLoadGhostMirrorScene);
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, StartArmTeleop);
        }

        private void UnloadGhostMirrorScene()
        {
            if(SceneManager.GetSceneByName("Test_GaelleGhostMirrorScene").isLoaded)
            {
                SceneManager.UnloadSceneAsync("Test_GaelleGhostMirrorScene");
            }
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

        private void UnloadGhostMirrorSceneAndLoadTeleoperationScene()
        {
            UnloadGhostMirrorScene();
            SceneManager.LoadScene("Test_GaelleGhostTeleoperationScene", LoadSceneMode.Additive);
        }

        private void StartArmTeleop()
        {
            teleoperationManager.AskForStartingArmTeleoperation();
        }

        private void UnloadGhostTeleoperationSceneAndLoadGhostMirrorScene()
        {
            UnloadGhostTeleoperationScene();
            LoadGhostMirrorScene();
        }
    }
}
