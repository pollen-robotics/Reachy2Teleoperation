using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;

namespace TeleopReachy
{
    public class RobotCalibration : Singleton<RobotCalibration>
    {
        public string name = "Pierre";
        private Transform trackedLeftHand;
        private Transform trackedRightHand;
        private Transform headset;
        private Transform userTrackerTransform;

        private List<Vector3> leftCoordinates = new List<Vector3>();
        private List<Quaternion> leftRotations = new List<Quaternion>();
        private List<Vector3> rightCoordinates = new List<Vector3>();
        private List<Quaternion> rightRotations = new List<Quaternion>();
        private List<Vector3> headsetPosition = new List<Vector3>();
        private List<Quaternion> headsetRotation = new List<Quaternion>();

        public double leftArmSize { get; set; }
        public double rightArmSize { get; set; }
        public double meanArmSize { get; set; }
        public double shoulderWidth { get; set; }

        private float intervalTime= 0.04f ; 
        private float actualTime = 0f;

        private bool calib_right_side = false;
        private bool fixed_right_frame = false; 
        private bool calib_left_side = false;
        private bool calibration_done = false;
        private bool start_calib_keyboard = false;
        private bool buttonX = false;

        private int validationPose = 0;
        
        private ControllersManager controllers;

        public UnityEvent event_OnCalibChanged;
        public UnityEvent event_WaitForCalib;
        public UnityEvent event_StartRightCalib;
        public UnityEvent event_StartLeftCalib;
        public UnityEvent event_ValidateCalib;
        public UnityEvent event_ModifyCalib;

        // à épurer derrière 
        private double calculatedRightArmSize = 0, 
                        approxrightarmsizex = 0, 
                        approxrightarmsizey = 0, 
                        approxrightarmsizexy = 0, 
                        calculatedLeftArmSize = 0, 
                        approxleftarmsizex = 0, 
                        approxleftarmsizey = 0, 
                        approxleftarmsizexy = 0;

                // Initialisation des variables de type Vector3
        private Vector3 rightRadii = new Vector3(0, 0, 0);
        private Vector3 rightShoulderCenter = new Vector3(0, 0, 0);
        private Vector3 leftRadii = new Vector3(0, 0, 0);
        private Vector3 leftShoulderCenter = new Vector3(0, 0, 0);
        private Matrix4x4 rightFrame = Matrix4x4.identity;
        private Matrix4x4 leftFrame = Matrix4x4.identity;
        private Vector3 rightShoulderCenterInHeadsetFrame = new Vector3(0, 0, 0);
        private Vector3 leftShoulderCenterInHeadsetFrame = new Vector3(0, 0, 0);




        private static RobotCalibration instance;

