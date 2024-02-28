using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using UnityEngine.Events;

namespace TeleopReachy
{
    public class WebRTCAVReceiver : WebRTCBase
    {
        //public Renderer screen;
        public RawImage screen;
        public AudioSource outputAudioSource;
        private MediaStream _receiveStream;

        private string right_track_id_name = "";

        public UnityEvent<bool> event_OnVideoRoomStatusHasChanged;
        public UnityEvent<bool> event_OnAudioReceiverRoomStatusHasChanged;
        public UnityEvent<Texture> event_OnVideoTextureReceived;

        protected override void Start()
        {
            StartCoroutine(WebRTC.Update());
            base.Start();
        }

        protected override void WebRTCCall()
        {
            base.WebRTCCall();
            right_track_id_name = "";
            _receiveStream = new MediaStream();
            _receiveStream.OnAddTrack += OnAddTrack;
            if (_pc != null)
            {
                _pc.OnTrack = (evt) =>
                {
                    Debug.Log($"[WebRTC] OnTrack {evt.Track} id: {evt.Track.Id}");

                    if (evt.Track.Kind == TrackKind.Video)
                    {
                        //right track is received before left
                        if (right_track_id_name == "")
                            right_track_id_name = evt.Track.Id;
                        _receiveStream.AddTrack(evt.Track);
                        if (screen == null)
                            Debug.LogError("Screen is not assigned. Image won't be rendered");
                        event_OnVideoRoomStatusHasChanged.Invoke(true);
                    }
                    else if (evt.Track.Kind == TrackKind.Audio)
                    {
                        _receiveStream.AddTrack(evt.Track);
                        if (outputAudioSource == null)
                            Debug.LogWarning("Output audio is not assigned. Sound won't be rendered");
                        event_OnAudioReceiverRoomStatusHasChanged.Invoke(true);
                    }
                };
                var transceiver = _pc.AddTransceiver(TrackKind.Video);
                transceiver.Direction = RTCRtpTransceiverDirection.RecvOnly;
            }
        }

        void OnAddTrack(MediaStreamTrackEvent e)
        {
            if (e.Track is VideoStreamTrack video)
            {
                video.OnVideoReceived += tex =>
                {
                    if (e.Track.Id == right_track_id_name)
                    {
                        screen.material.SetTexture("_MainTex_right", tex);
                        event_OnVideoTextureReceived.Invoke(tex);
                    }
                    else
                    {
                        screen.texture = tex;
                        screen.material.SetTexture("_MainTex", tex);
                    }
                };
            }
            else if (e.Track is AudioStreamTrack audio)
            {
                Debug.Log("playing audio track");
                outputAudioSource.Stop();
                outputAudioSource.SetTrack(audio);
                outputAudioSource.loop = true;
                outputAudioSource.Play();
            }
        }
    }
}
