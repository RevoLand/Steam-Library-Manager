using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Steam_Library_Manager.Functions
{
    class Network
    {
        public static void UpdatePublicIP()
        {
            try
            {
                Properties.Settings.Default.ListenIP = new WebClient().DownloadString("http://icanhazip.com").Replace("\n", "");
            }
            catch { }
        }

        public static int GetAvailablePort()
        {
            try
            {
                List<int> usedPorts = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();
                int unusedPort = 0;

                for (int port = 10000; port < 20000; port++)
                {
                    if (!usedPorts.Contains(port))
                    {
                        unusedPort = port;
                        break;
                    }
                }

                return unusedPort;
            }
            catch
            {
                return 19000;
            }
        }

        public static bool GetPortStatus(int port)
        {
            try
            {
                List<int> usedPorts = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();

                if (usedPorts.Contains(port))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
