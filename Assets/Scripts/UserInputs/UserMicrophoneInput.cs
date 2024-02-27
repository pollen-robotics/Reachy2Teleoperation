using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TeleopReachy
{
    public class UserMicrophoneInput : Singleton<UserMicrophoneInput>
    {
        private AudioClip m_clipInput;
        const int sampleRate = 48000;
        string _deviceName;

        // Start is called before the first frame update
        void Start()
        {
            _deviceName = Microphone.devices[0];
            Debug.Log("Microphone: " + _deviceName);
            Microphone.GetDeviceCaps(_deviceName, out int minFreq, out int maxFreq);
            // StartCoroutine(MicroStart());

            int m_lengthSeconds = 1;
            m_clipInput = Microphone.Start(_deviceName, true, m_lengthSeconds, sampleRate);
            // yield return null;

            while (!(Microphone.GetPosition(_deviceName) > 0)) { }
        }

        // Update is called once per frame
        public AudioClip GetMicrophoneInput()
        {
            return m_clipInput;
        }

        void OnDestroy()
        {
            Microphone.End(_deviceName);
        }
    }
}