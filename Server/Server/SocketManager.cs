using Microsoft.AspNetCore.Components.Routing;
using Newtonsoft.Json;
using Server.Model;
using System.Net.WebSockets;
using System.Text;
using SocketType = Server.Model.SocketType;

namespace Server
{
    public class SocketManager
    {
        private object _syncObject = new object();
        private byte[] _buffer = new byte[1024];
        private List<SocketModel> Sockets = new List<SocketModel>();
        private WebSocketReceiveResult? _webSocketRecieveResult;
        private Router _router;

        public SocketManager(Router router)
        {
            _router = router;
            RemoveClodesConnections();
        }

        public async Task ProcessSocketMessage(WebSocket socket)
        {
            var request = await ReadSocetStream(socket);

            try
            {
                if (request?.Command == Command.REGISTER_NODE_MCU)
                {
                    Add(new SocketModel(socket, SocketType.IOT));
                    MonitorIotConnection();

                    await Send(_router.TraceMessage((new RequestModel
                    {
                        Command = Command.REGISTER_NODE_MCU
                    })));
                }
                else if (request?.Command == Command.REGISTER_WEB_BROWSER)
                {
                    Add(new SocketModel(socket, SocketType.WEB_BROWSER));

                    if (OpenIotSockets().Any())
                    {
                        await Send(_router.TraceMessage((new RequestModel
                        {
                            Command = Command.REGISTER_NODE_MCU
                        })));
                    }
                }

                if (_webSocketRecieveResult != null)
                {
                    while (!_webSocketRecieveResult.CloseStatus.HasValue)
                    {
                        var command = await ReadSocetStream(socket);

                        if (_webSocketRecieveResult.MessageType == WebSocketMessageType.Text)
                        {
                            await Send(_router.TraceMessage(command));
                        }
                    }

                    await socket.CloseAsync(
                        _webSocketRecieveResult.CloseStatus.Value,
                        _webSocketRecieveResult.CloseStatusDescription,
                        CancellationToken.None);
                }

            }
            catch (Exception ex)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, ex.Message, CancellationToken.None);
            }
        }

        private async Task<RequestModel?> ReadSocetStream(WebSocket socket)
        {
            _webSocketRecieveResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(_buffer),
                CancellationToken.None);

            var request = Encoding.UTF8.GetString(_buffer).Substring(0, _webSocketRecieveResult.Count);

            return JsonConvert.DeserializeObject<RequestModel>(request);
        }

        public async Task Send(List<KeyValuePair<SocketType, RequestModel>> messagesToSend)
        {
            foreach (var message in messagesToSend)
            {
                if (message.Key != SocketType.UNKNOWN)
                {
                    string query = JsonConvert.SerializeObject(message.Value);

                    foreach (var socket in GetAll().Where(x => x.Socket.State == WebSocketState.Open && x.Type == message.Key))
                    {
                        await socket.Socket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(query), 0, query.Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    }
                }
            }
        }

        public void Add(SocketModel model)
        {
            lock (_syncObject)
            {
                Sockets.Add(model);
            }
        }

        private List<SocketModel> OpenIotSockets()
        {
            return Sockets
                .Where(x => x.Type == SocketType.IOT && x.Socket.State == WebSocketState.Open)
                .ToList();
        }

        public List<SocketModel> GetAll()
        {
            return Sockets;
        }

        private void RemoveClodesConnections()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    lock (_syncObject)
                    {
                        Sockets.RemoveAll(x => x.Socket.State == WebSocketState.Closed);
                    }

                    Thread.Sleep(5000);
                }
            });
        }

        private void MonitorIotConnection()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var iotDeviceConnected = false;

                    lock (_syncObject)
                    {
                        iotDeviceConnected = Sockets.Any(x => x.Type == SocketType.IOT && x.Socket.State == WebSocketState.Open);
                    }

                    if (!iotDeviceConnected)
                    {
                        await Send(_router.TraceMessage(new RequestModel() { Command = Command.UNREGISTER_NODE_MCU }));
                        break;
                    }

                    Thread.Sleep(1000);
                }
            });

        }
    }

}
