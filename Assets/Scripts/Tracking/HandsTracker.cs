using UnityEngine;
using System;
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
        private Quaternion neutralPose = Quaternion.Euler(270,180,180);

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

            // Debug.Log("right rotation / pose neutre : " + (Quaternion.Inverse(neutralPose) * UnityEngine.Quaternion.Inverse(transform.parent.rotation)* rightHand.GetVRHand().rotation).eulerAngles);
            // Debug.Log("left rotation / pose neutre : " + (Quaternion.Inverse(neutralPose) * UnityEngine.Quaternion.Inverse(transform.parent.rotation)* leftHand.GetVRHand().rotation).eulerAngles);
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
            if (rescaleTransform == 1 ) 
            {
                UnityEngine.Quaternion actualRotation = UnityEngine.Quaternion.Inverse(transform.parent.rotation) * hand.GetVRHand().rotation;
                UnityEngine.Quaternion rescaledRotation = RescalefromNeutralPose(actualRotation, hand, rescaleTransform);
                hand.handPose.SetTRS(new Vector3(0, 0, 0), rescaledRotation, new Vector3(1, 1, 1));
                Debug.Log("initialRotation :" + actualRotation.normalized.eulerAngles + " // rescaledRotation: " + rescaledRotation.eulerAngles);

                

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

    

        public Quaternion RescalefromNeutralPose(Quaternion actualRotation, HandController hand, int rescaleTransform)
        {
            Quaternion orientationFromNeutralPose = UnityEngine.Quaternion.Inverse(neutralPose)*actualRotation;
            Vector3 eulerFromNeutralPose = orientationFromNeutralPose.eulerAngles;
            float actual_x = NormalizeAngle(eulerFromNeutralPose.x);
            float actual_y = NormalizeAngle(eulerFromNeutralPose.y);
            float actual_z = NormalizeAngle(eulerFromNeutralPose.z);
            List<LinearParameters> paramList = new List<LinearParameters>();

            if (hand == rightHand)
            {
                if (rescaleTransform ==1) paramList = CaptureWristPose.Instance.rightLinearParameters;
                else if (rescaleTransform ==2) paramList = CaptureWristPose.Instance.fakeRightLinearParameters; //à retirer si on garde la calib
            }
            else
            {
                if (rescaleTransform ==1) paramList = CaptureWristPose.Instance.leftLinearParameters;
                else if (rescaleTransform ==2) paramList = CaptureWristPose.Instance.fakeLeftLinearParameters; //à retirer si on garde la calib
            }
            float new_x = paramList[0].A * actual_x + paramList[0].b;
            float new_y = paramList[1].A * actual_y + paramList[1].b;
            float new_z = paramList[2].A * actual_z + paramList[2].b;

           
            Vector3 newEulerAngles = new Vector3(new_x, new_y, new_z);
            Debug.Log("old x = " + actual_x + " new y = " + new_x + " old y = " + actual_y + " new y = " + new_y + " old z = " + actual_z + " new z = " + new_z);
            //on remet la rotation par rapport au usertracker
            Quaternion newQuaternion = neutralPose* Quaternion.Euler(newEulerAngles);

            return newQuaternion;
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle < -180)
            {
                angle += 360;
            }
            else if (angle > 180)
            {
                angle -= 360;
            }
            return angle;
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