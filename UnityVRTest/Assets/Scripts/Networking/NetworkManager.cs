using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public bool isHost = false;
    [Tooltip("Name of the device, limited to 12 characters")]
    public string deviceName = "DEVICE_NAME";

    Socket socket = null;
    bool isConnected = false;

    int udpStartPort = 50000; // The first port that the guest's UDP listener will try to bind to.
    int udpListenerPorts = 5; // The number of total ports (including the starting port) that the listener may try to bind to

    private void Start()
    {
        Debug.Log(GetSubnetMask());
        if (isHost)
        {
            SetupHost();
        }
        else
        {
            SetupGuest();
        }
    }

    // --------------------------------------------------------
    // HOST

    // Create a socket then listen on a port for any connection requests
    private async void SetupHost()
    {
        if (socket != null) { throw new Exception("Socket has already been set"); }

        // Creates a new socket to listen for any incoming connections
        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //try
        //{
            // Bind our socket to a port
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 50000);
            listener.Bind(localEP);

            IPAddress hostIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            int hostPort = ((IPEndPoint)listener.LocalEndPoint).Port;
            Debug.Log("My local IpAddress is :" + hostIP + " I am connected on port number " + hostPort);

            listener.Listen(5);

            socket = await listener.AcceptAsync();
            Debug.Log("CONNECTED");
            byte[] message = null;
            message = Encoding.ASCII.GetBytes("Hello World!");
            int bytesSent = 0;
            while (bytesSent < message.Length)
            {
                bytesSent += socket.Send(message, bytesSent, message.Length - bytesSent, SocketFlags.None);
            }
            //Continuously broadcast invites on the LAN
            //StartCoroutine(SendInvite(deviceName, hostIP, hostPort));
        //}
        //finally
        //{
        //    listener.Close();
        //}
    }

    // Send out an invite to every device on the local area network (LAN).
    // Invite contains: hostName, hostIP, hostPort
    private IEnumerator SendInvite(string hostName, IPAddress hostIP, int hostPort)
    {
        UdpClient udpBroadcaster = new UdpClient();
        udpBroadcaster.EnableBroadcast = true;

        byte[] message = null;
        message = Encoding.ASCII.GetBytes("Hello World!");
        // Name size - 12 bytes
        // IP size - 4 bytes
        // Port size - 2 bytes

        while (!isConnected)
        {
            Debug.Log("SENDING INVITES");

            for (int b = 0; b <= 255; b++)
            {
                byte[] ipBytes = hostIP.GetAddressBytes();
                ipBytes[3] = Convert.ToByte(b);
                var ip = new IPAddress(ipBytes);

                for (int i = 0; i < udpListenerPorts; i++)
                {
                    IPEndPoint broadcastEP = new IPEndPoint(ip, udpStartPort + i);

                    udpBroadcaster.SendAsync(message, message.Length, broadcastEP);
                }
            }

            yield return new WaitForSecondsRealtime(2.0f);
        }

        udpBroadcaster.Close();
    }

    // --------------------------------------------------------
    // GUEST

    // Create a client and then connect to the server port
    private void SetupGuest()
    {
        if (socket != null) { throw new Exception("Socket has already been set"); }

        UdpClient udpListener = null;
        for (int i = 0; i < udpListenerPorts; i++)
        {
            int port = udpStartPort + i;
            
            try
            {
                // Try to bind to this port
                udpListener = new UdpClient(port);

                Debug.Log($"[UDP] Successfully bound to port {port}");

                udpListener.BeginReceive(new AsyncCallback(ReceiveInvite), null);
                break;
            }
            catch (SocketException)
            {
                Debug.LogWarning($"[UDP] Port {port} is busy. Trying next...");
            }
        }
        if (udpListener == null) { throw new Exception($"[UDP] No open port in range [{udpStartPort}, {udpStartPort + udpListenerPorts})"); }
    }

    // Called when a UDP packet is recieved, we check if its an invite then handle the invitation accordingly
    private void ReceiveInvite(IAsyncResult ar)
    {

    }

    // --------------------------------------------------------
    // SHARED

    // S

    // Written by Google Gemini, replace if causing issues
    public IPAddress GetSubnetMask()
    {
        // 1. Get all network interfaces on the machine
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface adapter in interfaces)
        {
            // 2. Filter for active, operational interfaces (e.g., Ethernet or Wi-Fi)
            if (adapter.OperationalStatus == OperationalStatus.Up &&
                (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                 adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
            {
                // 3. Get the IP properties for the adapter
                IPInterfaceProperties properties = adapter.GetIPProperties();

                // 4. Look through all IP addresses associated with this adapter
                foreach (UnicastIPAddressInformation ipInfo in properties.UnicastAddresses)
                {
                    // 5. Filter for IPv4 addresses
                    if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Found the IPv4 subnet mask
                        return ipInfo.IPv4Mask;
                    }
                }
            }
        }

        // Return a default or null if no active IPv4 adapter is found
        Debug.LogError("Could not find an active IPv4 subnet mask.");
        return null;
    }

    // Releases the socket's resources
    private void OnDestroy()
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        finally
        {
            socket.Close();
        }
    }
}
