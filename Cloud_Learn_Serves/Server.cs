using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    private TcpListener _listener;

    public Server(string ipAddress, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("Server started.");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            HandleClientAsync(client);
        }
    }

    private async void HandleClientAsync(TcpClient client)
    {
        Console.WriteLine("Client connected.");

        using (var networkStream = client.GetStream())
        {
            var buffer = new byte[5];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 5)
            {
                byte dataType = buffer[0];
                int dataLength = BitConverter.ToInt32(buffer, 1);

                var nameLengthBuffer = new byte[4];
                await networkStream.ReadAsync(nameLengthBuffer, 0, nameLengthBuffer.Length);
                int nameLength = BitConverter.ToInt32(nameLengthBuffer, 0);

                var nameBuffer = new byte[nameLength];
                await networkStream.ReadAsync(nameBuffer, 0, nameBuffer.Length);
                string imageName = Encoding.UTF8.GetString(nameBuffer);

                var dataBuffer = new byte[dataLength - nameLength - 4];
                bytesRead = await networkStream.ReadAsync(dataBuffer, 0, dataBuffer.Length);

                if (bytesRead == dataBuffer.Length && dataType == 1)
                {
                    File.WriteAllBytes(imageName, dataBuffer);
                    Console.WriteLine($"Image {imageName} received and saved.");
                }
            }
        }

        client.Close();
        Console.WriteLine("Client disconnected.");
    }

    static async Task Main(string[] args)
    {
        var server = new Server("127.0.0.1", 5000);
        await server.StartAsync();
    }
}
