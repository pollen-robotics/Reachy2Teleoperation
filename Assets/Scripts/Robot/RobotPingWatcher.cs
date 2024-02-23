using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TeleopReachy
{
    public class RobotPingWatcher : MonoBehaviour
    {
        private const int QUEUE_SIZE = 10;

        private const float REFRESH_REQ_SEC = 1;

        public const int THRESHOLD_LOW_QUALITY_PING = 70;

        private float mean_ping = 0;

        private bool isUnstable = false;

        private bool has_received_ping = false;
        private bool pingable = true;

        Coroutine pingCheck = null;

        // Start is called before the first frame update
        void Start()
        {
            //UnityEngine.Ping always return -1 on android
#if !UNITY_ANDROID
            string robot_ip = PlayerPrefs.GetString("robot_ip");
            if (robot_ip != "localhost" && robot_ip != Robot.VIRTUAL_ROBOT_IP)
            {
                pingCheck = StartCoroutine(MeanPing(robot_ip));
                StartCoroutine(WaitForFirstPing());
            }
#endif
        }

        void OnDestroy()
        {
            if (pingCheck != null)
                StopCoroutine(pingCheck);
        }

        IEnumerator WaitForFirstPing()
        {
            yield return new WaitForSeconds(1);
            if(!has_received_ping) pingable = false;
        }

        IEnumerator MeanPing(string ip)
        {
            Queue<int> lastPingTimes = new Queue<int>(QUEUE_SIZE);
            while (true)
            {
                Ping p = new Ping(ip);
                isUnstable = false;
                yield return new WaitForSeconds(REFRESH_REQ_SEC);

                yield return new WaitUntil(() => p.isDone);
                has_received_ping = true;
                pingable = true;

                if (p.time > -1)
                {
                    lastPingTimes.Enqueue(p.time);
                }
                else
                {
                    lastPingTimes.Enqueue((int)REFRESH_REQ_SEC * 1000);
                    isUnstable = true;
                }

                if (lastPingTimes.Count > QUEUE_SIZE) lastPingTimes.Dequeue();

                float mean = 0;
                foreach (int obj in lastPingTimes)
                {
                    mean += obj;
                }
                mean_ping = mean / lastPingTimes.Count;
            }
        }

        public float GetPing()
        {
            if(pingable) return mean_ping;
            else return -1;
        }

        public bool GetIsUnstablePing()
        {
            return isUnstable;
        }
    }
}