using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class UserMobilityFakeMovement : MonoBehaviour
    {
        private UserMobilityInput userMobilityInput;
        private RobotStatus robotStatus;

        public float maxSpeed;
        public float sensitivity;

        private float speed = 0.0f;
        private float rotationAngle = 0.0f;
        private bool wasMoving;
        private int queueSize = 5;
        private Queue<float> previousTranslationSpeedQueue = new Queue<float>();
        private Queue<float> previousRotationAngleQueue = new Queue<float>();
        Vector3 directional_vector;

        private float counterFakeMovement;

        public UnityEvent event_OnStartMoving;
        public UnityEvent event_OnStopMoving;

        private bool ClickMode;
        private bool NavigationMode;
        private bool simulateFakeConstantMovement;
        private bool simulateFakeStaticMovement;

        private TeleoperationManager teleoperationManager;

        private void OnEnable()
        {
            EventManager.StartListening(EventNames.TeleoperationSceneLoaded, Init);
            directional_vector = transform.TransformDirection(new Vector3(0.5f, 0, 0.5f));
            wasMoving = false;
            ReinitCounter();
        }

        private void Init()
        {
            userMobilityInput = UserInputManager.Instance.UserMobilityInput;
            robotStatus = RobotDataManager.Instance.RobotStatus;
            
            teleoperationManager = TeleoperationManager.Instance;
            EventManager.StartListening(EventNames.OnStopTeleoperation, StopFakeMovements);

            simulateFakeConstantMovement = false;
            simulateFakeStaticMovement = true;
        }

        private void ReinitCounter()
        {
            counterFakeMovement = 100;
            previousTranslationSpeedQueue.Clear();
            previousRotationAngleQueue.Clear();
        }

        private float GetQueueMean(Queue<float> queue)
        {
            float sum = 0;
            foreach (float value in queue)
            {
                sum += value;
            }
            sum /= queue.Count;
            return sum;
        }

        private void AddToQueue(Queue<float> queue, float element)
        {
            if (queue.Count == queueSize)
            {
                queue.Dequeue();
            }
            queue.Enqueue(element);
        }

        public bool IsMoving()
        {
            return wasMoving;
        }

        void Update()
        {
            if (robotStatus != null && ((teleoperationManager.IsMobileBaseTeleoperationActive && robotStatus.IsMobileBaseOn()) || (teleoperationManager.IsRobotTeleoperationActive && ClickMode)) && !robotStatus.AreRobotMovementsSuspended())
            {
                Vector2 direction = userMobilityInput.GetMobileBaseDirection();
                Vector2 mobileBaseRotation = userMobilityInput.GetAngleDirection();
                speed = Mathf.Sqrt(Mathf.Pow(direction[0], 2.0f) + Mathf.Pow(direction[1], 2.0f)) * sensitivity;
                speed = Mathf.Clamp(speed, 0, maxSpeed);
                rotationAngle = Mathf.Sqrt(Mathf.Pow(mobileBaseRotation[0], 2.0f)) * sensitivity;

                if (speed == 0 && rotationAngle == 0)
                {
                    if (wasMoving)
                    {
                        counterFakeMovement -= 1.0f;
                        speed = GetQueueMean(previousTranslationSpeedQueue);
                        rotationAngle = GetQueueMean(previousRotationAngleQueue);
                    }
                    if (counterFakeMovement == 0)
                    {
                        event_OnStopMoving.Invoke();
                        wasMoving = false;
                        ReinitCounter();
                    }
                }
                else
                {
                    if (!wasMoving) event_OnStartMoving.Invoke();
                    wasMoving = true;
                    AddToQueue(previousTranslationSpeedQueue, speed);
                    AddToQueue(previousRotationAngleQueue, rotationAngle);
                }

                if (ClickMode)
                {
                    if(simulateFakeConstantMovement)
                    {
                        speed = maxSpeed;
                        rotationAngle = sensitivity;
                    }

                    else if(simulateFakeStaticMovement)
                    {
                        speed = 0;
                        rotationAngle = 0;
                    }
                }

                transform.position += speed * Time.deltaTime * Vector3.ProjectOnPlane(directional_vector, Vector3.up);
                transform.Rotate(Vector3.up, rotationAngle);
            }
        }

        public void SetNavigationMode(bool isNavigationMode)
        {
            NavigationMode = isNavigationMode;
        }

        public void SetClickMode(bool isClickMode)
        {
            ClickMode = isClickMode;
        }

        public void AskForFakeConstantMovement(bool wantConstantMovement)
        {
            simulateFakeConstantMovement = wantConstantMovement;
        }

        public void AskForFakeStaticMovement(bool wantStaticMovement)
        {
            simulateFakeStaticMovement = wantStaticMovement;
        }

        void StopFakeMovements()
        {
            simulateFakeConstantMovement = false;
            simulateFakeStaticMovement = false;
        }
    }
}