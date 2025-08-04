using UnityEngine;
using System.Collections;
using Grpc.Core;


namespace DataAcquisition
{
    public class gRPCBase : MonoBehaviour
    {
        protected Channel channel = null;

        public string rpcException;

        protected string ip_address;

        protected void InitChannel(string port)
        {
            ip_address = PlayerPrefs.GetString("robot_ip");
            string address = ip_address + ":" + PlayerPrefs.GetString(port);
            channel = new Channel(address, ChannelCredentials.Insecure);
        }

        // To remove later
        protected void InitCustomChannel(string ip_address, string port)
        {
            string address = ip_address + ":" + port;
            channel = new Channel(address, ChannelCredentials.Insecure);
        }

        protected virtual void RecoverFromNetWorkIssue()
        {

        }

        protected virtual void NotifyDisconnection()
        {

        }
    }
}