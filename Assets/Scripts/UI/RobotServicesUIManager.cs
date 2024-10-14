using UnityEngine;


namespace TeleopReachy
{
    public class RobotServicesUIManager : MonoBehaviour
    {
        private ConnectionStatus connectionStatus;

        void Awake()
        {
            if (Robot.IsCurrentRobotVirtual())
            {
                HideServices();
            }
            else
            {
                connectionStatus = ConnectionStatus.Instance;
                connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckServices);

                CheckServices();
            }
        }

        private void CheckServices()
        {
            transform.GetChild(0).GetChild(0).gameObject.SetActive(!connectionStatus.IsRobotInVideoRoom());
            transform.GetChild(0).GetChild(1).gameObject.SetActive(connectionStatus.IsRobotInVideoRoom());

            transform.GetChild(1).GetChild(0).gameObject.SetActive(!connectionStatus.IsRobotInAudioReceiverRoom());
            transform.GetChild(1).GetChild(1).gameObject.SetActive(connectionStatus.IsRobotInAudioReceiverRoom());

            transform.GetChild(2).GetChild(0).gameObject.SetActive(!connectionStatus.IsRobotInAudioSenderRoom());
            transform.GetChild(2).GetChild(1).gameObject.SetActive(connectionStatus.IsRobotInAudioSenderRoom());

            transform.GetChild(3).GetChild(0).gameObject.SetActive(!connectionStatus.IsRobotInDataRoom());
            transform.GetChild(3).GetChild(1).gameObject.SetActive(connectionStatus.IsRobotInDataRoom());
        }

        private void HideServices()
        {
            transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(0).GetChild(1).gameObject.SetActive(false);

            transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).GetChild(1).gameObject.SetActive(false);

            transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(2).GetChild(1).gameObject.SetActive(false);

            transform.GetChild(3).GetChild(0).gameObject.SetActive(false);
            transform.GetChild(3).GetChild(1).gameObject.SetActive(false);
        }
    }
}