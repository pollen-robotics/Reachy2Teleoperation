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
        private Transform userTrackerTransform;

        private Vector3 lastPointLeft;
        private Vector3 lastPointRight;

        private List<Vector3> leftCoordinates = new List<Vector3>();
        private List<Vector3> rightCoordinates = new List<Vector3>();
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
        public UnityEvent event_OnCalibChanged;
        public UnityEvent event_WaitForCalib;
        public UnityEvent event_StartRightCalib;
        public UnityEvent event_StartLeftCalib;





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
                event_OnCalibChanged.Invoke();
                Debug.Log("Avant la fonction d'update : rightarmsize =" + rightArmSize + " leftarmsize =" + leftArmSize);
                UserSize.Instance.UpdateUserSizeafterCalibration_differentarms(leftArmSize, rightArmSize, shoulderWidth);
                ExportCoordinatesToCSV(); 
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
                        Debug.Log(leftCoordinates.Count);
                        leftCoordinates.Add(trackedLeftHand.position);
                        lastPointLeft = trackedLeftHand.position;
                        actualTime=0f;}
                } else  {
                    calib_left_side = true;
                }
            }
            
        }



        public void UpperBodyFeatures()
        {
            Vector3 initialPosition = TransitionRoomManager.Instance.userTracker.position;
 
            (double calculatedLeftArmSize, double approxleftarmsizex, double approxleftarmsizey, Vector3 leftRadii, Vector3 leftShoulderCenter) = RansacEllipsoidFit(leftCoordinates);
            (double calculatedRightArmSize, double approxrightarmsizex, double approxrightarmsizey, Vector3 rightRadii, Vector3 rightShoulderCenter) = RansacEllipsoidFit(rightCoordinates);
            Debug.Log("LSM des deux côtés ok");

            leftArmSize = calculatedLeftArmSize;
            rightArmSize = calculatedRightArmSize;
            meanArmSize = (leftArmSize + rightArmSize) / 2f;

            Vector3 midShoulderPoint = (leftShoulderCenter + rightShoulderCenter) / 2f;
            TransitionRoomManager.Instance.midShoulderPoint = midShoulderPoint;
            Debug.Log("Epaule G : " + leftShoulderCenter +"/ Epaule D : " + rightShoulderCenter + "/Milieu Epaule  : " + midShoulderPoint +", Taille moyenne des bras : " + meanArmSize);

            shoulderWidth = Vector3.Distance(leftShoulderCenter, rightShoulderCenter)/2f;

            //ajout des data dans un .csv
            var filePath = @"C:\Users\robot\Dev\dataunity_exhaustif.csv";
            var currentTime = DateTime.Now.ToString("ddMM_HHmm", CultureInfo.InvariantCulture);
            var data = $"{currentTime},{leftArmSize},{leftShoulderCenter.x},{leftShoulderCenter.y},{leftShoulderCenter.z},{rightArmSize},{rightShoulderCenter.x},{rightShoulderCenter.y},{rightShoulderCenter.z},{shoulderWidth},{meanArmSize},{midShoulderPoint.x},{midShoulderPoint.y},{midShoulderPoint.z},";
            using (var writer = new StreamWriter(filePath, true))
            {writer.WriteLine(data);}


            filePath = @"C:\Users\robot\Dev\armsize.csv";
            data = $"{currentTime},{leftArmSize},{approxleftarmsizex},{approxleftarmsizey},\"{leftRadii.x},{leftRadii.y},{leftRadii.z}\",{rightArmSize},{approxrightarmsizex},{approxrightarmsizey},\"{rightRadii.x},{rightRadii.y},{rightRadii.z}\",{meanArmSize}";
            using (var writer = new StreamWriter(filePath, true))
            {writer.WriteLine(data);}

            return;
        }


        public bool IsCalibrated (){
            return calibration_done;
        }

        public void ExportCoordinatesToCSV()
        {
            userTrackerTransform = GameObject.Find("UserTracker").transform;
            Matrix4x4 userTrackerInverseTransform = userTrackerTransform.worldToLocalMatrix;

            string dateTimeString = DateTime.Now.ToString("ddMM_HHmm");
            string fileName = "DataCalib_" + dateTimeString + ".csv";
            List<Vector3> rightHandPositionsUserSpace = new List<Vector3>();
            List<Vector3> leftHandPositionsUserSpace = new List<Vector3>();

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

            using (FileStream fs = File.Create(Path.Combine(@"C:\Users\robot\Dev\data\Tests\SimonAP", fileName)))
            {
                string csvContent = "Side,X,Y,Z\n";
                foreach (Vector3 point in leftHandPositionsUserSpace)
                    csvContent += "Left," + point.z + "," + -point.x + "," + point.y + "\n";

                foreach (Vector3 point in rightHandPositionsUserSpace)
                    csvContent += "Right," + point.z + "," + -point.x + "," + point.y + "\n";

                byte[] csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                fs.Write(csvBytes, 0, csvBytes.Length);

            }

            Debug.Log("Coordonnées exportées dans fichier CSV : " + fileName);
        }

        public static (double, double, double, Vector3, Vector3) EllipsoidFitAleksander(List<Vector3> points)
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

            // Calcul de approxRadiusx
            double approxRadiusx = filteredPointsx.Any() ? distancesx.Average() : 0.0;

            // Calcul de approxRadiusy
            double approxRadiusy = filteredPointsy.Any() ? distancesy.Average() : 0.0;            

            return (meanRadius, approxRadiusx, approxRadiusy, radiiVector3, centreVector3);

        }

        //ajout d'un algorithme de RANSAC qui utilise la fonction EllipsoidFitAleksander avec des échantillons de points aléatoires pour la détection de l'épaule
        public static (double, double, double, Vector3, Vector3) RansacEllipsoidFit(List<Vector3> points)
        {
            int n = points.Count;
            int maxIterations = 500;
            int sampleSize = 100;
            double threshold = 0.1;
            double bestError = double.MaxValue;
            (double, double, double, Vector3, Vector3) bestModel = (0, 0, 0, Vector3.zero, Vector3.zero);

            for (int i = 0; i < maxIterations; i++)
            {
                List<Vector3> sample = new List<Vector3>();
                for (int j = 0; j < sampleSize; j++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, n);
                    sample.Add(points[randomIndex]);
                }

                (double, double, double, Vector3, Vector3) model = EllipsoidFitAleksander(sample);
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

    }
}