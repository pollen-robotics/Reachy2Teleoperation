using UnityEngine;


namespace TeleopReachy
{
    public class UserMicrophoneInput : Singleton<UserMicrophoneInput>
    {
        private AudioClip m_clipInput;
        const int sampleRate = 48000;
        string _deviceName;

        void Start()
        {
            _deviceName = Microphone.devices[0];
            Debug.Log("Microphone: " + _deviceName);
            Microphone.GetDeviceCaps(_deviceName, out int minFreq, out int maxFreq);

            int m_lengthSeconds = 1;
            m_clipInput = Microphone.Start(_deviceName, true, m_lengthSeconds, sampleRate);
            while (!(Microphone.GetPosition(_deviceName) > 0)) { }
        }

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