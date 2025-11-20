using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;

public class Backend : MonoBehaviour
{
    [Header("Settings")]
    public string deviceName = "DEVICE_NAME";
    public bool isHost = false;

    [Header("API Location")]
    [SerializeField]
    string ip = "127.0.0.1";
    [SerializeField]
    string port = "8080";

    private HubConnection connection;
   
    async void Start()
    {
        // Build the connection
        connection = new HubConnectionBuilder()
            .WithUrl($"http://{ip}:{port}/connectionHub")
            .Build();

        await connection.StartAsync();
        await connection.SendAsync("InitDevice", deviceName, isHost);
    }

    public void GetMessage(string message)
    {
        Debug.Log(message);
    }
}
