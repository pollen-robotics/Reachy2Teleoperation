
namespace TeleopReachy
{
    public class UserInputManager : Singleton<UserInputManager>
    {
        public UserMovementsInput UserMovementsInput { get; private set; }
        public UserEmotionInput UserEmotionInput { get; private set; }
        public UserMobilityInput UserMobilityInput { get; private set; }
        public UserMobilityFakeMovement UserMobilityFakeMovement { get; private set; }
        public UserEmergencyStopInput UserEmergencyStopInput { get; private set; }

        protected override void Init()
        {
            UserMovementsInput = GetComponent<UserMovementsInput>();
            UserEmotionInput = GetComponent<UserEmotionInput>();
            UserMobilityInput = GetComponent<UserMobilityInput>();
            UserMobilityFakeMovement = GetComponent<UserMobilityFakeMovement>();
            UserEmergencyStopInput = GetComponent<UserEmergencyStopInput>();
        }
    }
}

