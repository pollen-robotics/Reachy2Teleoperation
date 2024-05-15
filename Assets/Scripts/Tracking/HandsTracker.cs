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
                UnityEngine.Quaternion actualRotation = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * hand.GetVRHand().rotation;
                UnityEngine.Quaternion rescaledRotation = RescalefromCalibQuaternion(actualRotation, hand);
                
                Debug.Log("initialRotation :" + actualRotation.normalized.eulerAngles + " // rescaledRotation: " + rescaledRotation.eulerAngles);
                // UnityEngine.Quaternion rescaledRotation = RescaleRotation(actualRotation, hand, rescaleTransform);
                hand.handPose.SetTRS(new Vector3(0, 0, 0), rescaledRotation, new Vector3(1, 1, 1));

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
            List<List<float>> limitValues = new List<List<float>>();
            List<List<LinearParameters>> paramList = new List<List<LinearParameters>>();
            Vector3 actualEulerAngles = actualRotation.eulerAngles;
            
            if (hand == rightHand) 
            {
                if (rescaleTransform == 1) paramList = CaptureWristPose.Instance.rightLinearParameters;
                else if (rescaleTransform ==2 ) paramList = CaptureWristPose.Instance.fakeRightLinearParameters; //à retirer si on garde la calib
                limitValues = CaptureWristPose.Instance.rightLimitValues;

            }
            else {
                if (rescaleTransform == 1) paramList = CaptureWristPose.Instance.leftLinearParameters;
                else if (rescaleTransform ==2 ) paramList = CaptureWristPose.Instance.fakeLeftLinearParameters; //à retirer si on garde la calib
                limitValues = CaptureWristPose.Instance.leftLimitValues;

            }


            // new_x = actualEulerAngles.x;
            // new_y = actualEulerAngles.y;
            // new_z = actualEulerAngles.z;
            new_x = LinearRescale(actualEulerAngles.x, paramList[0], limitValues[0], 'x');
            new_y = LinearRescale(actualEulerAngles.y, paramList[1], limitValues[1],  'y');
            new_z = LinearRescale(actualEulerAngles.z, paramList[2], limitValues[2], 'z');

            // if (actualEulerAngles.x <= neutralPose.x)
            //     new_x = LinearRescale(actualEulerAngles.x, paramList[0]);
            // else new_x = LinearRescale(actualEulerAngles.x, paramList[3]);

            // if (actualEulerAngles.y <= neutralPose.y)
            //     new_y = LinearRescale(actualEulerAngles.y, paramList[1]);
            // else new_y = LinearRescale(actualEulerAngles.y, paramList[4]);

            // if (actualEulerAngles.z <= neutralPose.z)
            //     new_z = LinearRescale(actualEulerAngles.z, paramList[2]);
            // else new_z = LinearRescale(actualEulerAngles.z, paramList[5]);

            Vector3 rescaledEulerAngles = new Vector3 (new_x, new_y, new_z);
            Quaternion rescaledRotation = Quaternion.Euler(rescaledEulerAngles);
            
            return rescaledRotation;
        }

        private float LinearRescale (float originalValue, List<LinearParameters> param, List<float> limitValues, char mode)
        {
            float newValue = originalValue;
            if (mode == 'x' && originalValue < 0) originalValue = originalValue + 360;
            else if ((mode == 'y' || mode == 'z') && originalValue > limitValues[3]) originalValue = originalValue - 360;
            else if ((mode == 'y' || mode == 'z') && originalValue < limitValues[0]) originalValue = originalValue + 360;

            if (originalValue < limitValues[1])
            {
                newValue = param[1].A * originalValue + param[1].b;
            } else if (originalValue > limitValues[2]) {
                newValue = param[2].A * originalValue + param[2].b;
            } else {
                newValue = param[0].A * originalValue + param[0].b;
            }

            if ((mode == 'y' || mode == 'z') && newValue < 0) newValue = newValue + 360;
            if (newValue > 360 || newValue < 0 ) newValue = 0;

            Debug.Log(mode + " -> originalValue: " + originalValue + " newValue: " + newValue);
            return newValue;
        }

        public Quaternion RescalefromCalibQuaternion (Quaternion actualRotation, HandController hand)
        {
            Quaternion rescaledRotation = Quaternion.identity;
            actualRotation = actualRotation.normalized;

            if (hand == rightHand) 
            {
                rescaledRotation = CaptureWristPose.Instance.rightCalibrationQuaternion * actualRotation;
                
            }
            else {
                rescaledRotation = CaptureWristPose.Instance.leftCalibrationQuaternion * actualRotation;
            }
            ;
            return rescaledRotation.normalized;
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