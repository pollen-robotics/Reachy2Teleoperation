namespace TeleopReachy
{
    public class RobotDataManager : Singleton<RobotDataManager>
    {
        public RobotJointCommands RobotJointCommands { get; private set; }
        public RobotJointState RobotJointState { get; private set; }
        public RobotVideoStream RobotVideoStream { get; private set; }
        public RobotStatus RobotStatus { get; private set; }
        public RobotConfig RobotConfig { get; private set; }
        public RobotMobilityCommands RobotMobilityCommands { get; private set; }
        public RobotPingWatcher RobotPingWatcher { get; private set; }
        public RobotErrorManager RobotErrorManager { get; private set; }
        public RobotReachabilityManager RobotReachabilityManager { get; private set; }

        protected override void Init()
        {
            RobotJointCommands = GetComponent<RobotJointCommands>();
            RobotJointState = GetComponent<RobotJointState>();
            RobotVideoStream = GetComponent<RobotVideoStream>();
            RobotStatus = GetComponent<RobotStatus>();
            RobotMobilityCommands = GetComponent<RobotMobilityCommands>();
            RobotConfig = GetComponent<RobotConfig>();
            RobotPingWatcher = GetComponent<RobotPingWatcher>();
            RobotErrorManager = GetComponent<RobotErrorManager>();
            RobotReachabilityManager = GetComponent<RobotReachabilityManager>();
        }
    }
}
