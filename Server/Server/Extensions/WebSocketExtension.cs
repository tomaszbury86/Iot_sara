using Newtonsoft.Json;
using Server.Model;
using System.Net.WebSockets;
using System.Text;

namespace Server.Extensions
{
    public static class WebSocketExtension
    {
        private static byte[] _buffer = new byte[1024];
        private static WebSocketReceiveResult _webSocketRecieveResult;

        public static async Task Process(this WebSocket socket)
        {
            var request = await ReadSocetStream(socket);

            try
            {
                if (request.Command == Command.REGISTER_NODE_MCU)
                {
                    Socket.Add(new SocketModel(socket, SocketType.IOT));

                    await Router.TracePackage((new RequestModel
                    {
                        Command = Command.REGISTER_NODE_MCU
                    }));
                }
                else if (request.Command == Command.REGISTER_WEB_BROWSER)
                {
                    Socket.Add(new SocketModel(socket, SocketType.WEB_BROWSER));

                    if (Socket.OpenIotSockets().Any())
                    {
                        await Router.TracePackage((new RequestModel
                        {
                            Command = Command.REGISTER_NODE_MCU
                        }));

                    }
                }

                while (!_webSocketRecieveResult.CloseStatus.HasValue)
                {
                    var command = await ReadSocetStream(socket);

                    if (_webSocketRecieveResult.MessageType == WebSocketMessageType.Text)
                    {
                        await Router.TracePackage(command);
                    }
                }

                await socket.CloseAsync(
                    _webSocketRecieveResult.CloseStatus.Value,
                    _webSocketRecieveResult.CloseStatusDescription,
                    CancellationToken.None);

            }
            catch (Exception ex)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, ex.Message, CancellationToken.None);
            }
        }

        private static async Task<RequestModel> ReadSocetStream(WebSocket socket)
        {
            _webSocketRecieveResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(_buffer),
                CancellationToken.None);

            var request = Encoding.UTF8.GetString(_buffer).Substring(0, _webSocketRecieveResult.Count);

            return JsonConvert.DeserializeObject<RequestModel>(request);
        }
    }
}
