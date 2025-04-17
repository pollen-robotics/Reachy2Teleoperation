namespace TeleopReachy
{
    public class ActiveControllerManager : Singleton<ActiveControllerManager>
    {
        public ControllersManager ControllersManager { get; private set; }
        public ControllersVibrations ControllersVibrations { get; private set; }

        protected override void Init()
        {
            ControllersManager = ControllersManager.Instance;
            ControllersVibrations = GetComponent<ControllersVibrations>();
        }
    }
}


