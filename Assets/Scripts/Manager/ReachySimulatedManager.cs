
namespace TeleopReachy
{
    public class ReachySimulatedManager : Singleton<ReachySimulatedManager>
    {
        public ReachySimulatedCommands ReachySimulatedCommands { get; private set; }

        protected override void Init()
        {
            ReachySimulatedCommands = transform.GetChild(1).GetComponent<ReachySimulatedCommands>();
        }
    }
}
