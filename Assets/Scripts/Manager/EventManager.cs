using UnityEngine.Events;
using System.Collections.Generic;

// from https://medium.com/geekculture/how-to-use-events-to-implement-a-messaging-system-in-unity-c-342ab4806d53

namespace TeleopReachy
{
    public enum EventNames
    {
        EnterConnectionScene,
        EnterMirrorScene,
        EnterTeleoperationScene,

        QuitConnectionScene,
        QuitMirrorScene,
        QuitTeleoperationScene,
        QuitApplication,

        RobotDataSceneLoaded,
        MirrorSceneLoaded,
        TeleoperationSceneLoaded,

        ShowXRay,
        HideXRay,
        HeadsetRemoved,
        HeadsetReset,
        LeftControllerTrackingLost,
        LeftControllerTrackingRetrieved,
        RightControllerTrackingLost,
        RightControllerTrackingRetrieved,

        OnStartTeleoperation,
        OnStartArmTeleoperation,
        OnStartMobileBaseTeleoperation,
        OnStopMobileBaseTeleoperation,
        OnStartEmotionTeleoperation,
        OnStopEmotionTeleoperation,
        OnStopTeleoperation,
        OnSuspendTeleoperation,
        OnResumeTeleoperation,

        OnInitializeRobotStateRequested,
        OnReinitializeLimitsRequested,
        OnRobotStiffRequested,
        OnRobotCompliantRequested,
        OnRobotSmoothlyCompliantRequested,
        OnGraspingLock,

        OnEmotionMode,
        OnMobilityMode,

        OnEmergencyStop,

        OnFixUserOrigin,
    }

    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<EventNames, UnityEvent> eventDictionary;

        protected override void Init()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<EventNames, UnityEvent>();
            }
        }

        public static void StartListening(EventNames eventName, UnityAction listener)
        {
            UnityEvent thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                Instance.eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(EventNames eventName, UnityAction listener)
        {
            UnityEvent thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(EventNames eventName)
        {
            UnityEvent thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }
}