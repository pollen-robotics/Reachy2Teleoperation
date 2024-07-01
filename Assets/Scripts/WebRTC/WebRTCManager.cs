
using GstreamerWebRTC;


namespace TeleopReachy
{
    public class WebRTCManager : Singleton<WebRTCManager>
    {
        public ConnectionStatus ConnectionStatus { get; private set; }
        //public WebRTCData webRTCDataController { get; private set; }
        public GStreamerPluginCustom webRTCController { get; private set; }
        //public WebRTCAudioSender webRTCAudioSender { get; private set; }

        protected override void Init()
        {
            ConnectionStatus = GetComponent<ConnectionStatus>();
            //webRTCDataController = GetComponent<WebRTCData>();
            webRTCController = GetComponent<GStreamerPluginCustom>();
            //webRTCAudioSender = GetComponent<WebRTCAudioSender>();
        }
    }
}
