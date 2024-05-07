using UnityEngine;
using System.Collections.Generic;

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
        public Reachy.Kinematics.Matrix4x4 target_pos;

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
        public int rescaleTransform = 0 ; //à changer en false si test concluant et qu'on garde la calib 

        public ControllersManager controllers;

        void Awake()
        {
            rightHand = new HandController("right", controllers.rightHandDevice);
            leftHand = new HandController("left", controllers.leftHandDevice);

            controllers.event_OnDevicesUpdate.AddListener(UpdateDevices);
            CaptureWristPose.Instance.event_WristPoseCaptured.AddListener(() => ChangeTransforms(1));
            CaptureWristPose.Instance.event_WristPoseCaptured.AddListener(InitSwitchCalib);
            


        }

        void Start()
        {
            GetTransforms(rightHand, rescaleTransform);
            GetTransforms(leftHand, rescaleTransform);
        }

        private void InitSwitchCalib()
        {
            SwitchCalibrationManager.Instance.event_OldCalibAsked.AddListener(() => ChangeTransforms(0));
            SwitchCalibrationManager.Instance.event_NewCalibAsked.AddListener(() => ChangeTransforms(1));
            SwitchCalibrationManager.Instance.event_FakeCalibAsked.AddListener(() => ChangeTransforms(2));
        }

        private void UpdateDevices()
        {
            rightHand = new HandController("right", controllers.rightHandDevice);
            leftHand = new HandController("left", controllers.leftHandDevice);
        }

        void Update()
        {
            GetTransforms(rightHand, rescaleTransform);
            GetTransforms(leftHand, rescaleTransform);

            AdaptativeCloseHand(rightHand);
            AdaptativeCloseHand(leftHand);
        }

        private void ChangeTransforms(int numCalib)
        {
            this.rescaleTransform = numCalib;// à repasser en true si concluant et qu'on garde la calib
            Debug.Log("[HandsTracker] rescaleTransform : " + rescaleTransform); 
        }
        

        private void GetTransforms(HandController hand, int rescaleTransform)
        {
            // Position
            Vector3 positionHeadset = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * (hand.GetVRHand().position - transform.parent.position);
            Vector3 positionReachy = new Vector3(positionHeadset.z, -positionHeadset.x, positionHeadset.y);
            Vector4 positionVect = new Vector4(positionReachy.x, positionReachy.y, positionReachy.z, 1);

            // Rotation
            //ajout calib
            if (rescaleTransform == 1 || rescaleTransform == 2) 
            {
                UnityEngine.Quaternion actualRotation=hand.GetVRHand().rotation;
                UnityEngine.Quaternion rescaledRotation = RescaleRotation(actualRotation, hand, rescaleTransform);
                UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * rescaledRotation;
                hand.handPose.SetTRS(new Vector3(0, 0, 0), rotation, new Vector3(1, 1, 1));

            } else if (rescaleTransform == 0){
                UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * hand.GetVRHand().rotation;
                hand.handPose.SetTRS(new Vector3(0, 0, 0), rotation, new Vector3(1, 1, 1));
            } 

            // matrice de passage
            UnityEngine.Matrix4x4 mP = new UnityEngine.Matrix4x4(new Vector4(0, -1, 0, 0),
                                            new Vector4(0, 0, 1, 0),
                                            new Vector4(1, 0, 0, 0),
                                            new Vector4(0, 0, 0, 1));
            hand.handPose = (mP * hand.handPose) * mP.inverse;

            hand.handPose.SetColumn(3, positionVect);

            hand.target_pos = new Reachy.Kinematics.Matrix4x4
            {
                Data = { hand.handPose[0,0], hand.handPose[0,1], hand.handPose[0,2], hand.handPose[0,3],
                            hand.handPose[1,0], hand.handPose[1,1], hand.handPose[1,2], hand.handPose[1,3],
                            hand.handPose[2,0], hand.handPose[2,1], hand.handPose[2,2], hand.handPose[2,3],
                            hand.handPose[3,0], hand.handPose[3,1], hand.handPose[3,2], hand.handPose[3,3] }
            };
        }

        //ajout calib
        private Quaternion RescaleRotation(Quaternion actualRotation, HandController hand, int rescaleTransform) //Vector3 neutralpose, Vector3 minAngles, Vector3 maxAngles)
        {

            float new_x = 0, new_y = 0, new_z = 0;
            Vector3 neutralPose = new Vector3();
            List<LinearParameters> paramList = new List<LinearParameters>();
            Vector3 actualEulerAngles = actualRotation.eulerAngles;
            
            if (hand == rightHand) 
            {
                if (rescaleTransform == 1) paramList = CaptureWristPose.Instance.rightLinearParameters;
                else if (rescaleTransform ==2 ) paramList = CaptureWristPose.Instance.fakeRightLinearParameters; //à retirer si on garde la calib
                neutralPose = CaptureWristPose.Instance.rightNeutralOrientation.eulerAngles;
            }
            else {
                if (rescaleTransform == 1) paramList = CaptureWristPose.Instance.leftLinearParameters;
                else if (rescaleTransform ==2 ) paramList = CaptureWristPose.Instance.fakeLeftLinearParameters; //à retirer si on garde la calib
                neutralPose = CaptureWristPose.Instance.leftNeutralOrientation.eulerAngles;
            }
            //new_x = LinearRescale_x(actualEulerAngles.x, paramList[0]);
            new_y = LinearRescale(actualEulerAngles.y, paramList[1]);
            new_z = LinearRescale(actualEulerAngles.z, paramList[2]);

            // if (actualEulerAngles.x <= neutralPose.x)
            //     new_x = LinearRescale(actualEulerAngles.x, paramList[0]);
            // else new_x = LinearRescale(actualEulerAngles.x, paramList[3]);

            // if (actualEulerAngles.y <= neutralPose.y)
            //     new_y = LinearRescale(actualEulerAngles.y, paramList[1]);
            // else new_y = LinearRescale(actualEulerAngles.y, paramList[4]);

            // if (actualEulerAngles.z <= neutralPose.z)
            //     new_z = LinearRescale(actualEulerAngles.z, paramList[2]);
            // else new_z = LinearRescale(actualEulerAngles.z, paramList[5]);

            Vector3 rescaledEulerAngles = new Vector3 (actualEulerAngles.x, new_y, new_z);
            Quaternion rescaledRotation = Quaternion.Euler(rescaledEulerAngles);
            
            return rescaledRotation;
        }

        private float LinearRescale (float originalValue, LinearParameters param)
        {
            if (originalValue < 0) originalValue = originalValue + 360;

            float x360 = (360-param.b) / param.A;
            if (originalValue > 360 + x360) originalValue = originalValue - 360;
            else if (originalValue < x360 -360 && originalValue > 0) originalValue = originalValue + 360;

            float newValue = param.A * originalValue + param.b;
            if (newValue > 360 || newValue < 0 ) newValue = 0;

            Debug.Log("originalValue: " + originalValue + " newValue: " + newValue);
            return newValue;
        }

        private float LinearRescale_x (float originalValue, LinearParameters param)
        {
            
            LinearParameters paramUnder = CaptureWristPose.Instance.LinearCoefficient(0f,200f,0f,param.A*200+param.b);
            LinearParameters paramAbove= CaptureWristPose.Instance.LinearCoefficient(330f,360f,param.A*330+param.b,360f);

            if (originalValue < 0) originalValue = originalValue + 360;
            if (originalValue < 200) return LinearRescale(originalValue, paramUnder);
            else if (originalValue > 330) return LinearRescale(originalValue, paramAbove);
            else return LinearRescale(originalValue, param);

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