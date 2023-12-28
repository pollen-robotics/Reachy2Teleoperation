using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;


namespace TeleopReachy
{
    public class TimerSpeedManager : LazyFollow
    {
        private RobotStatus robotStatus;

        Coroutine timerCoroutine;
        Coroutine rotateLoader;

        // private ControllersManager controllers;
        void Start()
        {
            
            // controllers = ActiveControllerManager.Instance.ControllersManager;
            // if (controllers.headsetType == ControllersManager.SupportedDevices.Oculus) // If oculus 2
            // {
            //     targetOffset = new Vector3(0, -0.15f, 0.5f);
            // }
            // else{ // If oculus 3 or other
            targetOffset = new Vector3(0, -0.15f, 0.8f);
            // }
            maxDistanceAllowed = 0;

            timerCoroutine = null;
            rotateLoader = null;

            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotStatus.event_OnStartTeleoperation.AddListener(StartTimer);
            robotStatus.event_OnStopTeleoperation.AddListener(StopTimer);

            transform.ActivateChildren(false);
        }

        IEnumerator TimerCountdown()
        {
            transform.ActivateChildren(true);
            transform.GetChild(3).GetComponent<Text>().enabled = true;
            rotateLoader = StartCoroutine(RotateLoader(3));
            transform.GetChild(1).GetComponent<Text>().text = "3";
            yield return new WaitForSeconds(1);
            transform.GetChild(1).GetComponent<Text>().text = "2";
            yield return new WaitForSeconds(1);
            transform.GetChild(1).GetComponent<Text>().text = "1";
            yield return new WaitForSeconds(1);
            transform.GetChild(3).GetComponent<Text>().enabled = false;
            transform.GetChild(1).GetComponent<Text>().text = "Go!";
            yield return new WaitForSeconds(1);
            transform.ActivateChildren(false);
            timerCoroutine = null;
        }

        private IEnumerator RotateLoader(float duration)
        {
            GameObject loader = transform.GetChild(2).gameObject;
            loader.transform.GetComponent<RawImage>().enabled = true;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                loader.transform.Rotate(0, 0, -7, Space.Self);
                yield return null;
            }
            loader.transform.GetComponent<RawImage>().enabled = false;
        }

        void StartTimer()
        {
            timerCoroutine = StartCoroutine(TimerCountdown());
        }

        void StopTimer()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(rotateLoader);
                StopCoroutine(timerCoroutine);
            }
            transform.ActivateChildren(false);
        }
    }
}