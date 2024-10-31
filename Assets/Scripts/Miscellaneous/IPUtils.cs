using System.Net;
using System;
using UnityEngine;

namespace TeleopReachy
{
    public class IPUtils
    {
        //from https://learn.microsoft.com/en-us/dotnet/api/system.net.ipaddress.tryparse?view=net-8.0
        public static bool IsIPValid(string ipAddress)
        {
            try
            {
                IPAddress address;
                if (ipAddress == "localhost" || ipAddress == Robot.VIRTUAL_ROBOT_IP)
                    return true;
                if (ipAddress.EndsWith(".local"))
                {
                    IPAddress[] ipAddresses = Dns.GetHostAddresses(ipAddress);
                    foreach (IPAddress ip in ipAddresses)
                    {
                        // Check if the IP address is an IPv4 address
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            address = ip;
                        }
                    }
                }
                else 
                {
                    // Create an instance of IPAddress for the specified address string (in
                    // dotted-quad, or colon-hexadecimal notation).
                    address = IPAddress.Parse(ipAddress);
                }
                return true;
            }

            catch (ArgumentNullException e)
            {
                Debug.LogWarning("IP is invalid : " + e.Message);
                return false;
            }

            catch (FormatException e)
            {
                Debug.LogWarning("IP is invalid : " + e.Message);
                return false;
            }

            catch (Exception e)
            {
                Debug.LogWarning("IP is invalid : " + e.Message);
                return false;
            }
        }
    }
}
