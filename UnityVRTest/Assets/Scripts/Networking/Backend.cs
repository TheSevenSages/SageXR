using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;

public class Backend : MonoBehaviour
{
    [SerializeField]
    string ip = "127.0.0.1";
    [SerializeField]
    string port = "8080";

    private HubConnection connection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        // Build the connection
        connection = new HubConnectionBuilder()
            .WithUrl($"http://{ip}:{port}/connectionHub")
            .Build();

        await connection.StartAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
