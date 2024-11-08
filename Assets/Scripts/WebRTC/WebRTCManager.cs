
using GstreamerWebRTC;


namespace TeleopReachy
{
    public class WebRTCManager : Singleton<WebRTCManager>
    {
        public GStreamerPluginCustom gstreamerPlugin { get; private set; }

        protected override void Init()
        {
            gstreamerPlugin = GetComponent<GStreamerPluginCustom>();
        }
    }
}
