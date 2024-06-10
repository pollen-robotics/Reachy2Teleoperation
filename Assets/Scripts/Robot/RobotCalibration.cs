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
        private Transform trackedLeftHand;
        private Transform trackedRightHand;
        private Transform headset;
        private Transform userTrackerTransform;

        private List<Vector3> leftCoordinates = new List<Vector3>();
        private List<Vector3> leftRotations = new List<Vector3>();
        private List<Vector3> rightCoordinates = new List<Vector3>();
        private List<Vector3> rightRotations = new List<Vector3>();
        private List<Vector3> headsetPosition = new List<Vector3>();
        private List<Vector3> headsetRotation = new List<Vector3>();

        public double leftArmSize { get; set; }
        public double rightArmSize { get; set; }
        public double meanArmSize { get; set; }
        public double shoulderWidth { get; set; }

        private float intervalTime= 0.04f ; 
        private float actualTime = 0f;

        private bool calib_right_side = false;
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

            }

        public void Update()
        { 
            controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
            if (buttonX)
                start_calib_keyboard = true;

            if (!start_calib_keyboard && !calibration_done) 
            {
                event_WaitForCalib.Invoke();
                actualTime = 0f;
            }

            //capturing points from right side, then left side
            if (!calib_right_side && start_calib_keyboard) {
                event_StartRightCalib.Invoke();
                CapturePoints("right");
                actualTime += Time.deltaTime;}

            else if (!calib_left_side && start_calib_keyboard){
                event_StartLeftCalib.Invoke();

                //get the center of the right side
                if (leftCoordinates.Count == 0) 
                {
                    RansacEllipsoidFit(rightCoordinates, "right");
                    GetCenterInHeadsetFrame("right");
                }
                //capture the left side 
                CapturePoints("left");
                actualTime += Time.deltaTime;}

        
            else if (calib_left_side && !calibration_done)  
            {
                //get the center of the left side
                RansacEllipsoidFit(leftCoordinates, "left");
                GetCenterInHeadsetFrame("left");

                UpperBodyFeatures();
                calibration_done = true;
                UserSize.Instance.UpdateUserSizeafterCalibration_differentarms(leftArmSize, rightArmSize, shoulderWidth);
                ExportCalibCoordinatesToCSV();

                buttonX = false;
                event_ValidateCalib.Invoke();
            } 

            else if (calibration_done && validationPose < 3)
            {
                actualTime += Time.deltaTime;
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX && actualTime > 0.5f)  CaptureValidationGestures();

            }
            
            else if (validationPose == 3) 
            {
                Debug.Log("export de validation ok");
                validationPose ++;
                event_OnCalibChanged.Invoke();
                ExportValidationCoordinatesToCSV();
            }
      
        }

        private void CapturePoints (string side) 
        {
            if (side == "right"){
                if (rightCoordinates.Count < 400){
                    
                    if (actualTime >= intervalTime)
                    {
                        rightCoordinates.Add(trackedRightHand.position);
                        headsetPosition.Add(headset.position);
                        headsetRotation.Add(headset.rotation.eulerAngles);
                        actualTime=0f;}
                } else {
                    calib_right_side = true;
                    start_calib_keyboard = false;}

            } else if (side == "left"){
                if (leftCoordinates.Count < 400)
                {
                    if (actualTime >= intervalTime)
                    {
                        leftCoordinates.Add(trackedLeftHand.position);
                        headsetPosition.Add(headset.position);
                        headsetRotation.Add(headset.rotation.eulerAngles);
                        actualTime=0f;}
                } else  {
                    calib_left_side = true;
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
 
            leftArmSize = Math.Min(calculatedLeftArmSize, calculatedRightArmSize);
            rightArmSize = leftArmSize;
            meanArmSize = (leftArmSize + rightArmSize) / 2f;

            // determine midshoulderpoint in headsetframe 
            Vector3 midShoulderPointinHeadsetFrame = (leftShoulderCenterInHeadsetFrame + rightShoulderCenterInHeadsetFrame) / 2f;
            Matrix4x4 headTransform = Matrix4x4.TRS(headset.position, Quaternion.Euler(0,headset.rotation.y, 0), Vector3.one);
            Vector3 midShoulderPoint = headTransform.MultiplyPoint3x4(midShoulderPointinHeadsetFrame);


            //Vector3 midShoulderPoint = (leftShoulderCenter + rightShoulderCenter) / 2f;
            TransitionRoomManager.Instance.midShoulderPoint = midShoulderPoint;
            Vector3 newCenterinOldFrame = userTrackerInverseTransform.MultiplyPoint3x4(midShoulderPoint);
            Debug.Log("Epaule G : " + leftShoulderCenter +"/ Epaule D : " + rightShoulderCenter + "/Milieu Epaule  : " + midShoulderPoint +", Taille moyenne des bras : " + meanArmSize);

            shoulderWidth = Vector3.Distance(leftShoulderCenter, rightShoulderCenter)/2f;

            //ajout des data dans un .csv
            var filePath = @"C:\Users\robot\Dev\data_tests.csv";            
            var currentTime = DateTime.Now.ToString("ddMM_HHmm", CultureInfo.InvariantCulture);
             // date, position initiale, rotation initiale, nouvelle position, nouvelle rotation, nouveau centre dans repère de l'ancien, taille bras gauche, radii bras gauche, taille bras droit, radii bras droit, largeur épaules
            var data = $"{currentTime},\"{initialPosition.z},{-initialPosition.x},{initialPosition.y}\",\"{initialRotation.z},{-initialRotation.x},{initialRotation.y}\",\"{midShoulderPoint.z},{-midShoulderPoint.x},{midShoulderPoint.y}\", \"{newRotation.z},{-newRotation.x},{newRotation.y}\",\"{newCenterinOldFrame.z},{-newCenterinOldFrame.x}, {newCenterinOldFrame.y}\",{leftArmSize},\"{leftRadii.x},{leftRadii.y},{leftRadii.z}\",{rightArmSize},\"{rightRadii.x},{rightRadii.y},{rightRadii.z}\",{shoulderWidth}";
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

        public void GetCenterInHeadsetFrame(string side) {
            Matrix4x4 headsetMatrix = Matrix4x4.TRS(headset.position, Quaternion.Euler(0, headset.rotation.eulerAngles.y, 0), Vector3.one);
            Debug.Log("Headset matrix : " + headsetMatrix.rotation.eulerAngles+ "position : " + headsetMatrix.GetPosition());
            Matrix4x4 headsetInverseTransform = headsetMatrix.inverse;


    	    Vector3 globalPosition = Vector3.zero;
            if (side == "right")  globalPosition = rightShoulderCenter;
            else if (side == "left")  globalPosition = leftShoulderCenter;

            Debug.Log(side + "Center of shoulder in global frame : " + globalPosition);

            Vector3 positionInHeadsetFrame = headsetInverseTransform.MultiplyPoint3x4(globalPosition);
            Debug.Log(side+ "Center of shoulder in headset frame : " + positionInHeadsetFrame);
            
            if (side == "right") {
                rightShoulderCenterInHeadsetFrame = positionInHeadsetFrame;
                rightFrame = headsetMatrix;
            }
            else {
                leftShoulderCenterInHeadsetFrame = positionInHeadsetFrame;
                leftFrame = headsetMatrix;


            } 

            
        }


        public bool IsCalibrated (){
            return calibration_done;
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
            int ite = 0;
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

            foreach (Vector3 headRotation in headsetRotation)
            {

                Quaternion headRotationUserSpace; 
                if (indice < rightCoordinates.Count) headRotationUserSpace = Quaternion.Inverse(rightFrameQuaternion) * Quaternion.Euler(headRotation);
                else  headRotationUserSpace = Quaternion.Inverse(leftFrameQuaternion) * Quaternion.Euler(headRotation);
                // Convertissez la position en position par rapport au UserTracker
                headsetRotationsUserSpace.Add(headRotationUserSpace.eulerAngles);
                indice ++;
                
            }

            Debug.Log("en userspace, nb de points dans headset pos : " + headsetPositionsUserSpace.Count + " headset rot : " + headsetRotationsUserSpace.Count + " left pos : " + leftHandPositionsUserSpace.Count + " right pos : " + rightHandPositionsUserSpace.Count);

            using (FileStream fs = File.Create(Path.Combine(@"C:\Users\robot\Dev\data\Tests\Claire", fileName)))
            {
                string csvContent = "Side,X,Y,Z,X_headset,Y_headset,Z_headset,Xrot_headset,Yrot_headset,Zrot_headset\n";
                foreach (Vector3 point in rightHandPositionsUserSpace)
                    {
                        csvContent += "Right," + point.z + "," + -point.x + "," + point.y + ",";
                        csvContent += headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y + "\n";
                        ite= ite +1;
                    }

                foreach (Vector3 point in leftHandPositionsUserSpace)
                    {
                        csvContent += "Left," + point.z + "," + -point.x + "," + point.y + "," ;
                        csvContent+= headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y +"\n";
                        ite= ite +1;
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
            ite = 0;
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
            int ite = 0;
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

            foreach (Vector3 headRotation in headsetRotation)
            {
                // Convertissez la position en position par rapport au UserTracker
                Quaternion headRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * Quaternion.Euler(headRotation);
                headsetRotationsUserSpace.Add(headRotationUserSpace.eulerAngles);
            }

            foreach (Vector3 handRotation in rightRotations)
            {
                // Convertissez la rotation en rotation par rapport au UserTracker
                Quaternion handRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * Quaternion.Euler(handRotation);
                rightHandRotationsUserSpace.Add(handRotationUserSpace.eulerAngles);
            }
            foreach (Vector3 handRotation in leftRotations)
            {
                // Convertissez la rotation en rotation par rapport au UserTracker
                Quaternion handRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * Quaternion.Euler(handRotation);
                leftHandRotationsUserSpace.Add(handRotationUserSpace.eulerAngles);
            }


            Debug.Log("en userspace, nb de points dans headset pos : " + headsetPositionsUserSpace.Count + " headset rot : " + headsetRotationsUserSpace.Count + " left pos : " + leftHandPositionsUserSpace.Count + " left rot : " + leftHandRotationsUserSpace.Count + " right pos : " + rightHandPositionsUserSpace.Count + " right rot : " + rightHandRotationsUserSpace.Count);

            using (FileStream fs = File.Create(Path.Combine(@"C:\Users\robot\Dev\data\Tests\Claire\validation", fileName)))
            {
                string csvContent= "Side,X,Y,Z,X_rot, Y_rot,Z_rot,X_headset,Y_headset,Z_headset,Xrot_headset,Yrot_headset,Zrot_headset\n";
                foreach (Vector3 point in rightHandPositionsUserSpace)
                    {
                        csvContent += "Right," + point.z + "," + -point.x + "," + point.y + "," + rightHandRotationsUserSpace[ite].z + "," + -rightHandRotationsUserSpace[ite].x + "," + rightHandRotationsUserSpace[ite].y + ",";
                        csvContent += headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y + "\n";
                        ite= ite +1;
                    }
                ite = 0;

                foreach (Vector3 point in leftHandPositionsUserSpace)
                    {
                        csvContent += "Left," + point.z + "," + -point.x + "," + point.y + "," + leftHandRotationsUserSpace[ite].z + "," + -leftHandRotationsUserSpace[ite].x + "," + leftHandRotationsUserSpace[ite].y + ",";
                        csvContent+= headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y +"\n";
                        ite= ite +1;
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
            rightRotations.Add(trackedRightHand.rotation.eulerAngles);
            leftRotations.Add(trackedLeftHand.rotation.eulerAngles);
            headsetPosition.Add(headset.position);
            headsetRotation.Add(headset.rotation.eulerAngles);
            event_ValidateCalib.Invoke();
            validationPose ++;
            actualTime = 0f;
        }

    }
}
