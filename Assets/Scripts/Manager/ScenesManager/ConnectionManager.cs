using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace TeleopReachy
{
    public class ConnectionManager : MonoBehaviour
    {
        public GameObject CanvaRobotSelection;

        public GameObject prefabRobotButton;
        private Transform contentRobotList;

        public Button connectButton;

        public Button selectRobotButton;

        private List<Robot> robotsList;
        private const string robotListDataFile = "RobotsData.xml";

        private List<Button> CanvaRobotSelectionButtons;

        private bool has_robot_selected;
        //private bool has_robot_available;

        private bool isRobotSelectionMenuOpen;
        //private bool isServerInfoMenuOpen;
        private bool isAddRobotMenuOpen;
        private bool isDeleteRobotMenuOpen;
        private bool isModifyRobotMenuOpen;
        private bool isContentInitialized;

        private RobotButtonInfo robotToBeDeleted;
        private RobotButtonInfo robotToBeModified;

        private Robot selectedRobot;

        void Start()
        {
            isRobotSelectionMenuOpen = false;
            isAddRobotMenuOpen = false;
            isDeleteRobotMenuOpen = false;
            isModifyRobotMenuOpen = false;
            isContentInitialized = false;

            has_robot_selected = false;
            //has_robot_available = false;

            CanvaRobotSelectionButtons = new List<Button>();

            contentRobotList = CanvaRobotSelection.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0);

            robotsList = RobotConfigIO.LoadRobots(Application.persistentDataPath + "/" + robotListDataFile);
            if (!IsVirtualRobotInList())
                AddDefaultMirrorRobot();

            GenerateRobotScrollViewContent();

            UpdateSelectedRobot();
        }

        public void ConnectToRobot()
        {
            // If a robot is selected, load teleoperation scene with prefs set to selected info
            if (has_robot_selected)
            {
                PlayerPrefs.SetString("robot_ip", GetIpv4Address());
                PlayerPrefs.SetString("robot_info", selectedRobot.ip);

                EventManager.TriggerEvent(EventNames.QuitConnectionScene);
            }
        }

        private string GetIpv4Address()
        {
            if(selectedRobot.ip.EndsWith(".local"))
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(selectedRobot.ip);
                foreach (IPAddress ip in ipAddresses)
                {
                    // Check if the IP address is an IPv4 address
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return(ip.ToString());
                    }
                }
            }
            return selectedRobot.ip;
        }

        bool IsVirtualRobotInList()
        {
            foreach (Robot reachy in robotsList)
            {
                if (reachy.IsVirtualRobot())
                    return true;
            }
            return false;
        }

        void AddDefaultMirrorRobot()
        {
            Robot newRobot = new Robot();
            newRobot.ip = Robot.VIRTUAL_ROBOT_IP;
            newRobot.uid = Robot.VIRTUAL_ROBOT;
            robotsList.Add(newRobot);
        }

        void GenerateRobotScrollViewContent()
        {
            //CheckSavedRobotAvailable();
            // Create a robotButton for each saved robot
            foreach (Robot reachy in robotsList)
            {
                AddRobotButton(reachy);
            }
            isContentInitialized = true;
        }

        public void AddRobot()
        {
            Robot newRobot = new Robot();
            string action = "add";
            string ip = CanvaRobotSelection.transform.Find("AddRobot/LocationInputField").GetComponent<InputField>().text;
            
            // check the IP is valid
            if (!IPUtils.IsIPValid(ip))
            {
                RaiseRobotIpCannotBeNull(action);
                return;
            }

            //check that the IP is not already in the list
            foreach (Robot robot in robotsList)
            {
                if (robot.ip == ip)
                {
                    RaiseRobotIPAlreadyExists(action);
                    CanvaRobotSelection.transform.Find("AddRobot/LocationInputField").GetComponent<InputField>().text = "";
                    return;
                }
            }

            newRobot.ip = ip;

            //check the name is not already in the list and set to unknow if nothing has been filled
            string uid = CanvaRobotSelection.transform.Find("AddRobot/RobotNameInputField").GetComponent<InputField>().text.Trim();
            foreach (Robot robot in robotsList)
            {
                if (robot.uid == uid)
                {
                    RaiseRobotNameAlreadyExists(action);
                    return;
                }
            }
            newRobot.uid = uid != "" ? uid : "@Reachy";

            // Add robot to list, create new button and update menu
            robotsList.Add(newRobot);
            AddRobotButton(newRobot);

            UpdateSelectRobotMenu();

            RobotConfigIO.SaveRobots(Application.persistentDataPath + "/" + robotListDataFile, robotsList);

            OpenCloseAddRobot();
        }

        void AddRobotButton(Robot reachy)
        {
            GameObject newButton = (GameObject)Instantiate(prefabRobotButton, contentRobotList);
            newButton.GetComponent<RobotButtonManager>().SetRobot(reachy);
            newButton.GetComponent<RobotButtonManager>().event_OnDeletionRequested.AddListener(AskRobotDeletionConfirmation);
            newButton.GetComponent<RobotButtonManager>().event_OnModificationRequested.AddListener(AskModifyRobot);
            newButton.GetComponent<RobotButtonManager>().event_OnSelectedRobotButtonChanged.AddListener(ChangeSelectedRobot);
            CanvaRobotSelectionButtons.Add(newButton.GetComponent<Button>());

            // If it is the launch of the app, select default robot (last teleoperated robot)
            if (!isContentInitialized && PlayerPrefs.GetString("robot_info") != null)
            {
                if (reachy.ip == PlayerPrefs.GetString("robot_info"))
                {
                    newButton.GetComponent<RobotButtonManager>().SelectRobotButton();
                }
            }
        }

        // raises a warning when the IP is already in the list
        void RaiseRobotIPAlreadyExists(string action)
        {
            Debug.Log("IP already exists");
            ClearErrorMessage(action);
            string parent_folder = action == "add" ? "AddRobot" : "ModifyRobot";
            CanvaRobotSelection.transform.Find($"{parent_folder}/UidAlreadyExists").gameObject.SetActive(true);
        }

        void RaiseRobotNameAlreadyExists(string action)
        {
            Debug.Log("Name already exists");
            ClearErrorMessage(action);
            string parent_folder = action == "add" ? "AddRobot" : "ModifyRobot";
            CanvaRobotSelection.transform.Find($"{parent_folder}/NameAlreadyExists").gameObject.SetActive(true);
        }

        void RaiseRobotIpCannotBeNull(string action)
        {
            ClearErrorMessage(action);
            string parent_folder = action == "add" ? "AddRobot" : "ModifyRobot";
            CanvaRobotSelection.transform.Find($"{parent_folder}/UidCannotBeNull").gameObject.SetActive(true);
        }

        void ClearErrorMessage(string action)
        {
            string parent_folder = action == "add" ? "AddRobot" : "ModifyRobot";
            CanvaRobotSelection.transform.Find($"{parent_folder}/UidCannotBeNull").gameObject.SetActive(false);
            CanvaRobotSelection.transform.Find($"{parent_folder}/NameAlreadyExists").gameObject.SetActive(false);
            CanvaRobotSelection.transform.Find($"{parent_folder}/UidAlreadyExists").gameObject.SetActive(false);
        }

        void AskRobotDeletionConfirmation(RobotButtonInfo rbi)
        {
            robotToBeDeleted = rbi;
            OpenCloseDeleteRobot();
            CanvaRobotSelection.transform.Find("DeleteRobot/RobotUIDDeletion").GetComponent<Text>().text = rbi.robot.uid;
        }

        public void DeleteRobot()
        {
            robotsList.Remove(robotToBeDeleted.robot);
            CanvaRobotSelectionButtons.Remove(robotToBeDeleted.button.GetComponent<Button>());
            robotToBeDeleted.button.GetComponent<RobotButtonManager>().DeleteRobotButton();
            OpenCloseDeleteRobot();

            RobotConfigIO.SaveRobots(Application.persistentDataPath + "/" + robotListDataFile, robotsList);
            UpdateSelectRobotMenu();
            UpdateSelectedRobot();
        }

        void AskModifyRobot(RobotButtonInfo rbi)
        {
            robotToBeModified = rbi;
            OpenCloseModifyRobot();
            CanvaRobotSelection.transform.Find("AddRobot/RobotNameInputField").GetComponent<InputField>().text = robotToBeModified.robot.uid;
            CanvaRobotSelection.transform.Find("AddRobot/LocationInputField").GetComponent<InputField>().text = robotToBeModified.robot.ip;
        }

        public void ModifyRobot()
        {
            Robot newRobot = robotsList.Find(r => r.uid == robotToBeModified.robot.uid);
            string action = "modify";

            string ip = CanvaRobotSelection.transform.Find("ModifyRobot/LocationInputField").GetComponent<InputField>().text.Trim();
            
            //check the IP is valid
            if (!IPUtils.IsIPValid(ip))
            {
                RaiseRobotIpCannotBeNull(action);
                return;
            }
            
            //check the IP is not already in the list
            foreach (Robot robot in robotsList)
            {
                if (robot.ip == ip)
                {
                    RaiseRobotIPAlreadyExists(action);
                    CanvaRobotSelection.transform.Find("ModifyRobot/LocationInputField").GetComponent<InputField>().text = "";
                    
                    return;
                }
            }

            newRobot.ip = ip;

            //check the name is not already in the list and set to unknow if nothing has been filled
            string uid = CanvaRobotSelection.transform.Find("ModifyRobot/RobotNameInputField").GetComponent<InputField>().text.Trim();
            foreach (Robot robot in robotsList)
            {
                if (robot.uid == uid)
                {
                    RaiseRobotNameAlreadyExists(action);
                    return;
                }
            }
            newRobot.uid = uid != "" ? uid : "@Reachy";
            robotToBeModified.button.GetComponent<RobotButtonManager>().SetRobot(newRobot);

            RobotButtonManager selectedRobotButton = robotToBeModified.button.GetComponent<RobotButtonManager>().GetSelectedRobotButton();
            selectedRobot = (selectedRobotButton != null ? selectedRobotButton.GetRobot() : null);

            OpenCloseModifyRobot();

            RobotConfigIO.SaveRobots(Application.persistentDataPath + "/" + robotListDataFile, robotsList);
            UpdateSelectedRobot();
        }

        void ChangeSelectedRobot(Robot reachy)
        {
            selectedRobot = reachy;
            UpdateSelectedRobot();
        }

        void UpdateSelectedRobot()
        {
            CheckRobotIsSelected();

            selectRobotButton.transform.GetChild(0).gameObject.SetActive(has_robot_selected);
            selectRobotButton.transform.GetChild(1).gameObject.SetActive(!has_robot_selected);

            if (has_robot_selected)
            {
                selectRobotButton.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = selectedRobot.uid;
                selectRobotButton.transform.GetChild(0).GetChild(3).GetComponent<Text>().text = selectedRobot.ip;
            }

            connectButton.interactable = has_robot_selected;
        }

        public void OpenCloseSelectRobotMenu()
        {
            isRobotSelectionMenuOpen = !isRobotSelectionMenuOpen;
            CanvaRobotSelection.transform.GetChild(0).gameObject.SetActive(isRobotSelectionMenuOpen);
            UpdateSelectRobotMenu();
        }


        void UpdateSelectRobotMenu()
        {
            //CheckSavedRobotAvailable();
            /*if (isRobotSelectionMenuOpen)
            {
                CanvaRobotSelection.transform.GetChild(0).GetChild(1).gameObject.SetActive(!has_robot_available);
            }*/

            foreach (Button button in CanvaRobotSelectionButtons)
            {
                button.interactable = !isAddRobotMenuOpen;
            }
        }

        public void OpenCloseAddRobot()
        {
            isAddRobotMenuOpen = !isAddRobotMenuOpen;

            if (!isAddRobotMenuOpen)
            {
                CanvaRobotSelection.transform.Find("AddRobot/RobotNameInputField").GetComponent<InputField>().text = "";
                CanvaRobotSelection.transform.Find("AddRobot/LocationInputField").GetComponent<InputField>().text = "";
                ClearErrorMessage("add");
            }

            CanvaRobotSelection.transform.GetChild(1).gameObject.SetActive(isAddRobotMenuOpen);

            foreach (Button button in CanvaRobotSelectionButtons)
            {
                button.interactable = !isAddRobotMenuOpen;
            }
        }

        public void OpenCloseDeleteRobot()
        {
            isDeleteRobotMenuOpen = !isDeleteRobotMenuOpen;
            CanvaRobotSelection.transform.GetChild(2).gameObject.SetActive(isDeleteRobotMenuOpen);

            foreach (Button button in CanvaRobotSelectionButtons)
            {
                button.interactable = !isDeleteRobotMenuOpen;
            }
        }

        public void OpenCloseModifyRobot()
        {
            isModifyRobotMenuOpen = !isModifyRobotMenuOpen;

            if (!isModifyRobotMenuOpen)
            {
                CanvaRobotSelection.transform.Find("AddRobot/RobotNameInputField").GetComponent<InputField>().text = "";
                CanvaRobotSelection.transform.Find("AddRobot/LocationInputField").GetComponent<InputField>().text = "";
                ClearErrorMessage("modify");
            }

            CanvaRobotSelection.transform.GetChild(3).gameObject.SetActive(isModifyRobotMenuOpen);

            foreach (Button button in CanvaRobotSelectionButtons)
            {
                button.interactable = !isModifyRobotMenuOpen;
            }
        }


        /*void CheckSavedRobotAvailable()
        {
            has_robot_available = !(robotsList.Count == 0);
        }*/

        void CheckRobotIsSelected()
        {
            /*if (!has_robot_available) has_robot_selected = false;
            else
            {*/
            RobotButtonManager robotButtonManager = contentRobotList.GetChild(0).GetComponent<RobotButtonManager>().GetSelectedRobotButton();
            has_robot_selected = robotButtonManager != null ? (robotButtonManager.GetRobot() != null) : false;
            // }
        }

        public void QuitApplication()
        {
            EventManager.TriggerEvent(EventNames.QuitApplication);
        }

    }
}