﻿using System;
using System.Net;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;

class Program
{
    static async Task Main(string[] args)
    {
        // Print the IP address of the host
        string ipAddress = GetLocalIPAddress();
        Console.WriteLine($"Host IP Address: {ipAddress}");

        // Specify the port number
        int port = 1883;
        Console.WriteLine($"Port: {port}");

        var optionsBuilder = new MqttServerOptionsBuilder()
            .WithConnectionValidator(c =>
            {
                // Accept all connections
                c.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.Success;
            })
            .WithApplicationMessageInterceptor(context =>
            {
                Console.WriteLine($"Client {context.ClientId} published message:");
                Console.WriteLine($"Topic: {context.ApplicationMessage.Topic}");
                Console.WriteLine($"Payload: {context.ApplicationMessage.ConvertPayloadToString()}");
            });

        var mqttServer = new MqttFactory().CreateMqttServer();

        mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
        {
            Console.WriteLine($"Client connected: {e.ClientId}");
        });

        mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
        {
            Console.WriteLine($"Client disconnected: {e.ClientId}");
        });

        try
        {
            await mqttServer.StartAsync(optionsBuilder.Build());
            Console.WriteLine("MQTT Broker started. Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            await mqttServer.StopAsync();
        }
    }

    static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "No IP Address Found";
    }
}
