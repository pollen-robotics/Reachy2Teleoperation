using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

using Data.Acquisition;
using TeleopReachy;

namespace DataAcquisition
{
    public class RecordingSessionManager : PagesManager
    {
        private int currentEpisode = 0;
        private bool skipAllowed = true;
        private bool skipRequested = false;
        private bool endRequested = false;
        private bool earlyEndAlreadyAsked = false;

        private bool suspendTime = false;
        private bool pushSession = true;

        private Coroutine saveEpisodeCoroutine = null;
        private bool episodeSaved = false;

        private bool sessionCycleStarted = false;

        private TeleopReachy.ControllersManager controllers;
        private bool rightPrimaryButtonPreviouslyPressed;
        private bool rightSecondaryButtonPreviouslyPressed;
        private bool leftPrimaryButtonPreviouslyPressed;
        private float leftGripPreviousValue;

        public UnityEvent<bool> event_OnPushOver;
        public UnityEvent<bool> event_OnConsolidationOver;
        public UnityEvent<bool> event_OnStartSessionOver;
        public UnityEvent event_OnEpisodeSaved;
        public UnityEvent event_OnEpisodeSavingStart;

        public bool pushRequested = false;
        public bool sessionStartSucceeded = false;

        public bool FirstCycle { get; private set; }
        public bool SaveEpisode { get; private set; }

        private Phase currentPhase = Phase.None;
        private HeadsetRemovedStep currentHeadsetRemovedStep = HeadsetRemovedStep.None;

        protected enum Phase
        {
            EpisodeRecording, BreakTime, EpisodeStartDelay, EpisodeSaving, None
        }

        protected enum HeadsetRemovedStep
        {
            Step1, Step2, None
        }

        void Start()
        {
            FirstCycle = true;
            SaveEpisode = true;
            controllers = TeleopReachy.ControllersManager.Instance;
            rightPrimaryButtonPreviouslyPressed = true;
            rightSecondaryButtonPreviouslyPressed = true;

            TeleopReachy.TeleoperationManager.Instance.CustomSuspensionImplemented = true;

            EventManager.StartListening(TeleopReachy.EventNames.OnStartArmTeleoperation, StartRecordingCycle);
            EventManager.StartListening(EventNames.HeadsetRemoved, HeadsetRemoved);
            EventManager.StartListening(EventNames.OnEmergencyStop, EmergencyStopActivated);
        }

        public async void StartRecordingSession()
        {
            ActionAck ack = await DataAcquisitionManager.Instance.DataController.StartSession();
            sessionStartSucceeded = ack.SuccessAck;
            event_OnStartSessionOver.Invoke(sessionStartSucceeded);
        }

        public void StartRecordingCycle()
        {
            currentEpisode = 1;
            if (sessionStartSucceeded)
            {
                // if (TeleopReachy.RobotDataManager.Instance.RobotStatus.HasMotorsSpeedLimited())
                // {
                //     TeleopReachy.RobotDataManager.Instance.RobotStatus.event_OnRobotMotorsFullSpeed.AddListener(RunSessionCycle);
                // }
                // else
                // {
                StartCoroutine(SessionCycle());
                // }
            }

            // StartCoroutine(RunSessionCycle()); // For test only
        }

