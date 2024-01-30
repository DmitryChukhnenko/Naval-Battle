using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Client {
    public class TCP {
        public static async Task SendInt32(TcpClient client, int number) =>
            await client.GetStream().WriteAsync(BitConverter.GetBytes(number));

        public static async Task SendWithLength (TcpClient client, byte[] buffer) {
            await SendInt32(client, buffer.Length);
            await client.GetStream().WriteAsync(buffer);
        }

        public static async Task SendString(TcpClient client, string text) =>
            await SendWithLength(client, Encoding.UTF8.GetBytes(text));

        public static async Task SendBinaryWriter (TcpClient client, Action<BinaryWriter> write) {
            MemoryStream memory = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memory);
            write(writer);
            await SendWithLength(client, memory.ToArray());
        }

        public static async Task SendFile (TcpClient client, Stream file) {
            int length = (int)file.Length;
            await SendInt32(client, length);

            byte[] buffer = new byte[1024];
            int pos = 0;
            while (pos < length) {
                int read = await file.ReadAsync(buffer, 0,
                    Math.Min(buffer.Length, length - pos));
                await client.GetStream().WriteAsync(buffer, 0, read);
                //Console.WriteLine(read);
                pos += read;
            }
        }

        public static async Task<byte[]> ReceiveFixed(TcpClient client, int length) {
            byte[] buffer = new byte[length];
            await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
            return buffer;
        }

        public static async Task<int> ReceiveInt32 (TcpClient client) =>
            BitConverter.ToInt32(await ReceiveFixed(client, sizeof(int)));

        public static async Task<byte[]> ReceiveVariable (TcpClient client) =>
            await ReceiveFixed(client, await ReceiveInt32(client));

        public static async Task<string> ReceiveString (TcpClient client) =>
            Encoding.UTF8.GetString(await ReceiveVariable(client));

        public static async Task ReceiveFile (TcpClient client, Stream file) {
            int length = await ReceiveInt32 (client);
            int pos = 0;
            byte[] buffer = new byte[1024];
            while (pos < length) {
                int read = await client.GetStream().ReadAsync(buffer, 0,
                    Math.Min(buffer.Length, length - pos));
                await file.WriteAsync (buffer, 0, read);
                pos += read;
            }
        }
    }
}