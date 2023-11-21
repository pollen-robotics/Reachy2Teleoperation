using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Reachy.Kinematics;


namespace TeleopReachy
{
    public enum ArmSide 
    {
        LEFT, RIGHT
    }
    [System.Serializable]
    public class HandController
    {
        private Transform VRHand;

        [SerializeField]
        [HideInInspector]
        public UnityEngine.Matrix4x4 handPose = UnityEngine.Matrix4x4.identity;
        [SerializeField]
        [HideInInspector]
        public Reachy.Kinematics.Point target_position;
        [HideInInspector]
        public Reachy.Kinematics.Matrix3x3 target_rotation;

        [SerializeField]
        [HideInInspector]
        public string handSide;

        [SerializeField]
        [HideInInspector]
        public float trigger = 0;

        public ArmSide side_id;

        public UnityEngine.XR.InputDevice device;

        public HandController(string side, UnityEngine.XR.InputDevice handDevice)
        {
            InitDevice(side);
            device = handDevice;
        }

        public HandController(string side)
        {
            InitDevice(side);
        }

        void InitDevice(string side)
        {
            handSide = side;

            if (handSide == "right")
            {
                side_id = ArmSide.RIGHT;
                VRHand = GameObject.Find("TrackedRightHand").transform;
            }
            else
            {
                side_id = ArmSide.LEFT;
                VRHand = GameObject.Find("TrackedLeftHand").transform;
            }
        }

        public Transform GetVRHand()
        {
            return VRHand;
        }
    }

    public class HandsTracker : MonoBehaviour
    {
        public HandController rightHand;
        public HandController leftHand;

        public ControllersManager controllers;

        void Awake()
        {
            rightHand = new HandController("right", controllers.rightHandDevice);
            leftHand = new HandController("left", controllers.leftHandDevice);

            controllers.event_OnDevicesUpdate.AddListener(UpdateDevices);
        }

        void Start()
        {
            GetTransforms(rightHand);
            GetTransforms(leftHand);
        }

        private void UpdateDevices()
        {
            rightHand = new HandController("right", controllers.rightHandDevice);
            leftHand = new HandController("left", controllers.leftHandDevice);
        }

        void Update()
        {
            GetTransforms(rightHand);
            GetTransforms(leftHand);

            AdaptativeCloseHand(rightHand);
            AdaptativeCloseHand(leftHand);
        }

        private void GetTransforms(HandController hand)
        {
            // Position
            Vector3 positionHeadset = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * (hand.GetVRHand().position - transform.parent.position);
            Vector3 positionReachy = new Vector3(positionHeadset.z, -positionHeadset.x, positionHeadset.y);

            // Rotation
            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * hand.GetVRHand().rotation;
            hand.handPose.SetTRS(new Vector3(0, 0, 0), rotation, new Vector3(1, 1, 1));

            // matrice de passage
            UnityEngine.Matrix4x4 mP = new UnityEngine.Matrix4x4(new Vector4(0, -1, 0, 0),
                                            new Vector4(0, 0, 1, 0),
                                            new Vector4(1, 0, 0, 0),
                                            new Vector4(0, 0, 0, 1));
            hand.handPose = (mP * hand.handPose) * mP.inverse;

            hand.target_position = new Point { X = positionReachy[0], Y = positionReachy[1], Z = positionReachy[2]};
            hand.target_rotation = new Matrix3x3 {
                Data = { hand.handPose[0,0], hand.handPose[0,1], hand.handPose[0,2],
                        hand.handPose[1,0], hand.handPose[1,1], hand.handPose[1,2],
                        hand.handPose[2,0], hand.handPose[2,1], hand.handPose[2,2], }
            };
        }

        private void AdaptativeCloseHand(HandController hand)
        {
            // Get value of how much trigger is pushed
            float trigger;
            if (hand.device.isValid)
            {
                hand.device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out trigger);
                hand.trigger = trigger;
            }
        }
    }
}