        public new static RobotCalibration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RobotCalibration>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(RobotCalibration).Name;
                        instance = obj.AddComponent<RobotCalibration>();
                    }
                }
                return instance;
            }
        }

     

        public void Start()
        {
            Debug.Log("Start of RobotCalibration");

            headset = GameObject.Find("Main Camera").transform;
            userTrackerTransform = GameObject.Find("UserTracker").transform; 
            trackedLeftHand = GameObject.Find("LeftHand Controller").transform;
            trackedRightHand = GameObject.Find("RightHand Controller").transform;

            controllers = ActiveControllerManager.Instance.ControllersManager;

            if (trackedLeftHand == null || trackedRightHand == null) {
                Debug.Log("Manettes non trouvées."); 
                return;
            } 

            event_OnCalibChanged = new UnityEvent();
            event_StartLeftCalib = new UnityEvent();
            event_StartRightCalib = new UnityEvent();
            event_WaitForCalib = new UnityEvent();  
            event_ValidateCalib = new UnityEvent();
            event_ModifyCalib = new UnityEvent();

            }

        public void Update()
        { 
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
            if (buttonX)
                {
                start_calib_keyboard = true;
                }

            if (!start_calib_keyboard && !calibration_done) 
            {
                event_WaitForCalib.Invoke();
                if (actualTime >= 1f && calib_right_side && !fixed_right_frame) {
                    FixFrame("right");
                    fixed_right_frame = true;
                    }
            }

            //capturing points from right side, then left side
            if (!calib_right_side && start_calib_keyboard) {
                //if (rightCoordinates.Count == 399) rightFrame = Matrix4x4.TRS(headset.position, Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0), Vector3.one);

                event_StartRightCalib.Invoke();
                CapturePoints("right");
            }

            else if (!calib_left_side && start_calib_keyboard){

                //get the center of the right side
                if (leftCoordinates.Count == 0) 
                {
                    RansacEllipsoidFit(rightCoordinates, "right");
                    GetCenterInHeadsetFrame("right");
                    //leftFrame = Matrix4x4.TRS(headset.position, Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0), Vector3.one);

                }
                //if (leftCoordinates.Count == 399) leftFrame = Matrix4x4.TRS(headset.position, Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0), Vector3.one);
                //capture the left side 
                event_StartLeftCalib.Invoke();
                CapturePoints("left");
                }

        
            else if (calib_left_side && !calibration_done)  
            {
                Debug.Log("actualTime"+ actualTime);
                if (actualTime >= 1f) 
                {
                    FixFrame("left");

                    //get the center of the left side
                    RansacEllipsoidFit(leftCoordinates, "left");
                    GetCenterInHeadsetFrame("left");

                    UpperBodyFeatures();
                    calibration_done = true;
                    UserSize.Instance.UpdateUserSizeafterCalibration_differentarms(leftArmSize, rightArmSize, shoulderWidth);
                    ExportCalibCoordinatesToCSV();

                    buttonX = false;
                    event_ValidateCalib.Invoke();
                    event_ModifyCalib.Invoke();
                }
            } 

            else if (calibration_done && validationPose < 4)
            {
                Debug.Log("condition validation pose");
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX && actualTime > 0.5f)  CaptureValidationGestures();

            }
            
            else if (validationPose == 4) 
            {
                Debug.Log("export de validation ok");
                validationPose ++;
                event_OnCalibChanged.Invoke();
                ExportValidationCoordinatesToCSV();
            }

            actualTime += Time.deltaTime;

      
        }

        private void CapturePoints (string side) 
        {
            if (side == "right"){
                if (rightCoordinates.Count < 400){
                    
                    if (actualTime >= intervalTime)
                    {
                        rightCoordinates.Add(trackedRightHand.position);
                        headsetPosition.Add(headset.position);
                        headsetRotation.Add(headset.rotation);
                        actualTime=0f;}
                } else {
                    calib_right_side = true;
                    start_calib_keyboard = false;
                    actualTime = 0f;}

            } else if (side == "left"){
                if (leftCoordinates.Count < 400)
                {
                    if (actualTime >= intervalTime)
                    {
                        leftCoordinates.Add(trackedLeftHand.position);
                        headsetPosition.Add(headset.position);
                        headsetRotation.Add(headset.rotation);
                        actualTime=0f;}
                } else  {
                    calib_left_side = true;
                    actualTime = 0f;
                }
            }
            
        }



        public void UpperBodyFeatures()
        {
            
            // on récupère les données du UserTracker comme s'il était fixé au moment de la fin de la calib 
            Vector3 headPosition = headset.position - headset.forward * 0.1f;
            Vector3 initialPosition = new Vector3(headPosition.x, headPosition.y - UserSize.Instance.UserShoulderHeadDistance, headPosition.z);
            Vector3 initialRotation = userTrackerTransform.rotation.eulerAngles;
            Vector3 newRotation = new Vector3(0, headset.rotation.eulerAngles.y,0);
            Matrix4x4 oldMatrix = Matrix4x4.TRS(initialPosition, Quaternion.Euler(initialRotation), Vector3.one);

            Matrix4x4 userTrackerInverseTransform = oldMatrix.inverse;
 
            leftArmSize = calculatedLeftArmSize;
            rightArmSize = calculatedRightArmSize;
            meanArmSize = (leftArmSize + rightArmSize) / 2f;

            // determine midshoulderpoint in headsetframe 
            Vector3 midShoulderPointinHeadsetFrame = (leftShoulderCenterInHeadsetFrame + rightShoulderCenterInHeadsetFrame) / 2f;

            // if (midShoulderPointinHeadsetFrame.x > 0.02f) 
            // {
            //     Debug.Log("MidShoulderPoint x too far from headset position: " + midShoulderPointinHeadsetFrame.x);
            //     midShoulderPointinHeadsetFrame.x = 0;
            // }
            Quaternion userActualOrientation = Quaternion.Euler(0,headset.rotation.eulerAngles.y, 0);
            Debug.Log("User actual orientation : " + userActualOrientation.eulerAngles);
            Matrix4x4 headTransform = Matrix4x4.TRS(headset.position, userActualOrientation, Vector3.one);
            Vector3 midShoulderPoint = headTransform.MultiplyPoint3x4(midShoulderPointinHeadsetFrame);

        //    //condition on the lateral translation of the midshoulderpoint 
        //     if (Math.Abs(midShoulderPoint.x - headset.position.x) > 0.02f) 
        //     {
        //         Debug.Log("MidShoulderPoint x too far from headset position: " + midShoulderPoint.x + " / headset position : " + headset.position.x);
        //         midShoulderPoint.x = headset.position.x;
        //     }
 
            TransitionRoomManager.Instance.midShoulderPoint = midShoulderPoint;
            TransitionRoomManager.Instance.userNewOrientation = userActualOrientation;

            Vector3 newCenterinOldFrame = userTrackerInverseTransform.MultiplyPoint3x4(midShoulderPoint);
            Debug.Log("Epaule G : " + leftShoulderCenter +"/ Epaule D : " + rightShoulderCenter + "/Milieu Epaule  : " + midShoulderPoint +", Taille moyenne des bras : " + meanArmSize);

            shoulderWidth = Vector3.Distance(leftShoulderCenter, rightShoulderCenter)/2f;

            //ajout des data dans un .csv
            var filePath = $@"C:\Users\robot\Dev\data_tests_{name}.csv";            
            var currentTime = DateTime.Now.ToString("ddMM_HHmm", CultureInfo.InvariantCulture);
             // date, position initiale, rotation initiale, nouvelle position, nouvelle rotation, nouveau centre dans repère de l'ancien, taille bras gauche, radii bras gauche, taille bras droit, radii bras droit, largeur épaules, position repère calib gauche, orientation repère calib gauche, position repère calib droite, orientation repère calib droite, centre épaule gauche dans repère headset, centre épaule droite dans repère headset, centre épaule gauche dans monde, centre épaule droite dans monde
            var data = $"{name},{currentTime},\"{initialPosition.z},{-initialPosition.x},{initialPosition.y}\",\"{initialRotation.z},{-initialRotation.x},{initialRotation.y}\",\"{midShoulderPoint.z},{-midShoulderPoint.x},{midShoulderPoint.y}\",\"{newRotation.z},{-newRotation.x},{newRotation.y}\",\"{newCenterinOldFrame.z},{-newCenterinOldFrame.x},{newCenterinOldFrame.y}\",{leftArmSize},\"{leftRadii.x},{leftRadii.y},{leftRadii.z}\",{rightArmSize},\"{rightRadii.x},{rightRadii.y},{rightRadii.z}\",{shoulderWidth},\"{leftFrame.GetPosition().z},{-leftFrame.GetPosition().x},{leftFrame.GetPosition().y}\",\"{leftFrame.rotation.eulerAngles.z},{-leftFrame.rotation.eulerAngles.x},{leftFrame.rotation.eulerAngles.y}\",\"{rightFrame.GetPosition().z},{-rightFrame.GetPosition().x},{rightFrame.GetPosition().y}\",\"{rightFrame.rotation.eulerAngles.z},{-rightFrame.rotation.eulerAngles.x},{rightFrame.rotation.eulerAngles.y}\",\"{leftShoulderCenterInHeadsetFrame.z},{-leftShoulderCenterInHeadsetFrame.x},{leftShoulderCenterInHeadsetFrame.y}\",\"{rightShoulderCenterInHeadsetFrame.z},{-rightShoulderCenterInHeadsetFrame.x},{rightShoulderCenterInHeadsetFrame.y}\",\"{leftShoulderCenter.z},{-leftShoulderCenter.x},{leftShoulderCenter.y}\",\"{rightShoulderCenter.z},{-rightShoulderCenter.x},{rightShoulderCenter.y}\"";
            using (var writer = new StreamWriter(filePath, true))
            {writer.WriteLine(data);}

            //ajout des data dans un .csv
            filePath = @"C:\Users\robot\Dev\dataunity_exhaustif.csv";
            currentTime = DateTime.Now.ToString("ddMM_HHmm", CultureInfo.InvariantCulture);
            data = $"{currentTime},{leftArmSize},{leftShoulderCenter.x},{leftShoulderCenter.y},{leftShoulderCenter.z},{rightArmSize},{rightShoulderCenter.x},{rightShoulderCenter.y},{rightShoulderCenter.z},{shoulderWidth},{meanArmSize},{midShoulderPoint.x},{midShoulderPoint.y},{midShoulderPoint.z},";
            using (var writer = new StreamWriter(filePath, true))
            {writer.WriteLine(data);}


            filePath = @"C:\Users\robot\Dev\armsize.csv";
            data = $"{currentTime},{leftArmSize},{approxleftarmsizex},{approxleftarmsizey},\"{leftRadii.x},{leftRadii.y},{leftRadii.z}\",{rightArmSize},{approxrightarmsizex},{approxrightarmsizey},\"{rightRadii.x},{rightRadii.y},{rightRadii.z}\",{meanArmSize}, {approxleftarmsizexy}, {approxrightarmsizexy}";
            using (var writer = new StreamWriter(filePath, true))
            {writer.WriteLine(data);}

            return;
        }

        public void FixFrame(string side)
        {
            Matrix4x4 headsetMatrix = Matrix4x4.TRS(headset.position, Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0), Vector3.one);
            if (side == "right") rightFrame = headsetMatrix;
            else leftFrame = headsetMatrix;
        }

        public void GetCenterInHeadsetFrame(string side) {
            Matrix4x4 headsetMatrix = Matrix4x4.identity;
            Vector3 globalPosition = Vector3.zero;
            // Matrix4x4 headsetMatrix = Matrix4x4.TRS(headset.position, Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0), Vector3.one);
            // Debug.Log("Headset matrix : " + headsetMatrix.rotation.eulerAngles+ "position : " + headsetMatrix.GetPosition());
            
            if (side == "right")  {
                globalPosition = rightShoulderCenter;
                headsetMatrix = rightFrame;
                }
            else if (side == "left") {
                globalPosition = leftShoulderCenter;
                headsetMatrix = leftFrame;
                }
            
            Matrix4x4 headsetInverseTransform = headsetMatrix.inverse;

            Debug.Log(side + "Center of shoulder in global frame : " + globalPosition);

            Vector3 positionInHeadsetFrame = headsetInverseTransform.MultiplyPoint3x4(globalPosition);
            Debug.Log(side+ "Center of shoulder in headset frame : " + positionInHeadsetFrame);
            
            if (side == "right") {
                rightShoulderCenterInHeadsetFrame = positionInHeadsetFrame;
                //rightFrame = headsetMatrix;
            }
            else {
                leftShoulderCenterInHeadsetFrame = positionInHeadsetFrame;
                //leftFrame = headsetMatrix;

            } 

            
        }


        public bool IsCalibrated (){
            return calibration_done;
        }

        public Quaternion ConvertOrientationUnityROS(Quaternion unityQuaternion)
        {
            Quaternion convertedQuaternion = new Quaternion(-unityQuaternion.z, unityQuaternion.x, -unityQuaternion.y, unityQuaternion.w);
            return convertedQuaternion;
        }


        public void ExportCalibCoordinatesToCSV()
        {
            Matrix4x4 rightFrameInverse = rightFrame.inverse;
            Matrix4x4 leftFrameInverse = leftFrame.inverse;
            Quaternion rightFrameQuaternion = rightFrame.rotation;
            Quaternion leftFrameQuaternion = leftFrame.rotation;


            string dateTimeString = DateTime.Now.ToString("ddMM_HHmm");
            string fileName = "DataCalib_" + dateTimeString + ".csv";
            
            List<Vector3> rightHandPositionsUserSpace = new List<Vector3>();
            List<Vector3> leftHandPositionsUserSpace = new List<Vector3>();
            List<Vector3> headsetPositionsUserSpace = new List<Vector3>();
            List<Vector3> headsetRotationsUserSpace = new List<Vector3>();
            int indice = 0 ;
            Debug.Log("UserTrackerTransform pour les data csv : "  + userTrackerTransform.position + " " + userTrackerTransform.rotation.eulerAngles);
            
            Debug.Log("nb de points dans headset pos : " + headsetPosition.Count + " headset rot : " + headsetRotation.Count + " left pos : " + leftCoordinates.Count + " right pos : " + rightCoordinates.Count );

            foreach (Vector3 handPosition in leftCoordinates)
            {
                // Convertissez la position en position par rapport au headset
                Vector3 handPositionUserSpace = leftFrameInverse.MultiplyPoint3x4(handPosition);
                handPositionUserSpace.y += headset.position.y;
                leftHandPositionsUserSpace.Add(handPositionUserSpace);
            }

            foreach (Vector3 handPosition in rightCoordinates)
            {
                // Convertissez la position en position par rapport au headset
                Vector3 handPositionUserSpace = rightFrameInverse.MultiplyPoint3x4(handPosition);
                handPositionUserSpace.y += headset.position.y;
                rightHandPositionsUserSpace.Add(handPositionUserSpace);
            }

            foreach (Vector3 headPosition in headsetPosition)
            {
                Vector3 headPositionUserSpace;
                if (indice < rightCoordinates.Count) headPositionUserSpace = rightFrameInverse.MultiplyPoint3x4(headPosition);
                else headPositionUserSpace = leftFrameInverse.MultiplyPoint3x4(headPosition);
                headPositionUserSpace.y += headset.position.y;
                headsetPositionsUserSpace.Add(headPositionUserSpace);
                indice ++;
            }

            indice = 0 ;

            foreach (Quaternion headRotation in headsetRotation)
            {

                Quaternion headRotationUserSpace; 
                if (indice < rightCoordinates.Count) headRotationUserSpace = Quaternion.Inverse(rightFrameQuaternion) * headRotation;
                else  headRotationUserSpace = Quaternion.Inverse(leftFrameQuaternion) * headRotation;
                // Convertissez la position en position par rapport au UserTracker
                headsetRotationsUserSpace.Add(headRotationUserSpace.eulerAngles);
                indice ++;
                
            }

            Debug.Log("en userspace, nb de points dans headset pos : " + headsetPositionsUserSpace.Count + " headset rot : " + headsetRotationsUserSpace.Count + " left pos : " + leftHandPositionsUserSpace.Count + " right pos : " + rightHandPositionsUserSpace.Count);

            using (FileStream fs = File.Create(Path.Combine($@"C:\Users\robot\Dev\data\Tests\{name}", fileName)))
            {
                string csvContent = "Side,X,Y,Z,X_headset,Y_headset,Z_headset,Xrot_headset,Yrot_headset,Zrot_headset,X_world,Y_world,Z_world,X_headset_world,Y_headset_world,Z_headset_world,Xrot_headset_world,Yrot_headset_world,Zrot_headset_world\n";
                for (int ite = 0; ite < rightHandPositionsUserSpace.Count; ite++)
                    {
                        csvContent += "Right," + rightHandPositionsUserSpace[ite].z + "," + -rightHandPositionsUserSpace[ite].x + "," + rightHandPositionsUserSpace[ite].y + ",";
                        csvContent += headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y +",";
                        csvContent += rightCoordinates[ite].z + "," + -rightCoordinates[ite].x + "," + rightCoordinates[ite].y + ",";
                        csvContent += headsetPosition[ite].z + ","+ -headsetPosition[ite].x + "," + headsetPosition[ite].y + "," + headsetRotation[ite].z + "," + -headsetRotation[ite].x + "," + headsetRotation[ite].y + "\n";
                    }

                for (int ite = 0; ite < leftHandPositionsUserSpace.Count; ite++)
                    {
                        csvContent += "Left," + leftHandPositionsUserSpace[ite].z + "," + -leftHandPositionsUserSpace[ite].x + "," + leftHandPositionsUserSpace[ite].y + "," ;
                        csvContent+= headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y + ",";
                        csvContent += leftCoordinates[ite].z + "," + -leftCoordinates[ite].x + "," + leftCoordinates[ite].y + ",";
                        csvContent += headsetPosition[ite].z + ","+ -headsetPosition[ite].x + "," + headsetPosition[ite].y + "," + headsetRotation[ite].z + "," + -headsetRotation[ite].x + "," + headsetRotation[ite].y + "\n";
                    }

                byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                fs.Write(csvBytes, 0, csvBytes.Length);

            }

            Debug.Log("Coordonnées exportées dans fichier CSV : " + fileName);
             // clear all the lists 
            leftCoordinates.Clear();
            rightCoordinates.Clear();
            headsetPosition.Clear();
            headsetRotation.Clear();
        }

        public void ExportValidationCoordinatesToCSV()
        {
            Debug.Log("UserTracker : " + userTrackerTransform.position + " rotation : " + userTrackerTransform.rotation.eulerAngles);
            Quaternion userTrackerrotation = userTrackerTransform.rotation;
            Matrix4x4 userTrackerInverseTransform = userTrackerTransform.worldToLocalMatrix;


            string dateTimeString = DateTime.Now.ToString("ddMM_HHmm");
            string fileName = "ValidationData_" + dateTimeString + ".csv";
            
            List<Vector3> rightHandPositionsUserSpace = new List<Vector3>();
            List<Vector3> rightHandRotationsUserSpace= new List<Vector3>();
            List<Vector3> leftHandPositionsUserSpace = new List<Vector3>();
            List<Vector3> leftHandRotationsUserSpace = new List<Vector3>();
            List<Vector3> headsetPositionsUserSpace = new List<Vector3>();
            List<Vector3> headsetRotationsUserSpace = new List<Vector3>();
            List<Quaternion> leftHandQuaternionsUserSpace = new List<Quaternion>();
            List<Quaternion> rightHandQuaternionsUserSpace = new List<Quaternion>();
            List<Quaternion> headsetQuaternionsUserSpace = new List<Quaternion>();
            List<Quaternion> leftHandQuaternionWorld = new List<Quaternion>();
            List<Quaternion> rightHandQuaternionWorld = new List<Quaternion>();
            List<Quaternion> headsetQuaternionWorld = new List<Quaternion>();
            List<Vector3> leftHandEulerWorld = new List<Vector3>();
            List<Vector3> rightHandEulerWorld = new List<Vector3>();
            List<Vector3> headsetEulerWorld = new List<Vector3>();

            Debug.Log("nb de points dans headset pos : " + headsetPosition.Count + " headset rot : " + headsetRotation.Count + " left pos : " + leftCoordinates.Count + " left rot : " + leftRotations.Count + " right pos : " + rightCoordinates.Count + " right rot : " + rightRotations.Count);

            foreach (Vector3 handPosition in leftCoordinates)
            {
                // Convertissez la position en position par rapport au UserTracker
                Vector3 handPositionUserSpace = userTrackerInverseTransform.MultiplyPoint3x4(handPosition);
                handPositionUserSpace.y += userTrackerTransform.position.y;
                leftHandPositionsUserSpace.Add(handPositionUserSpace);
            }

            foreach (Vector3 handPosition in rightCoordinates)
            {
                // Convertissez la position en position par rapport au UserTracker
                Vector3 handPositionUserSpace = userTrackerInverseTransform.MultiplyPoint3x4(handPosition);
                handPositionUserSpace.y += userTrackerTransform.position.y;
                rightHandPositionsUserSpace.Add(handPositionUserSpace);
            }

            foreach (Vector3 headPosition in headsetPosition)
            {
                // Convertissez la position en position par rapport au UserTracker
                Vector3 headPositionUserSpace = userTrackerInverseTransform.MultiplyPoint3x4(headPosition);
                headPositionUserSpace.y += userTrackerTransform.position.y;
                headsetPositionsUserSpace.Add(headPositionUserSpace);
            }

            foreach (Quaternion headRotation in headsetRotation)
            {
                // Convertissez la position en position par rapport au UserTracker
                headsetQuaternionWorld.Add(ConvertOrientationUnityROS(headRotation));
                Quaternion headRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * headRotation;
                headsetRotationsUserSpace.Add(ConvertOrientationUnityROS(headRotationUserSpace).eulerAngles);
                headsetQuaternionsUserSpace.Add(ConvertOrientationUnityROS(headRotationUserSpace));
                headsetEulerWorld.Add(new Vector3(headRotation.eulerAngles.z, -headRotation.eulerAngles.x, headRotation.eulerAngles.y));
            }

            foreach (Quaternion handRotation in rightRotations)
            {
                // Convertissez la rotation en rotation par rapport au UserTracker
                rightHandQuaternionWorld.Add(ConvertOrientationUnityROS(handRotation));
                Quaternion handRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * handRotation;
                rightHandRotationsUserSpace.Add(ConvertOrientationUnityROS(handRotationUserSpace).eulerAngles);
                rightHandQuaternionsUserSpace.Add(ConvertOrientationUnityROS(handRotationUserSpace));
                rightHandEulerWorld.Add(new Vector3(handRotation.eulerAngles.z, -handRotation.eulerAngles.x, handRotation.eulerAngles.y));

            }
            foreach (Quaternion handRotation in leftRotations)
            {
                // Convertissez la rotation en rotation par rapport au UserTracker
                leftHandQuaternionWorld.Add(ConvertOrientationUnityROS(handRotation));
                Quaternion handRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * handRotation;
                leftHandRotationsUserSpace.Add(ConvertOrientationUnityROS(handRotationUserSpace).eulerAngles);
                leftHandQuaternionsUserSpace.Add(ConvertOrientationUnityROS(handRotationUserSpace));
                leftHandEulerWorld.Add(new Vector3 (handRotation.eulerAngles.z, -handRotation.eulerAngles.x, handRotation.eulerAngles.y));
            }


            Debug.Log("en userspace, nb de points dans headset pos : " + headsetPositionsUserSpace.Count + " headset rot : " + headsetRotationsUserSpace.Count + " left pos : " + leftHandPositionsUserSpace.Count + " left rot : " + leftHandRotationsUserSpace.Count + " right pos : " + rightHandPositionsUserSpace.Count + " right rot : " + rightHandRotationsUserSpace.Count);

            using (FileStream fs = File.Create(Path.Combine($@"C:\Users\robot\Dev\data\Tests\{name}\validation", fileName)))
            {
                string csvContent= "Side,X,Y,Z,X_rot,Y_rot,Z_rot,X_quat,Y_quat,Z_quat,W_quat,X_headset,Y_headset,Z_headset,Xrot_headset,Yrot_headset,Zrot_headset,Xquat_headset,Yquat_headset,Zquat_headset,Wquat_headset,X_world,Y_world,Z_world,Xrot_world,Yrot_world,Zrot_world,Xquat_world,Yquat_world,Zquat_world,Wquat_world,X_headset_world,Y_headset_world,Z_headset_world,Xrot_headset_world,Yrot_headset_world,Zrot_headset_world,Xquat_headset_world,Yquat_headset_world,Zquat_headset_world,Wquat_headset_world\n";
                for (int ite = 0; ite < rightHandPositionsUserSpace.Count; ite++)
                    {
                        csvContent += "Right," + rightHandPositionsUserSpace[ite].z + "," + -rightHandPositionsUserSpace[ite].x + "," + rightHandPositionsUserSpace[ite].y + "," + rightHandRotationsUserSpace[ite].x + "," + rightHandRotationsUserSpace[ite].y + "," + rightHandRotationsUserSpace[ite].z + ",";
                        csvContent += rightHandQuaternionsUserSpace[ite].x + "," + rightHandQuaternionsUserSpace[ite].y + "," + rightHandQuaternionsUserSpace[ite].z + "," + rightHandQuaternionsUserSpace[ite].w + ",";
                        csvContent += headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + ",";
                        csvContent += headsetQuaternionsUserSpace[ite].x + "," + headsetQuaternionsUserSpace[ite].y + "," + headsetQuaternionsUserSpace[ite].z + "," + headsetQuaternionsUserSpace[ite].w + ",";
                        csvContent += rightCoordinates[ite].z + "," + -rightCoordinates[ite].x + "," + rightCoordinates[ite].y + "," + rightHandEulerWorld[ite].x + "," + rightHandEulerWorld[ite].y + "," + rightHandEulerWorld[ite].z + ",";
                        csvContent += rightHandQuaternionWorld[ite].x + "," + rightHandQuaternionWorld[ite].y + "," + rightHandQuaternionWorld[ite].z + "," + rightHandQuaternionWorld[ite].w + ",";
                        csvContent += headsetPosition[ite].z + ","+ -headsetPosition[ite].x + "," + headsetPosition[ite].y + "," + headsetEulerWorld[ite].x + "," + headsetEulerWorld[ite].y + "," + headsetEulerWorld[ite].z + ",";
                        csvContent += headsetQuaternionWorld[ite].x + "," + headsetQuaternionWorld[ite].y + "," + headsetQuaternionWorld[ite].z + "," + headsetQuaternionWorld[ite].w + "\n";
                   
                    }

                for (int ite = 0; ite < leftHandPositionsUserSpace.Count; ite++)
                    {
                        csvContent += "Left," + leftHandPositionsUserSpace[ite].z + "," + -leftHandPositionsUserSpace[ite].x + "," + leftHandPositionsUserSpace[ite].y + "," + leftHandRotationsUserSpace[ite].x + "," + leftHandRotationsUserSpace[ite].y + "," + leftHandRotationsUserSpace[ite].z + ",";
                        csvContent += leftHandQuaternionsUserSpace[ite].x + "," + leftHandQuaternionsUserSpace[ite].y + "," + leftHandQuaternionsUserSpace[ite].z + "," + leftHandQuaternionsUserSpace[ite].w + ",";
                        csvContent += headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + ",";
                        csvContent += headsetQuaternionsUserSpace[ite].x + "," + headsetQuaternionsUserSpace[ite].y + "," + headsetQuaternionsUserSpace[ite].z + "," + headsetQuaternionsUserSpace[ite].w + ",";
                        csvContent += leftCoordinates[ite].z + "," + -leftCoordinates[ite].x + "," + leftCoordinates[ite].y + "," + leftHandEulerWorld[ite].x + "," + leftHandEulerWorld[ite].y + "," + leftHandEulerWorld[ite].z + ",";
                        csvContent += leftHandQuaternionWorld[ite].x + "," + leftHandQuaternionWorld[ite].y + "," + leftHandQuaternionWorld[ite].z + "," + leftHandQuaternionWorld[ite].w + ",";
                        csvContent += headsetPosition[ite].z + ","+ -headsetPosition[ite].x + "," + headsetPosition[ite].y + "," + headsetEulerWorld[ite].x + "," + headsetEulerWorld[ite].y + "," + headsetEulerWorld[ite].z + ",";
                        csvContent += headsetQuaternionWorld[ite].x + "," + headsetQuaternionWorld[ite].y + "," + headsetQuaternionWorld[ite].z + "," + headsetQuaternionWorld[ite].w + "\n";
                   
                    }

                byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                fs.Write(csvBytes, 0, csvBytes.Length);

            }

            Debug.Log("Coordonnées exportées dans fichier CSV : " + fileName);
             // clear all the lists 
        }


        public (double, double, double, double, Vector3, Vector3) EllipsoidFitAleksander(List<Vector3> points)
        {
            var D = Matrix<double>.Build.DenseOfRowArrays(points.Select(p => new double[] {
                p.x * p.x + p.y * p.y - 2 * p.z * p.z,
                p.x * p.x + p.z * p.z - 2 * p.y * p.y,
                2 * p.x, 2 * p.y, 2 * p.z,
                1
            }).ToArray());

            var d2 = Vector<double>.Build.DenseOfEnumerable(points.Select(p => (double)p.x * p.x + (double)p.y * p.y + (double)p.z * p.z));
            var u = D.TransposeThisAndMultiply(D).Solve(D.TransposeThisAndMultiply(d2));

            double a = u[0] + u[1] - 1;
            double b = u[0] - 2 * u[1] - 1;
            double c = u[1] - 2 * u[0] - 1;
            var zs = Vector<double>.Build.Dense(3);
            var v = Vector<double>.Build.DenseOfEnumerable(new double[] {a, b, c}.Concat(zs).Concat(u.SubVector(2, u.Count - 2)));

            var A = Matrix<double>.Build.DenseOfRowArrays(
                new double[] {v[0], v[3], v[4], v[6]},
                new double[] {v[3], v[1], v[5], v[7]},
                new double[] {v[4], v[5], v[2], v[8]},
                new double[] {v[6], v[7], v[8], v[9]});

            var centre = (-A.SubMatrix(0, 3, 0, 3)).Solve(v.SubVector(6, 3));
            var T = DenseMatrix.CreateIdentity(4);
            T[3, 0] = centre[0];
            T[3, 1] = centre[1];
            T[3, 2] = centre[2];            

            var R = T.Multiply(A).Multiply(T.Transpose());
            var eig = R.SubMatrix(0, 3, 0, 3).Divide(-R[3, 3]).Evd();
            var evals = eig.EigenValues.Real();

            var radii = evals.Map(value => Math.Sqrt(1 / Math.Abs(value)));
            Vector3 radiiVector3 = new Vector3((float)radii[0], (float)radii[1], (float)radii[2]);
            double meanRadius = radii.Median();
            Vector3 centreVector3 = new Vector3((float)centre[0], (float)centre[1], (float)centre[2]);
            
            double xThreshold = 0.07;
            var filteredPointsx = points.Where(p => Math.Abs(p.x - centre[0]) <= xThreshold);
            var distancesx = filteredPointsx.Select(p => Vector3.Distance(centreVector3, p));

            double yThreshold = 0.07;
            var filteredPointsy = points.Where(p => Math.Abs(p.y - centre[1]) <= yThreshold);
            var distancesy = filteredPointsy.Select(p => Vector3.Distance(centreVector3, p));

            var filteredPointsxy = points.Where(p => 
                p.x >= centre[0] - xThreshold && p.x <= centre[0] + xThreshold &&
                p.y >= centre[1] - yThreshold && p.y <= centre[1] + yThreshold
            );   
            var distancesxy = filteredPointsxy.Select(p => Vector3.Distance(centreVector3, p));

            // Calcul de approxRadiusx
            double approxRadiusx = filteredPointsx.Any() ? distancesx.Average() : 0.0;
            // Calcul de approxRadiusy
            double approxRadiusy = filteredPointsy.Any() ? distancesy.Average() : 0.0;            

            double approxRadiusxy = filteredPointsxy.Any() ? distancesxy.Average() : 0.0;



            return (meanRadius, approxRadiusx, approxRadiusy, approxRadiusxy, radiiVector3, centreVector3);

        }

        //ajout d'un algorithme de RANSAC qui utilise la fonction EllipsoidFitAleksander avec des échantillons de points aléatoires pour la détection de l'épaule
        public void RansacEllipsoidFit(List<Vector3> points, string side)
        {
            int n = points.Count;
            int maxIterations = 500;
            int sampleSize = 100;
            double threshold = 0.1;
            double bestError = double.MaxValue;
            (double, double, double, double, Vector3, Vector3) bestModel = (0, 0, 0, 0, Vector3.zero, Vector3.zero);

            for (int i = 0; i < maxIterations; i++)
            {
                List<Vector3> sample = new List<Vector3>();
                for (int j = 0; j < sampleSize; j++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, n);
                    sample.Add(points[randomIndex]);
                }

                (double, double, double, double, Vector3, Vector3) model = EllipsoidFitAleksander(sample);
                double error = 0;
                foreach (Vector3 point in points)
                {
                    double distance = Vector3.Distance(point, model.Item5);
                    if (distance > threshold)
                        error += distance;
                }

                if (error < bestError)
                {
                    bestError = error;
                    bestModel = model;
                }
            }

            if (bestModel.Item1 < 0.4)
            {
                Debug.Log("Erreur de calibration : bras trop petit, recalibrez.");
                calib_left_side = false;
                calib_right_side = false;
                return;
            }

            if (side == "right")
            {
                calculatedRightArmSize = bestModel.Item1;
                approxrightarmsizex = bestModel.Item2;
                approxrightarmsizey = bestModel.Item3;
                approxrightarmsizexy = bestModel.Item4;
                rightRadii = bestModel.Item5;
                rightShoulderCenter = bestModel.Item6;
            }
            else if (side == "left")
            {
                calculatedLeftArmSize = bestModel.Item1;
                approxleftarmsizex = bestModel.Item2;
                approxleftarmsizey = bestModel.Item3;
                approxleftarmsizexy = bestModel.Item4;
                leftRadii = bestModel.Item5;
                leftShoulderCenter = bestModel.Item6;
            }


        }

        public void CaptureValidationGestures()
        {
            Debug.Log("CaptureValidationGestures");
            rightCoordinates.Add(trackedRightHand.position);
            leftCoordinates.Add(trackedLeftHand.position);
            rightRotations.Add(trackedRightHand.rotation);
            leftRotations.Add(trackedLeftHand.rotation);
            headsetPosition.Add(headset.position);
            headsetRotation.Add(headset.rotation);
            event_ValidateCalib.Invoke();
            validationPose ++;
            actualTime = 0f;
        }

    }
}
