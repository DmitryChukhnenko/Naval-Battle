using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Client {
    public static class TCP {
        // Все приватные методы — только для внутреннего использования
        private static async Task SendInt32(this TcpClient client, int number) =>
            await client.GetStream().WriteAsync(BitConverter.GetBytes(number));

        private static async Task SendVariable(this TcpClient client, byte[] buffer) {
            await SendInt32(client, buffer.Length);
            await client.GetStream().WriteAsync(buffer);
        }

        private static async Task SendString(this TcpClient client, string text) =>
            await SendVariable(client, Encoding.UTF8.GetBytes(text));

        // Отправлять можно только этой функцией
        public static async Task SendMessage(this TcpClient target, MessageType messageType, params object[] args) {
            Message message = new Message(messageType, args);
            string json = JsonSerializer.Serialize(message, new JsonSerializerOptions {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(
                    UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
            });
            await SendString(target, json);
        }

        // Все приватные методы — только для внутреннего использования
        private static async Task<byte[]> ReceiveFixed(this TcpClient client, int length) {
            byte[] buffer = new byte[length];
            await client.GetStream().ReadExactlyAsync(buffer, 0, buffer.Length);
            return buffer;
        }

        private static async Task<int> ReceiveInt32(this TcpClient client) =>
            BitConverter.ToInt32(await ReceiveFixed(client, sizeof(int)));

        private static async Task<byte[]> ReceiveVariable(this TcpClient client) =>
            await ReceiveFixed(client, await ReceiveInt32(client));

        private static async Task<string> ReceiveString(this TcpClient client) =>
            Encoding.UTF8.GetString(await ReceiveVariable(client));

        // Принимать сообщения можно только этой функцией
        public static async Task<Message> ReceiveMessage(this TcpClient client) {
            string json = await client.ReceiveString();
            Message message = JsonSerializer.Deserialize<Message>(json, new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
            }) ?? throw new NullReferenceException();
            return message;
        }
    }
}