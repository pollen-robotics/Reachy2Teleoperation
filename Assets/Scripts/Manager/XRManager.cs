using System.Collections;
using UnityEngine;

using UnityEngine.XR.Management;

namespace TeleopReachy
{
    public class XRManager : MonoBehaviour
    {
        public IEnumerator StartXRCoroutine()
        {
            Debug.Log("Initializing XR...");

            while (XRGeneralSettings.Instance == null)
            {
                Debug.Log("Wait for XR...");
                yield return new WaitForSeconds(0.2f);
            }


            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            }
            else
            {
                Debug.Log("Starting XR...");
                XRGeneralSettings.Instance.Manager.StartSubsystems();
                Debug.Log("XR started");
            }
        }

        void Start()
        {
            StartCoroutine("StartXRCoroutine");
        }

        void OnDestroy()
        {
            Debug.Log("Stopping XR...");

            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            Debug.Log("XR stopped completely.");
        }
    }
}