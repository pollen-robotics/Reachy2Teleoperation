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

        private Vector3 lastPointLeft;
        private Vector3 lastPointRight;

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
        private ControllersManager controllers;
        private bool start_calib_keyboard = false;
        private bool buttonX = false;
        private int validationPose = 0;
        public UnityEvent event_OnCalibChanged;
        public UnityEvent event_WaitForCalib;
        public UnityEvent event_StartRightCalib;
        public UnityEvent event_StartLeftCalib;
        public UnityEvent event_ValidateCalib;






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
            trackedLeftHand = GameObject.Find("LeftHand Controller").transform;
            trackedRightHand = GameObject.Find("RightHand Controller").transform;
            controllers = ActiveControllerManager.Instance.ControllersManager;

            if (trackedLeftHand == null || trackedRightHand == null) {
                Debug.Log("Manettes non trouvées."); 
                return;
            } 

            lastPointLeft = trackedLeftHand.position;
            lastPointRight = trackedRightHand.position;
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
                CapturePoints("left");
                actualTime += Time.deltaTime;}

        
            else if (calib_left_side && !calibration_done)  
            {
                UpperBodyFeatures();
                calibration_done = true;
                //event_OnCalibChanged.Invoke();
                UserSize.Instance.UpdateUserSizeafterCalibration_differentarms(leftArmSize, rightArmSize, shoulderWidth);
                Debug.Log("calib ok");
                ExportCoordinatesToCSV("calib");
                Debug.Log("export de calib ok");    
                buttonX = false;
                event_ValidateCalib.Invoke();
            } 

            else if (calibration_done && validationPose < 3)
            {
                Debug.Log("validatePose");
                actualTime += Time.deltaTime;
                controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonX);
                if (buttonX && actualTime > 0.5f)  CaptureValidationGestures();

            }
            
            else if (validationPose == 3) 
            {
                ExportCoordinatesToCSV("validation");
                Debug.Log("export de validation ok");
                validationPose ++;
                event_OnCalibChanged.Invoke();
            }
      
        }

        private void CapturePoints (string side) 
        {
            Debug.Log("actualTime=" + actualTime);
            if (side == "right"){
                if (rightCoordinates.Count < 400){
                    
                    if (actualTime >= intervalTime)
                    {
                        rightCoordinates.Add(trackedRightHand.position);
                        headsetPosition.Add(headset.position);
                        headsetRotation.Add(headset.rotation.eulerAngles);
                        Debug.Log("add headset pos "+ headsetPosition.Count+ " rotation " + headsetRotation.Count);
                        lastPointRight = trackedRightHand.position;
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
                        Debug.Log("add headset pos "+ headsetPosition.Count+ " rotation " + headsetRotation.Count);
                        lastPointLeft = trackedLeftHand.position;
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
            Vector3 initialRotation = TransitionRoomManager.Instance.oldUserCenter.rotation.eulerAngles;
            Vector3 newRotation = new Vector3(0, headset.rotation.eulerAngles.y,0);
            Matrix4x4 oldMatrix = Matrix4x4.TRS(initialPosition, Quaternion.Euler(initialRotation), Vector3.one);

            Matrix4x4 userTrackerInverseTransform = oldMatrix.inverse;
 
            (double calculatedLeftArmSize, double approxleftarmsizex, double approxleftarmsizey, double approxleftarmsizexy, Vector3 leftRadii, Vector3 leftShoulderCenter) = RansacEllipsoidFit(leftCoordinates);
            (double calculatedRightArmSize, double approxrightarmsizex, double approxrightarmsizey, double approxrightarmsizexy, Vector3 rightRadii, Vector3 rightShoulderCenter) = RansacEllipsoidFit(rightCoordinates);
            Debug.Log("LSM des deux côtés ok");

            leftArmSize = Math.Min(calculatedLeftArmSize, calculatedRightArmSize);
            rightArmSize = leftArmSize;
            meanArmSize = (leftArmSize + rightArmSize) / 2f;

            Vector3 midShoulderPoint = (leftShoulderCenter + rightShoulderCenter) / 2f;
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


        public bool IsCalibrated (){
            return calibration_done;
        }

        public void ExportCoordinatesToCSV(string type)
        {
            userTrackerTransform = GameObject.Find("UserTracker").transform;
            Quaternion userTrackerrotation = userTrackerTransform.rotation;
            Matrix4x4 userTrackerInverseTransform = userTrackerTransform.worldToLocalMatrix;

            string dateTimeString = DateTime.Now.ToString("ddMM_HHmm");
            string fileName;
            if (type == "calib") fileName = "DataCalib_" + dateTimeString + ".csv";
            else fileName = "ValidationData_" + dateTimeString + ".csv";
            
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
                Vector3 handPositionUserSpace = userTrackerInverseTransform.MultiplyPoint(handPosition);
                handPositionUserSpace.y += userTrackerTransform.position.y;
                leftHandPositionsUserSpace.Add(handPositionUserSpace);
            }

            foreach (Vector3 handPosition in rightCoordinates)
            {
                // Convertissez la position en position par rapport au UserTracker
                Vector3 handPositionUserSpace = userTrackerInverseTransform.MultiplyPoint(handPosition);
                handPositionUserSpace.y += userTrackerTransform.position.y;
                rightHandPositionsUserSpace.Add(handPositionUserSpace);
            }

            foreach (Vector3 headPosition in headsetPosition)
            {
                // Convertissez la position en position par rapport au UserTracker
                Vector3 headPositionUserSpace = userTrackerInverseTransform.MultiplyPoint(headPosition);
                headPositionUserSpace.y += userTrackerTransform.position.y;
                headsetPositionsUserSpace.Add(headPositionUserSpace);
            }

            foreach (Vector3 headRotation in headsetRotation)
            {
                // Convertissez la position en position par rapport au UserTracker
                Quaternion headRotationUserSpace = Quaternion.Inverse(userTrackerrotation) * Quaternion.Euler(headRotation);
                headsetRotationsUserSpace.Add(headRotationUserSpace.eulerAngles);
            }

            if (rightRotations.Count > 0)
            {
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
                //headsetPositionsUserSpace = headsetPositionsUserSpace.Concat(headsetPositionsUserSpace).ToList();
                //headsetRotationsUserSpace = headsetRotationsUserSpace.Concat(headsetRotationsUserSpace).ToList();
            }

            Debug.Log("en userspace, nb de points dans headset pos : " + headsetPositionsUserSpace.Count + " headset rot : " + headsetRotationsUserSpace.Count + " left pos : " + leftHandPositionsUserSpace.Count + " left rot : " + leftHandRotationsUserSpace.Count + " right pos : " + rightHandPositionsUserSpace.Count + " right rot : " + rightHandRotationsUserSpace.Count);

            using (FileStream fs = File.Create(Path.Combine(@"C:\Users\robot\Dev\data\Tests\Claire", fileName)))
            {
                string csvContent; 
                if (type == "validation") csvContent = "Side,X,Y,Z,X_rot, Y_rot,Z_rot,X_headset,Y_headset,Z_headset,Xrot_headset,Yrot_headset,Zrot_headset\n";
                else  csvContent = "Side,X,Y,Z,X_headset,Y_headset,Z_headset,Xrot_headset,Yrot_headset,Zrot_headset\n";
                foreach (Vector3 point in rightHandPositionsUserSpace)
                    {
                        Debug.Log("RightSide : ite " + ite );
                        csvContent += "Right," + point.z + "," + -point.x + "," + point.y + ",";
                        if (type == "validation") csvContent += rightHandRotationsUserSpace[ite].z + "," + -rightHandRotationsUserSpace[ite].x + "," + rightHandRotationsUserSpace[ite].y + ",";
                        csvContent += headsetPositionsUserSpace[ite].z + ","+ -headsetPositionsUserSpace[ite].x + "," + headsetPositionsUserSpace[ite].y + "," + headsetRotationsUserSpace[ite].z + "," + -headsetRotationsUserSpace[ite].x + "," + headsetRotationsUserSpace[ite].y + "\n";
                        ite= ite +1;
                    }
                if (type == "validation") ite = 0;

                foreach (Vector3 point in leftHandPositionsUserSpace)
                    {
                        Debug.Log("LeftSide : ite " + ite );
                        csvContent += "Left," + point.z + "," + -point.x + "," + point.y + "," ;
                        if (type == "validation") csvContent += leftHandRotationsUserSpace[ite].z + "," + -leftHandRotationsUserSpace[ite].x + "," + leftHandRotationsUserSpace[ite].y + ",";
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

        public static (double, double, double, double, Vector3, Vector3) EllipsoidFitAleksander(List<Vector3> points)
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
        public static (double, double, double, double, Vector3, Vector3) RansacEllipsoidFit(List<Vector3> points)
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

            return bestModel;
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
