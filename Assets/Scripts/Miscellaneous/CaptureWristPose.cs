using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TeleopReachy
{
    [System.Serializable]
    public struct LinearParameters
    {
        public float A;
        public float b;

        public LinearParameters(float A, float b)
        {
            this.A = A;
            this.b = b;
        }
    }
    public class CaptureWristPose : Singleton<CaptureWristPose>
    {
        public float recordInterval = 1f;
        private float timer = 0f;
        private Transform leftController;
        private Transform rightController;
        private Transform userTrackerTransform;
        public Vector3 rightMinAngles = new Vector3(-100f, -100f, -100f);
        public Vector3 rightMaxAngles = new Vector3(100f, 100f, 100f);
        public Vector3 leftMinAngles = new Vector3(-100f, -100f, -100f);
        public Vector3 leftMaxAngles = new Vector3(100f, 100f, 100f);
        public List<Quaternion> rightTargetQuaternions = new List<Quaternion>
        {
            // avec les bras bien axés selon le torse
            // new Quaternion(0.7344111f, 0.005129192f, 0.006385155f, -0.6786556f),
            // new Quaternion(0.4123681f, -0.5375473f, 0.494651f, -0.5443492f),
            // new Quaternion(0.509445f, 0.5849369f, -0.4539117f, -0.4384963f),
            // new Quaternion(0.5188917f, -0.006976227f, -0.004126042f, -0.8548017f),
            // new Quaternion(0.8167634f, 0.0747829f, -0.03443614f, -0.5710686f),
            // new Quaternion(0.6491414f, 0.2352062f, 0.2241392f, -0.6877902f),
            // new Quaternion(0.6518105f, -0.3496563f, -0.4103235f, -0.5334024f)

            // avec les bras en manipulation (vers le centre)
            // new Quaternion(-0.733967f, 0.001562502f, -0.03871955f, 0.6780788f),
            // new Quaternion(-0.1353134f, 0.269623f, -0.6390435f, 0.7075431f),
            // new Quaternion(-0.52517f, -0.524559f, 0.5125754f, 0.4316261f),
            // new Quaternion(-0.4319915f, 0.01383755f, -0.06989686f, 0.8990587f),
            // new Quaternion(-0.8277104f, -0.02025749f, -0.04587379f, 0.5589104f),
            // new Quaternion(-0.5284399f, -0.356739f, -0.4259247f, 0.6419322f),
            // new Quaternion(-0.6661459f, 0.2963598f, 0.3415214f, 0.5931137f)

            // sans la translation initiale avec bras en manipulation
            // new Quaternion(0.7200386f, 0.001945139f, 0.007194162f, -0.6938941f),
            // new Quaternion(0.1892721f, -0.3952768f, 0.6013614f, -0.6680547f),
            // new Quaternion(0.5539141f, 0.3362891f, -0.622225f, -0.4392323f),
            // new Quaternion(0.3826739f, -0.0293789f, 0.01883947f, -0.9232241f),
            // new Quaternion(0.7974557f, 0.01352278f, 0.04223183f, -0.601746f),
            // new Quaternion(0.5404658f, 0.2670475f, 0.3463716f, -0.7187553f),
            // new Quaternion(0.6178856f, -0.3566078f, -0.3912661f, -0.5813426f)

            // new Quaternion(0.7035419f, 0.00237859f, -0.01086387f, -0.7105669f),
            // new Quaternion(0.4569412f, -0.5365228f, 0.5234174f, -0.4789388f),
            // new Quaternion(0.4668782f, 0.4594523f, -0.5760691f, -0.4889508f),
            // new Quaternion(0.4070962f, -0.06596193f, 0.008979646f, -0.9109563f),
            // new Quaternion(0.8455631f, -0.0146188f, -0.05328752f, -0.5310086f),
            // new Quaternion(0.607029f, 0.2573243f, 0.2729943f, -0.7005529f),
            // new Quaternion(0.6617436f, -0.3689488f, -0.3730824f, -0.5355201f)

            //new Quaternion(0.7124369f, -0.008194365f, -0.01602869f, -0.7015054f),
            // new Quaternion(0.3607748f, 0.5844909f, -0.5274996f, -0.4999564f),
            // new Quaternion(0.4488821f, -0.5250984f, 0.541166f, -0.4794956f),
            // new Quaternion(0.5578061f, 0.03717021f, 0.001883716f, -0.8291365f),
            // new Quaternion(0.7947264f, 0.027002f, 0.03841751f, -0.605149f),
            // new Quaternion(0.5677758f, -0.3151843f, -0.2760258f, -0.7085899f),
            // new Quaternion(0.6178392f, 0.2879293f, 0.4050178f, -0.6093704f)

            //avec la nouvelle transfo
             new Quaternion(-0.7255567f, -0.01298403f, -0.05923351f, 0.6854853f),
            new Quaternion(-0.3958733f, 0.5345705f, -0.5357223f, 0.5201157f),
            new Quaternion(-0.5435364f, -0.5220206f, 0.5024199f, 0.423836f),
            new Quaternion(-0.8828944f, -0.06969474f, -0.05926104f, 0.4605739f),
            new Quaternion(-0.4835635f, -0.06222418f, -0.04293682f, 0.8720384f),
            new Quaternion(-0.4804487f, -0.05482046f, -0.01646214f, 0.875153f),
            new Quaternion(-0.6646084f, -0.2905225f, -0.3577821f, 0.5881193f)

                
        };
        public List<Quaternion> leftTargetQuaternions = new List<Quaternion>
        {
            // avec les bras bien axés selon le torse
            // new Quaternion(-0.7203441f, 0.03399835f, -0.005024872f, 0.6927649f),
            // new Quaternion(-0.4838696f, -0.5394279f, 0.4340175f, 0.5352727f),
            // new Quaternion(-0.4509256f, 0.5427567f, -0.473059f, 0.5275382f),
            // new Quaternion(-0.4702284f, -0.03697109f, -0.02264086f, 0.8814794f),
            // new Quaternion(-0.7880319f, 0.07103119f, -0.0172527f, 0.6112795f),
            // new Quaternion(-0.6622814f, 0.2737709f, 0.2503947f, 0.6509497f),
            // new Quaternion(-0.775171f, -0.06023739f, -0.1252128f, 0.6162818f)

            // avec les bras en manipulation (vers le centre)
            // new Quaternion(0.7092143f, -0.02600733f, 0.009396867f, -0.7044505f),
            // new Quaternion(0.1157549f, 0.2098556f, -0.6401092f, -0.7299464f),
            // new Quaternion(0.4695066f, -0.493953f, 0.5385018f, -0.4955704f),
            // new Quaternion(0.3384593f, 0.01320817f, 0.05411278f, -0.9393311f),
            // new Quaternion(0.8484932f, -0.06607866f, 0.03255066f, -0.524055f),
            // new Quaternion(0.5566997f, -0.3870198f, -0.3827864f, -0.6275157f),
            // new Quaternion(0.5646459f, 0.3065913f, 0.4434907f, -0.6248944f)

            // sans la translation initiale avec bras en manipulation
            // new Quaternion(0.7107952f, -0.0223146f, -0.01824737f, -0.7028083f),
            // new Quaternion(0.2514088f, 0.3065544f, -0.6233323f, -0.6739992f),
            // new Quaternion(0.472937f, -0.5334244f, 0.5401126f, -0.4472893f),
            // new Quaternion(0.4025533f, 0.02738633f, -0.005768039f, -0.9149686f),
            // new Quaternion(0.787545f, -0.04789896f, -0.00476822f, -0.6143745f),
            // new Quaternion(0.5785097f, -0.3282643f, -0.3510408f, -0.6590445f),
            // new Quaternion(0.6363655f, 0.1495565f, 0.3091016f, -0.6907446f)

            // new Quaternion(-0.7134706f, 0.03353291f, 0.0868822f, 0.6944688f),
            // new Quaternion(-0.4406907f, -0.4731545f, 0.4475748f, 0.6177326f),
            // new Quaternion(-0.5528527f, 0.5070614f, -0.4861065f, 0.4482669f),
            // new Quaternion(-0.4674285f, 0.1121391f, 0.03723811f, 0.8760987f),
            // new Quaternion(-0.8303028f, 0.05414607f, 0.02070714f, 0.5542896f),
            // new Quaternion(-0.5477383f, 0.3547438f, 0.3736976f, 0.6591583f),
            // new Quaternion(-0.7251863f, -0.107534f, -0.1053768f, 0.6718907f)

            //new Quaternion(-0.7194409f, 0.001554064f, -0.03588923f, 0.6936241f),
            // new Quaternion(-0.276718f, 0.5592638f, -0.6697316f, 0.4026301f),
            // new Quaternion(-0.5469989f, -0.5289803f, 0.5015224f, 0.4116402f),
            // new Quaternion(-0.545078f, -0.04925552f, -0.04966375f, 0.8354625f),
            // new Quaternion(-0.847944f, -0.04641689f, -0.07273667f, 0.5230163f),
            // new Quaternion(-0.517005f, -0.3960971f, -0.3785332f, 0.6576669f),
            // new Quaternion(-0.632797f, 0.3413086f, 0.414336f, 0.5580344f)

            //avec la nouvelle transfo
            new Quaternion(0.7378079f, -0.0204621f, -0.03755364f, -0.6736546f),
            new Quaternion(0.4374879f, 0.4771764f, -0.5734872f, -0.5020154f),
            new Quaternion(0.5206729f, -0.4900601f, 0.4992528f, -0.4893745f),
            new Quaternion(0.8468782f, -0.01118093f, 0.06245401f, -0.5279886f),
            new Quaternion(0.4728747f, -0.01893464f, 0.01083151f, -0.8808596f),
            new Quaternion(0.4780315f, 0.008404803f, 0.03032118f, -0.877779f),
            new Quaternion(0.6593991f, -0.2977514f, -0.2526426f, -0.642424f),
        };

        public List<Quaternion> rightCalibrationQuaternions = new List<Quaternion>();
    
        public List<Quaternion> leftCalibrationQuaternions = new List<Quaternion>();
        
        public Quaternion rightCalibrationQuaternion = new Quaternion();
        public Quaternion leftCalibrationQuaternion = new Quaternion();
        public Quaternion rightNeutralOrientation;
        public Quaternion leftNeutralOrientation;
        public List<List<LinearParameters>> rightLinearParameters = new List<List<LinearParameters>>();
        public List<List<LinearParameters>> leftLinearParameters = new List<List<LinearParameters>>();
        public List<List<LinearParameters>> fakeRightLinearParameters = new List<List<LinearParameters>>();
        public List<List<LinearParameters>> fakeLeftLinearParameters = new List<List<LinearParameters>>();

        public List<List<float>> rightLimitValues = new List<List<float>>();
        public List<List<float>> leftLimitValues = new List<List<float>>();
        private List<string> capturedData = new List<string>();
        private bool buttonX;
        private int nbPosition = 0;
        public UnityEvent event_onStartWristCalib;
        private ControllersManager controllers;
        public UnityEvent event_NeutralPoseCaptured = new UnityEvent();
        public UnityEvent event_WristPoseCaptured = new UnityEvent();


        public void Start()
        {
            Debug.Log("[Wrist Calibration version User] Start");
            leftController = GameObject.Find("TrackedLeftHand").transform;
            rightController = GameObject.Find("TrackedRightHand").transform;
            event_onStartWristCalib = new UnityEvent();
            event_onStartWristCalib.AddListener(StartWristCalibration);
            controllers = ActiveControllerManager.Instance.ControllersManager;

            // leftTargetQuaternions.Add(new Quaternion(0.7548487f, -0.03436273f, -0.05109999f, -0.653002f));
            // leftTargetQuaternions.Add(new Quaternion(0.436973f, 0.4826646f, -0.5211778f, -0.5517819f));
            // leftTargetQuaternions.Add(new Quaternion(0.4986962f, -0.5191435f, 0.4933016f, -0.4883094f));
            // leftTargetQuaternions.Add(new Quaternion(0.4339799f, 0.04320588f, 0.04761387f, -0.8986255f));
            // leftTargetQuaternions.Add(new Quaternion(0.8197948f, -0.0386104f, -0.07024907f, -0.5670194f));
            // leftTargetQuaternions.Add(new Quaternion(0.5805598f, -0.2952426f, -0.4605616f, -0.6030466f));
            // leftTargetQuaternions.Add(new Quaternion(0.6486574f, 0.2513193f, 0.280946f, -0.6611745f));

            // rightTargetQuaternions.Add(new Quaternion(0.7357965f, 0.02146629f, 0.007585278f, -0.6768201f));
            // rightTargetQuaternions.Add(new Quaternion(0.4939354f, -0.479659f, 0.5272943f, -0.4979117f));
            // rightTargetQuaternions.Add(new Quaternion(0.4756156f, 0.5365561f, -0.4671748f, -0.5173444f));
            // rightTargetQuaternions.Add(new Quaternion(0.4470062f, 0.04602879f, -0.04383902f, -0.8922697f));
            // rightTargetQuaternions.Add(new Quaternion(0.8046046f, 0.03835141f, 0.0276129f, -0.5919276f));
            // rightTargetQuaternions.Add(new Quaternion(0.4137476f, 0.4840278f, 0.4346469f, -0.636877f));
            // rightTargetQuaternions.Add(new Quaternion(0.7194823f, -0.2188603f, -0.1713959f, -0.6364505f));
            
        }

        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= recordInterval && nbPosition < 8)
            {
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX)
                {
                    event_onStartWristCalib.Invoke();
                }
            }

            if (nbPosition == 7)
            {
                
                // GetRescalingParameters();
                // GetFakeRescalingParameters(); // à enlever si on garde la calibration
                rightCalibrationQuaternion = AverageQuaternions(rightCalibrationQuaternions);
                leftCalibrationQuaternion = AverageQuaternions(leftCalibrationQuaternions);
                capturedData.Add($"CalibQuat, {leftCalibrationQuaternion.x},{leftCalibrationQuaternion.y},{leftCalibrationQuaternion.z},{leftCalibrationQuaternion.w},{rightCalibrationQuaternion.x},{rightCalibrationQuaternion.y},{rightCalibrationQuaternion.z},{rightCalibrationQuaternion.w}");
                SavePoseData(); 
                event_WristPoseCaptured.Invoke();
                nbPosition ++;
                capturedData.Clear();
            }
        }

        public void StartWristCalibration()
        {

            userTrackerTransform = GameObject.Find("UserTracker").transform;
            Debug.Log("[Wrist Calibration] Start Calibration");
            Matrix4x4 userTrackerInverseTransform = userTrackerTransform.worldToLocalMatrix;

            Vector3 leftHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(leftController.position);
            Quaternion leftHandRotation = UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * leftController.rotation;
            Vector3 leftHandEulerAngles = leftHandRotation.eulerAngles;

            Vector3 rightHandPosition = userTrackerInverseTransform.MultiplyPoint3x4(rightController.position);
            Quaternion rightHandRotation = UnityEngine.Quaternion.Inverse(userTrackerTransform.rotation) * rightController.rotation;
            Vector3 rightHandEulerAngles = rightHandRotation.eulerAngles;


            nbPosition++;
            buttonX = false;
            timer = 0f;
            //if (nbPosition<8 && nbPosition>1) GetCalibrationQuaternion(leftTargetQuaternions[nbPosition - 2], rightTargetQuaternions[nbPosition - 2], leftHandRotation, rightHandRotation);

            GetCalibrationQuaternion(leftTargetQuaternions[nbPosition - 1], rightTargetQuaternions[nbPosition - 1], leftHandRotation, rightHandRotation);
            Debug.Log("left quat =" + leftCalibrationQuaternions[nbPosition - 1].eulerAngles + "right quat =" + rightCalibrationQuaternions[nbPosition - 1].eulerAngles );
            switch (nbPosition)
            {
                case 1:
                    rightNeutralOrientation = rightHandRotation;
                    leftNeutralOrientation = leftHandRotation;
                    event_NeutralPoseCaptured.Invoke();
                    Debug.Log("Position 1");
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    break;
                case 2:
                    Quaternion localLeftHandRotation = UnityEngine.Quaternion.Inverse(leftNeutralOrientation) * leftHandRotation ;
                    Debug.Log("localLeftHandRotation ="+ localLeftHandRotation.eulerAngles);
                    Quaternion localRightHandRotation = UnityEngine.Quaternion.Inverse(rightNeutralOrientation) *  rightHandRotation;
                    Debug.Log("localRightHandRotation ="+ localRightHandRotation.eulerAngles);
                    rightMaxAngles.z = rightHandEulerAngles.z;
                    leftMinAngles.z = leftHandEulerAngles.z;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 2");
                    break;
                case 3:
                    localLeftHandRotation = UnityEngine.Quaternion.Inverse(leftNeutralOrientation) * leftHandRotation ;
                    Debug.Log("localLeftHandRotation ="+ localLeftHandRotation.eulerAngles);
                    localRightHandRotation = UnityEngine.Quaternion.Inverse(rightNeutralOrientation) *  rightHandRotation;
                    Debug.Log("localRightHandRotation ="+ localRightHandRotation.eulerAngles);
                    rightMinAngles.z = rightHandEulerAngles.z;
                    leftMaxAngles.z = leftHandEulerAngles.z;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 3");
                    break;
                case 4:
                    localLeftHandRotation = UnityEngine.Quaternion.Inverse(leftNeutralOrientation) * leftHandRotation ;
                    Debug.Log("localLeftHandRotation ="+ localLeftHandRotation.eulerAngles);
                    localRightHandRotation = UnityEngine.Quaternion.Inverse(rightNeutralOrientation) *  rightHandRotation;
                    Debug.Log("localRightHandRotation ="+ localRightHandRotation.eulerAngles);
                    rightMinAngles.x = rightHandEulerAngles.x;
                    leftMinAngles.x = leftHandEulerAngles.x;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 4");
                    break;
                case 5:
                    localLeftHandRotation = UnityEngine.Quaternion.Inverse(leftNeutralOrientation) * leftHandRotation ;
                    Debug.Log("localLeftHandRotation ="+ localLeftHandRotation.eulerAngles);
                    localRightHandRotation = UnityEngine.Quaternion.Inverse(rightNeutralOrientation) *  rightHandRotation;
                    Debug.Log("localRightHandRotation ="+ localRightHandRotation.eulerAngles);
                    rightMaxAngles.x = rightHandEulerAngles.x;
                    leftMaxAngles.x = leftHandEulerAngles.x;
                    Debug.Log("Position 5");
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    break;
                case 6:
                    localLeftHandRotation = UnityEngine.Quaternion.Inverse(leftNeutralOrientation) * leftHandRotation ;
                    Debug.Log("localLeftHandRotation ="+ localLeftHandRotation.eulerAngles);
                    localRightHandRotation = UnityEngine.Quaternion.Inverse(rightNeutralOrientation) *  rightHandRotation;
                    Debug.Log("localRightHandRotation ="+ localRightHandRotation.eulerAngles);
                    rightMaxAngles.y = rightHandEulerAngles.y;
                    leftMinAngles.y = leftHandEulerAngles.y;
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("Position 6");
                    break;
                case 7:
                    localLeftHandRotation = UnityEngine.Quaternion.Inverse(leftNeutralOrientation) * leftHandRotation ;
                    Debug.Log("localLeftHandRotation ="+ localLeftHandRotation.eulerAngles);
                    localRightHandRotation = UnityEngine.Quaternion.Inverse(rightNeutralOrientation) *  rightHandRotation;
                    Debug.Log("localRightHandRotation ="+ localRightHandRotation.eulerAngles);
                    rightMinAngles.y = rightHandEulerAngles.y;
                    leftMaxAngles.y = leftHandEulerAngles.y;
                    Debug.Log("Position 7");
                    capturedData.Add($"{leftHandPosition.x},{leftHandPosition.y},{leftHandPosition.z},{leftHandRotation.x},{leftHandRotation.y},{leftHandRotation.z},{leftHandRotation.w},{leftHandEulerAngles.x},{leftHandEulerAngles.y},{leftHandEulerAngles.z},{rightHandPosition.x},{rightHandPosition.y},{rightHandPosition.z},{rightHandRotation.x},{rightHandRotation.y},{rightHandRotation.z},{rightHandRotation.w},{rightHandEulerAngles.x},{rightHandEulerAngles.y},{rightHandEulerAngles.z}");
                    Debug.Log("rightminangles ="+ rightMinAngles+ "rightmaxangles ="+ rightMaxAngles+ "leftminangles ="+ leftMinAngles+ "leftmaxangles ="+ leftMaxAngles);
                    break;
            }
        }


        public void SavePoseData()
        {
            Debug.Log("[Wrist Calibration] Saving Data");
            string path = "C:/Users/robot/Dev/WristCalibrationData_userinitialtransformation.csv";
            string dataToAppend = string.Join("\n", capturedData) + "\n";

            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.Write(dataToAppend);
            }
        }

        public void GetRescalingParameters()
        {
            rightLinearParameters.Add(Get3LinearParameters(rightMinAngles.x, rightMaxAngles.x, 290f, 305f, 'x'));
            rightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.y - 360, rightMinAngles.y, -60f, 60f, 'y'));
            rightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.z - 360, rightMinAngles.z, -90f, 90f, 'z'));

            rightLimitValues.Add(GetLimitValues(rightMinAngles.x, rightMaxAngles.x));
            rightLimitValues.Add(GetLimitValues(rightMaxAngles.y - 360, rightMinAngles.y));
            rightLimitValues.Add(GetLimitValues(rightMaxAngles.z - 360, rightMinAngles.z));

            Debug.Log("rightlimitvalues ="+ rightLimitValues[0] + rightLimitValues[1] + rightLimitValues[2]);


            //rightLinearParameters.Add(LinearCoefficient(rightMinAngles.x, rightMaxAngles.x, 290f, 310f));
            //rightLinearParameters.Add(LinearCoefficient(rightMaxAngles.y - 360, rightMinAngles.y, -60f, 60f));
            //rightLinearParameters.Add(LinearCoefficient(rightMaxAngles.z - 360, rightMinAngles.z, -90f, 90f));


            // rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.x, rightMaxAngles.x, 0f, 20f));
            // rightLinearParameters.Add(LinearCoefficient(rightNeutralOrientation.eulerAngles.y, rightMaxAngles.y, 0f, 50f));
            // float x0, x360, y0, y360, z0, z360;
            // (z0, z360) = GetExtremum(rightLinearParameters[2]);
            // rightLinearParameters.Add(LinearCoefficient(z0, z360, 0f, 360f));

            leftLinearParameters.Add(Get3LinearParameters(leftMinAngles.x, leftMaxAngles.x, 290f, 305f, 'x'));
            leftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.y - 360, leftMinAngles.y, -60f, 60f, 'y'));
            leftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.z - 360, leftMinAngles.z, -90f, 90f, 'z'));

            leftLimitValues.Add(GetLimitValues(leftMinAngles.x, leftMaxAngles.x));
            leftLimitValues.Add(GetLimitValues(leftMaxAngles.y - 360, leftMinAngles.y));
            leftLimitValues.Add(GetLimitValues(leftMaxAngles.z - 360, leftMinAngles.z));

            Debug.Log("leftlimitvalues ="+ leftLimitValues[0] + leftLimitValues[1] + leftLimitValues[2]);

            // leftLinearParameters.Add(LinearCoefficient(leftMinAngles.x, leftMaxAngles.x, 290f, 310f));
            // leftLinearParameters.Add(LinearCoefficient(leftMaxAngles.y - 360 , leftMinAngles.y, -60f, 60f));
            // leftLinearParameters.Add(LinearCoefficient(leftMinAngles.z, leftMaxAngles.z, -90f, 90f));
            
            // leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.x, leftMaxAngles.x, 0f, 20f));
            // leftLinearParameters.Add(LinearCoefficient(leftNeutralOrientation.eulerAngles.y, leftMaxAngles.y, 0f, 50f));
            // (z0, z360) = GetExtremum(leftLinearParameters[2]);
            // leftLinearParameters.Add(LinearCoefficient(z0, z360, 0f, 360f));
            
        }

        public void GetFakeRescalingParameters() // à enlever si on garde la calibration
        {
            fakeRightLinearParameters.Add(Get3LinearParameters(rightMinAngles.x, rightMaxAngles.x, 290f, 310f, 'x'));
            fakeRightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.y - 360, rightMinAngles.y, -60f, 60f, 'y'));
            fakeRightLinearParameters.Add(Get3LinearParameters(rightMaxAngles.z - 360, rightMinAngles.z, -90f, 90f, 'z'));

            fakeLeftLinearParameters.Add(Get3LinearParameters(leftMinAngles.x, leftMaxAngles.x, 290f, 310f, 'x'));
            fakeLeftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.y - 360, leftMinAngles.y, -60f, 60f, 'y'));
            fakeLeftLinearParameters.Add(Get3LinearParameters(leftMaxAngles.z - 360, leftMinAngles.z, -90f, 90f, 'z'));

            
        }

        public LinearParameters LinearCoefficient(float x1, float x2, float y1, float y2)
        {
            float A = (y2 - y1) / (x2 - x1);
            float B = y1 - A * x1;
            LinearParameters linearParameters = new LinearParameters(A, B);
            return linearParameters;
        }

        public List<LinearParameters> Get3LinearParameters(float x1, float x2, float y1, float y2, char mode)
        {
            List<LinearParameters> parameters = new List<LinearParameters>();

            LinearParameters intervalParameters = LinearCoefficient(x1, x2, y1, y2);
            parameters.Add(intervalParameters);

            LinearParameters negParameters = new LinearParameters(0, 0);
            LinearParameters posParameters = new LinearParameters(0, 0);

            if (mode == 'x') 
            {
                negParameters = LinearCoefficient(0, x1, 0, y1);
                posParameters = LinearCoefficient(x2, 360, y2, 360);
            } 
            else 
            {
                float rangeX = 360 - (x2 - x1);
                float medianX = (x1 + x2) / 2;
                float rangeY = 360 - (y2 - y1);
                float medianY = (y1 + y2) / 2;
                negParameters = LinearCoefficient(x1 - rangeX/2, x1, y1 - rangeY/2, y1);
                posParameters = LinearCoefficient(x2, x2 + rangeX/2, y2, y2 + rangeY/2);
                
            }

            parameters.Add(negParameters);
            parameters.Add(posParameters);

            Debug.Log("Parameters: " + parameters[0].A + " " + parameters[0].b + " " + parameters[1].A + " " + parameters[1].b + " " + parameters[2].A + " " + parameters[2].b);

            return parameters;

        }

        public List<float> GetLimitValues (float min, float max) 
        {
            List<float> limitValues = new List<float>();
            float rangeX = 360 - (max - min);
            float limitDown = min - rangeX/2;
            float limitUp = max + rangeX/2;
            limitValues.Add(limitDown);
            limitValues.Add(min);
            limitValues.Add(max);
            limitValues.Add(limitUp);
            return limitValues;
        }


        public (float, float) GetExtremum(LinearParameters linearParameters)
        {

            float x0 = -linearParameters.b / linearParameters.A;
            float x360 = (360 - linearParameters.b) / linearParameters.A;
            return (x0, x360);
        }

        public void GetCalibrationQuaternion (Quaternion leftTargetQuat, Quaternion rightTargetQuat, Quaternion leftUserQuat, Quaternion rightUserQuat)
        {

            Quaternion leftRotationDifference = leftTargetQuat * Quaternion.Inverse(leftUserQuat);
            Quaternion rightRotationDifference = rightTargetQuat * Quaternion.Inverse(rightUserQuat);
            rightCalibrationQuaternions.Add(rightRotationDifference);
            leftCalibrationQuaternions.Add(leftRotationDifference);
            
        }

        public Quaternion AverageQuaternions(List<Quaternion> quaternions)
        {
            if (quaternions == null || quaternions.Count == 0)
                return Quaternion.identity;

            Quaternion average = quaternions[0].normalized;
            List<float> weights = new List<float> {1.0f, 3.0f,3.0f,1.0f,1.0f,1.0f,1.0f};
            float learningRate = 1.0f;
            const float threshold = 0.0001f;
            bool done = false;

            while (!done)
            {
                Quaternion incrementalAvg = Quaternion.identity;
                float totalWeight = 0.0f;

                for (int i = 0; i < quaternions.Count; i++)
                {
                    Quaternion q = quaternions[i].normalized;
                    if (Quaternion.Dot(average, q) < 0)
                        q = new Quaternion(-q.x, -q.y, -q.z, -q.w);

                    float weight = Quaternion.Angle(average, q) * weights[i];
                    incrementalAvg = Quaternion.Slerp(incrementalAvg, q, weight / (totalWeight + weight));
                    totalWeight += weight;
                } 

                Quaternion newAverage = Quaternion.Slerp(average, incrementalAvg, learningRate);
                if (Quaternion.Angle(average, newAverage) < threshold)
                {
                    done = true;
                }
                else
                {
                    average = newAverage;
                }
            }
            Debug.Log("Average quaternion: " + average.normalized.eulerAngles);

            return average.normalized;
        }


    }
}

