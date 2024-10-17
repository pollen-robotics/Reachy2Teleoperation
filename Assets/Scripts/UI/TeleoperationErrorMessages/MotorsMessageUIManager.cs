using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace TeleopReachy
{
    public class MotorsMessageUIManager : InformationalPanel
    {
        [SerializeField]
        private Text errorText;

        private RobotErrorManager errorManager;

        private int nbMotorsWarning = 0;
        private int nbMotorsError = 0;

        private Coroutine motorsWarningValue;
        private Coroutine motorsErrorValue;

        private string errorTextToDisplay;

        private bool hasMotorsError;
        private bool needNbMotorsUpdate;

        void Start()
        {
            SetOculusTargetOffset(new Vector3(0, -0.27f, 0.8f));

            errorManager = RobotDataManager.Instance.RobotErrorManager;
            errorManager.event_OnWarningMotorsTemperatures.AddListener(WarningMotorTemperature);
            errorManager.event_OnErrorMotorsTemperatures.AddListener(ErrorMotorTemperature);

            hasMotorsError = false;

            HideInfoMessage();
        }

        protected override void Update()
        {
            if (needInfoPanelUpdate)
            {
                if (hasMotorsError)
                {
                    if (motorsErrorValue != null) StopCoroutine(motorsErrorValue);
                    motorsErrorValue = StartCoroutine(ReinitializeMotorsErrorValue(2));
                    backgroundColor = ColorsManager.error_red;
                }
                else
                {
                    if (motorsWarningValue != null) StopCoroutine(motorsWarningValue);
                    motorsWarningValue = StartCoroutine(ReinitializeMotorsWarningValue(2));
                    backgroundColor = ColorsManager.error_black;
                }
                hasMotorsError = false;
            }
            base.Update();
            if (needNbMotorsUpdate)
            {
                infoText.text = textToDisplay;
                errorText.text = errorTextToDisplay;
                if (infoBackground != null) infoBackground.color = backgroundColor;
            }
        }

        void WarningMotorTemperature(List<string> motors)
        {
            if (motors.Count > nbMotorsWarning)
            {
                nbMotorsWarning = motors.Count;
                textToDisplay = nbMotorsWarning > 1 ? nbMotorsWarning + " Motors are heating up" : "1 Motor is heating up";
                ShowInfoMessage();
            }
        }

        IEnumerator ReinitializeMotorsWarningValue(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            textToDisplay = "No motor is heating up";
            needNbMotorsUpdate = true;
        }

        void ErrorMotorTemperature(List<string> motors)
        {
            nbMotorsError = motors.Count;
            errorTextToDisplay = nbMotorsError > 1 ? nbMotorsError + " Motors in critical error" : "1 Motor in critical error";
            hasMotorsError = true;
            ShowInfoMessage();
        }

        IEnumerator ReinitializeMotorsErrorValue(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            errorTextToDisplay = "No motor in critical error";
            backgroundColor = ColorsManager.error_black;
            needNbMotorsUpdate = true;
        }
    }
}