        void Update()
        {
            if (sessionCycleStarted && !suspendTime)
            {
                bool rightPrimaryButtonPressed = false;
                bool rightSecondaryButtonPressed = false;

                // Press space to skip current phase
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (!(currentPhase == Phase.EpisodeSaving && suspendTime)) RequestSkip();
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    endRequested = true;
                }

                // Press A to skip current phase
                if (controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed) && rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
                {
                    if (!(currentPhase == Phase.EpisodeSaving && suspendTime)) RequestSkip();
                }
                if (controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out rightSecondaryButtonPressed) && rightSecondaryButtonPressed && !rightSecondaryButtonPreviouslyPressed)
                {
                    endRequested = true;
                }
                rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
                rightSecondaryButtonPreviouslyPressed = rightSecondaryButtonPressed;
            }
            else
            {
                if (currentHeadsetRemovedStep == HeadsetRemovedStep.Step1)
                {
                    bool rightPrimaryButtonPressed = false;
                    if (controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed) && rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
                    {
                        GetPanelByName("HeadsetRemovedPanel").GetComponent<OrderedPagesManager>().NextPage();
                        EventManager.TriggerEvent(EventNames.OnFixUserOrigin);
                        UserTrackerManager.Instance.ShowXAxis(true);
                        currentHeadsetRemovedStep = HeadsetRemovedStep.Step2;
                    }
                    rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
                }
                if (currentHeadsetRemovedStep == HeadsetRemovedStep.Step2)
                {
                    bool rightPrimaryButtonPressed = false;
                    if (controllers.rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightPrimaryButtonPressed) && rightPrimaryButtonPressed && !rightPrimaryButtonPreviouslyPressed)
                    {
                        UserTrackerManager.Instance.ShowXAxis(false);
                        GetPanelByName("HeadsetRemovedPanel").GetComponent<OrderedPagesManager>().NextPage();
                        ClosePanelByName("HeadsetRemovedPanel");
                        currentHeadsetRemovedStep = HeadsetRemovedStep.None;
                        int layerUI = LayerMask.NameToLayer("UI");
                        currentOpenPage.parent.switchLayer(layerUI);
                        ResumeCurrentPhase();
                    }
                    rightPrimaryButtonPreviouslyPressed = rightPrimaryButtonPressed;
                }
            }

            if (episodeSaved)
            {
                episodeSaved = false;
                skipAllowed = true;
                saveEpisodeCoroutine = null;
                event_OnEpisodeSaved.Invoke();
            }

            bool leftPrimaryButtonPressed = false;
            float leftGripValue;
            if (currentPhase == Phase.BreakTime)
            {
                if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed) && leftPrimaryButtonPressed && !leftPrimaryButtonPreviouslyPressed)
                {
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.ResumeRobotTeleoperation();
                }
                else if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftPrimaryButtonPressed) && !leftPrimaryButtonPressed && leftPrimaryButtonPreviouslyPressed)
                {
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.SuspendRobotTeleoperation();
                }
                leftPrimaryButtonPreviouslyPressed = leftPrimaryButtonPressed;

                if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out leftGripValue) && (leftGripPreviousValue <= 0.5f) && (leftGripValue > 0.5f))
                {
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.ResumeRobotTeleoperation();
                }
                else if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out leftGripValue) && (leftGripPreviousValue > 0.5f) && (leftGripValue <= 0.5f))
                {
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.SuspendRobotTeleoperation();
                }
                leftGripPreviousValue = leftGripValue;
            }
        }

        private void RunSessionCycle()
        {
            FirstCycle = true;
            StartCoroutine(SessionCycle());
        }

        IEnumerator SessionCycle()
        {
            // TeleopReachy.RobotDataManager.Instance.RobotStatus.event_OnRobotMotorsFullSpeed.RemoveListener(RunSessionCycle);
            while (saveEpisodeCoroutine != null)
            {
                yield return null;
            }
            sessionCycleStarted = true;

            while (currentEpisode <= RecordingSessionParameters.Instance.NbEpisodesGoal && !endRequested)
            {
                float startDelay = RecordingSessionParameters.Instance.StartDelay + 0.5f;
                if (FirstCycle)
                {
                    startDelay += 3.0f;
                }
                yield return RunPhase(
                    Phase.EpisodeStartDelay,
                    "RecordingStart",
                    startDelay
                    );
                yield return RunPhase(Phase.EpisodeRecording, "RecordingTimer", RecordingSessionParameters.Instance.EpisodeDuration);
                FirstCycle = false;

                yield return RunPhase(Phase.EpisodeSaving, "SaveEpisode", 5.0f);
                if (SaveEpisode)
                {
                    if (currentEpisode < RecordingSessionParameters.Instance.NbEpisodesGoal)
                    {
                        yield return RunPhase(Phase.BreakTime, "BreakTime", RecordingSessionParameters.Instance.BreakTimeDuration);
                    }
                    currentEpisode++;
                }
                else
                {
                    yield return RunPhase(Phase.BreakTime, "BreakTime", RecordingSessionParameters.Instance.BreakTimeDuration);
                }
            }
            if (!endRequested) currentEpisode--;
            OpenPageByName("GoalCompleted"); // Final panel
            if (currentEpisode == RecordingSessionParameters.Instance.NbEpisodesGoal)
            {
                TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.ShowXRay);
                Task stopEpisodeTask = DataAcquisitionManager.Instance.DataController.StopEpisode();
                yield return new WaitUntil(() => stopEpisodeTask.IsCompleted);
                if (SaveEpisode) 
                {
                    skipAllowed = false;
                    saveEpisodeCoroutine = StartCoroutine(DelayedSaveEpisode());
                }
                while (saveEpisodeCoroutine != null)
                {
                    yield return null;
                }
            }
        }

        IEnumerator RunPhase(Phase phase, string panelToShow, float duration)
        {
            currentPhase = phase;
            OpenPageByName(panelToShow);
            do
            {
                if (phase == Phase.EpisodeStartDelay)
                {
                    TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.HideXRay);
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.ResumeRobotTeleoperation();
                }
                else if (phase == Phase.EpisodeRecording)
                {
                    if (RobotDataManager.Instance.RobotStatus.AreRobotMovementsSuspended())
                    {
                        skipRequested = true;
                        break;
                    }
                    TeleopReachy.EmotionMenuManager.Instance.ActivateEmotion();
                    Task startEpisodeTask = DataAcquisitionManager.Instance.DataController.StartEpisode();
                    yield return new WaitUntil(() => startEpisodeTask.IsCompleted);
                    SaveEpisode = true;
                }
                else if (phase == Phase.EpisodeSaving)
                {
                    TeleopReachy.EmotionMenuManager.Instance.DeactivateEmotion();
                    TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.ShowXRay);
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.SuspendRobotTeleoperation();
                    Task stopEpisodeTask = DataAcquisitionManager.Instance.DataController.StopEpisode();
                    yield return new WaitUntil(() => stopEpisodeTask.IsCompleted);
                }
                else if (phase == Phase.BreakTime)
                {
                    if (SaveEpisode) 
                    {
                        skipAllowed = false;
                        saveEpisodeCoroutine = StartCoroutine(DelayedSaveEpisode());
                    }
                }
            }
            while (false);

            float elapsed = 0f;
            skipRequested = false;
            endRequested = false;

            if (duration != 0)
            {
                while (elapsed < duration && !skipRequested)
                {
                    if (!suspendTime) elapsed += Time.deltaTime;
                    if (phase == Phase.BreakTime && endRequested)
                    {
                        EarlyEndSession();
                    }
                    yield return null;
                }
            }
            else
            {
                while (!skipRequested)
                {
                    if (phase == Phase.BreakTime && endRequested)
                    {
                        EarlyEndSession();
                    }
                    yield return null;
                }
            }
            
            while (saveEpisodeCoroutine != null)
            {
                yield return null;
            }
        }

        IEnumerator DelayedSaveEpisode()
        {
            // yield return new WaitForSeconds(5.0f);
            if (SaveEpisode)
            {
                Task saveEpisodeTask = DataAcquisitionManager.Instance.DataController.SaveEpisode();
                event_OnEpisodeSavingStart.Invoke();
                yield return new WaitUntil(() => saveEpisodeTask.IsCompleted);
                episodeSaved = true;
            }
        }

        private void EarlyEndSession()
        {
            if (!earlyEndAlreadyAsked)
            {
                earlyEndAlreadyAsked = true;
                SuspendCurrentPhase();
                OpenPanelByName("PushSessionDataPanel");
            }
        }

        public void SuspendCurrentPhase()
        {
            suspendTime = true;
            currentOpenPage.GetComponentInChildren<CountdownWithPulse>().SuspendCountdown();
        }

        public void ResumeCurrentPhase()
        {
            suspendTime = false;
            endRequested = false;
            earlyEndAlreadyAsked = false;
            if (currentOpenPage.GetComponentInChildren<CountdownWithPulse>() != null) currentOpenPage.GetComponentInChildren<CountdownWithPulse>().ResumeCountdown();
        }

        public void ContinueRecordingSession(int nbAdditionalEpisodes)
        {
            DataAcquisitionManager.Instance.RecordingSessionParameters.UpdateNbEpisodeGoal(nbAdditionalEpisodes);
            currentEpisode++;
            StartCoroutine(SessionCycle());
        }

        public int GetCurrentEpisode()
        {
            return currentEpisode;
        }

        public void SetBackPreviousEpisode()
        {
            SaveEpisode = false;
        }

        public void DoNotPushSession()
        {
            pushSession = false;
        }

        public void PushSession()
        {
            pushSession = true;
        }

        public async void StopSession()
        {
            endRequested = true;
            if (pushSession)
            {
                if (saveEpisodeCoroutine == null) StopAndPushDataFromSession();
                else this.event_OnEpisodeSaved.AddListener(StopAndPushDataFromSession);
            }
            else
            {
                if (saveEpisodeCoroutine == null) StopWithoutPushingDataFromSession();
                else this.event_OnEpisodeSaved.AddListener(StopWithoutPushingDataFromSession);
            }
        }

        public async void StopWithoutPushingDataFromSession()
        {
            this.event_OnEpisodeSaved.AddListener(StopWithoutPushingDataFromSession);
            ActionAck ack = await DataAcquisitionManager.Instance.DataController.StopSession();
            event_OnConsolidationOver.Invoke(ack.SuccessAck);
            DataAcquisitionManager.Instance.DataController.UpdateDataset(DatasetPushState.LocalOnly);
        }

        public async void StopAndPushDataFromSession()
        {
            this.event_OnEpisodeSaved.AddListener(StopAndPushDataFromSession);
            pushRequested = true;
            ActionAck ack = await DataAcquisitionManager.Instance.DataController.StopSession();
            event_OnConsolidationOver.Invoke(ack.SuccessAck);
            ActionAck pushAck = await DataAcquisitionManager.Instance.DataController.PushDataFromSession();
            event_OnPushOver.Invoke(pushAck.SuccessAck);
            DataAcquisitionManager.Instance.DataController.UpdateDataset(DatasetPushState.Pushed);
        }

        public void RequestSkip()
        {
            if (skipAllowed) skipRequested = true;
        }

        public void LeaveTeleoperationScene()
        {
            TeleopReachy.EventManager.TriggerEvent(TeleopReachy.EventNames.QuitTeleoperationScene);
            SessionType.Instance.SelectBasicControlSession();
        }

        void EmergencyStopActivated()
        {
            SuspendTeleoperation();
        }

        void HeadsetRemoved()
        {
            switch (currentPhase)
            {
                case Phase.BreakTime:
                    SuspendCurrentPhase();
                    OpenPanelByName("HeadsetRemovedPanel");
                    int layerNotVisible = LayerMask.NameToLayer("NotVisible");
                    currentOpenPage.parent.switchLayer(layerNotVisible);
                    currentHeadsetRemovedStep = HeadsetRemovedStep.Step1;
                    break;
                case Phase.EpisodeSaving:
                    SuspendCurrentPhase();
                    break;
                case Phase.EpisodeRecording:
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.SuspendRobotTeleoperation();
                    GoToSafePose();
                    SetBackPreviousEpisode();
                    RequestSkip();
                    // TODO
                    // Skip, don't save and go to BreakTime in pause
                    break;
                case Phase.EpisodeStartDelay:
                    TeleopReachy.RobotDataManager.Instance.RobotStatus.SuspendRobotTeleoperation();
                    GoToSafePose();
                    SetBackPreviousEpisode();
                    RequestSkip();
                    // TODO
                    // Skip, don't save and go to BreakTime in pause
                    break;
                case Phase.None:
                    SuspendTeleoperation();
                    break;
            }
        }

        void SuspendTeleoperation()
        {
            if (TeleoperationManager.Instance.IsRobotTeleoperationActive) EventManager.TriggerEvent(EventNames.OnSuspendTeleoperation);
        }

        void GoToSafePose()
        {
            // Here goto elbow_90 pose for example
        }
    }
}
