using System.Net;
using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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
                if (ipAddress == "localhost" || ipAddress == Robot.VIRTUAL_ROBOT_IP)
                    return true;

                else if ((Regex.IsMatch(ipAddress, @"^reachy2-\w+\.local$")))
                    return true;

                else 
                    return IsValidIPv4(ipAddress);
            }

            catch (ArgumentNullException e)
            {
                Debug.LogError("IP is invalid : " + e.Message);
                return false;
            }

            catch (FormatException e)
            {
                Debug.LogError("IP is invalid : " + e.Message);
                return false;
            }

            catch (Exception e)
            {
                Debug.LogError("IP is invalid : " + e.Message);
                return false;
            }
        }

        public static bool IsValidIPv4(string input)
        {
            return IPAddress.TryParse(input, out IPAddress ip)
            && ip.AddressFamily == AddressFamily.InterNetwork
            && ip.ToString() == input;
        }
    }
}
