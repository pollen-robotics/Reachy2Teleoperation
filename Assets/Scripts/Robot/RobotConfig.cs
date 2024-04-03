using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Google.Protobuf;
using Reachy.Part;
using Reachy.Part.Arm;
using Reachy.Part.Head;
using Reachy.Part.Hand;
using Mobile.Base.Utility;
using Component;

namespace TeleopReachy
{
    public class RobotConfig : MonoBehaviour
    {
        private DataMessageManager dataController;
        // private WebRTCRestartService restartService;
        private ConnectionStatus connectionStatus;

        public Dictionary<string, PartId> partsId { get; private set; }
        public Dictionary<string, ComponentId> componentsId { get; private set; }

        private bool has_right_arm;
        private bool has_left_arm;
        private bool has_head;
        private bool has_left_gripper;
        private bool has_right_gripper;
        private bool has_mobile_base;

        private bool is_virtual;

        private bool has_robot_config;

        public UnityEvent event_OnConfigChanged;

        // Awake is called before Start functions
        void Start()
        {
            dataController = DataMessageManager.Instance;
            connectionStatus = WebRTCManager.Instance.ConnectionStatus;

            dataController.event_OnRobotReceived.AddListener(GetPartsId);
            connectionStatus.event_OnConnectionStatusHasChanged.AddListener(CheckConfig);

            has_robot_config = false;

            has_right_arm = false;
            has_left_arm = false;
            has_head = false;
            has_mobile_base = false;
            has_left_gripper = false;
            has_right_gripper = false;

            is_virtual = Robot.IsCurrentRobotVirtual();
        }

        void ResetConfig()
        {
            Debug.Log("[Robot config]: ResetConfig");
            has_right_arm = false;
            has_left_arm = false;
            has_head = false;
            has_mobile_base = false;
            has_left_gripper = false;
            has_right_gripper = false;

            has_robot_config = false;

            event_OnConfigChanged.Invoke();
        }

        void CheckConfig()
        {
            Debug.Log("[Robot config]: CheckConfig");
            if (connectionStatus.HasRobotJustLeftDataRoom())
            {
                ResetConfig();
            }
            Debug.Log("[Robot config]:has_robot_config: " + has_robot_config);
        }

        void GetPartsId(Reachy.Reachy reachy)
        {
            partsId = new Dictionary<string, PartId>();
            var descriptor = Reachy.Reachy.Descriptor;

            foreach (var field in descriptor.Fields.InDeclarationOrder())
            {
                var value = field.Accessor.GetValue(reachy) as IMessage;
                if (value != null && (value is Arm || value is Head || value is Hand))
                {
                    var idField = value.Descriptor.FindFieldByName("part_id");
                    if (idField != null)
                    {
                        PartId id = (PartId)idField.Accessor.GetValue(value);
                        partsId.Add(field.Name, id);
                    }
                }
                if (value != null && value is MobileBase)
                {
                    PartId id = new PartId { Name = "mobile_base" };
                    partsId.Add(field.Name, id);
                }
            }

            GetReachyConfig();
        }

        private void GetReachyConfig()
        {
            Debug.Log("[Robot config]: GetReachyConfig");
            has_right_arm = partsId.ContainsKey("r_arm");
            has_left_arm = partsId.ContainsKey("l_arm");
            has_head = partsId.ContainsKey("head");
            has_right_gripper = partsId.ContainsKey("r_hand");
            has_left_gripper = partsId.ContainsKey("l_hand");
            has_mobile_base = partsId.ContainsKey("mobile_base");

            has_robot_config = true;

            event_OnConfigChanged.Invoke();
        }

        public bool HasRightArm()
        {
            return has_right_arm;
        }
        public bool HasLeftArm()
        {
            return has_left_arm;
        }

        public bool HasHead()
        {
            return has_head;
        }

        public bool HasLeftGripper()
        {
            return has_left_gripper;
        }

        public bool HasRightGripper()
        {
            return has_right_gripper;
        }

        public bool HasMobileBase()
        {
            return has_mobile_base;
        }

        public Dictionary<string, PartId> GetAllPartsId()
        {
            return partsId;
        }

        public Dictionary<string, ComponentId> GetAllComponentsId()
        {
            return componentsId;
        }

        public bool GotReachyConfig()
        {
            return has_robot_config;
        }

        public bool IsVirtual()
        {
            return is_virtual;
        }

        public override string ToString()
        {
            return string.Format(@"has_right_arm = {0},
            has_left_arm= {1},
            has_head= {2},
            has_mobile_base= {3},
            has_left_gripper= {4},
            has_right_gripper= {5},
            has_robot_config= {6}",
            has_right_arm, has_left_arm, has_head, has_mobile_base, has_left_gripper,
            has_right_gripper, has_robot_config);
        }

    }
}
