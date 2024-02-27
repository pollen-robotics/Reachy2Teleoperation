using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using System.Threading.Tasks;
using System;

using UnityEngine.UI;
using Unity.WebRTC;

using System.Linq;

namespace TeleopReachy
{
    [RequireComponent(typeof(AudioSource))]
    public class WebRTCAudioSender : WebRTCBase
    {
        [SerializeField] private AudioSource inputAudioSource;

        private MediaStream _sendStream;

        private AudioStreamTrack m_audioTrack;

        private RTCRtpSender _sender = null;


        public UnityEvent<bool> event_AudioSenderStatusHasChanged;
        private bool isRobotInRoom = false;


        protected override void Start()
        {
            base.Start();
            inputAudioSource = GetComponent<AudioSource>();

            UserMicrophoneInput microphoneInput = UserMicrophoneInput.Instance;
            AudioClip clipInput = microphoneInput.GetMicrophoneInput();
            inputAudioSource.loop = true;
            inputAudioSource.clip = clipInput;
            inputAudioSource.Play();

            m_audioTrack = new AudioStreamTrack(inputAudioSource);
            m_audioTrack.Loopback = false;

            // Task.Run(() => AudioStart());

        }

        // IEnumerator MicroStart()
        // {
        //     yield return null;
        //     yield return new WaitForSeconds(2);


        // }

        // protected void AudioStart()
        // {

        // }


        protected override void WebRTCCall()
        {
            Debug.Log("[WebRTCAudioSender] Call started");
            base.WebRTCCall();
            _sendStream = new MediaStream();

            if (_pc != null)
            {
                _pc.OnNegotiationNeeded = () =>
                {
                    Debug.Log($"[WebRTCAudioSender] OnNegotiationNeeded");
                    StartCoroutine(PeerNegotiationNeeded(_pc));
                };

                _sender = _pc.AddTrack(m_audioTrack, _sendStream);

                Debug.Log(m_audioTrack.ReadyState);

                var codecs = RTCRtpSender.GetCapabilities(TrackKind.Audio).codecs;

                var excludeCodecTypes = new[] { "audio/CN", "audio/telephone-event" };

                List<RTCRtpCodecCapability> availableCodecs = new List<RTCRtpCodecCapability>();
                foreach (var codec in codecs)
                {
                    if (excludeCodecTypes.Count(type => codec.mimeType.Contains(type)) > 0)
                        continue;
                    /*if (codec.mimeType.Contains("audio/opus"))
                    {
                        //to force opus
                        availableCodecs.Add(codec);
                    }*/
                }


                var transceiver1 = _pc.GetTransceivers().First();
                Debug.Log("codec " + transceiver1 + " " + availableCodecs);
                var error = transceiver1.SetCodecPreferences(availableCodecs.ToArray());
                if (error != RTCErrorType.None)
                    Debug.LogError(error);
                //var transceiver1 = _pc.GetTransceivers().First();
                transceiver1.Direction = RTCRtpTransceiverDirection.SendOnly;

                isRobotInRoom = true;
                event_AudioSenderStatusHasChanged.Invoke(isRobotInRoom);
            }
        }

        protected override void OnDestroy()
        {
            Task.Run(() => DisposeAll());

            base.OnDestroy();
        }

        protected void DisposeAll()
        {
            inputAudioSource.Stop();

            if (_pc != null && _sender != null)
            {
                _pc.RemoveTrack(_sender);
            }
            m_audioTrack?.Dispose();
            _sendStream?.Dispose();
        }
    }
}
