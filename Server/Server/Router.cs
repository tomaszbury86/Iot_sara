using Newtonsoft.Json;
using Server.Model;
using System.Net.WebSockets;
using System.Text;

namespace Server
{

    public static class Router
    {
        public static async Task TracePackage(RequestModel? command)
        {
            if (command != null)
            {


                if (Socket.Sockets != null)
                {
                    if (command.Command == Command.GET_GPIO_LIST_DONE)
                    {
                        await Send(Socket.Sockets
                            .Where(x => x.Type == SocketType.WEB_BROWSER)
                            .Select(x => x.Socket)
                            .ToList(), command);

                    }
                    else if (command.Command == Command.REGISTER_NODE_MCU)
                    {
                        await Send(Socket.Sockets
                           .Where(x => x.Type == SocketType.WEB_BROWSER)
                           .Select(x => x.Socket)
                           .ToList(), command);

                        await Send(Socket.Sockets
                            .Where(x => x.Type == SocketType.IOT)
                            .Select(x => x.Socket)
                            .ToList(), new RequestModel()
                            {
                                Command = Command.GET_GPIO_LIST,
                                Args = "0"
                            });

                        Socket.LastHearthbeatTime = DateTime.UtcNow;
                        //Hearthbeat();
                    }
                    else if (command.Command == Command.SWITCH_LED)
                    {
                        await Send(Socket.Sockets
                              .Where(x => x.Type == SocketType.IOT)
                              .Select(x => x.Socket)
                              .ToList(), new RequestModel() { Command = Command.SWITCH_LED });
                    }
                    else if (command.Command == Command.SWITCH_LED_DONE)
                    {
                        await Send(Socket.Sockets
                               .Where(x => x.Type == SocketType.WEB_BROWSER)
                               .Select(x => x.Socket)
                               .ToList(), command);

                        Socket.LastHearthbeatTime = DateTime.UtcNow;
                    }
                    else if (command.Command == Command.SWITCH_GPIO_DONE)
                    {
                        await Send(Socket.Sockets
                               .Where(x => x.Type == SocketType.WEB_BROWSER)
                               .Select(x => x.Socket)
                               .ToList(), command);
                    }
                    else if (command.Command == Command.OFF_GPIO)
                    {
                        await Send(Socket.Sockets
                               .Where(x => x.Type == SocketType.IOT)
                               .Select(x => x.Socket)
                               .ToList(), command);
                    }
                    else if (command.Command == Command.ON_GPIO)
                    {
                        await Send(Socket.Sockets
                               .Where(x => x.Type == SocketType.IOT)
                               .Select(x => x.Socket)
                               .ToList(), command);
                    }
                }

            }
        }

        public static async Task Send(List<WebSocket> socketList, RequestModel request)
        {
            string query = JsonConvert.SerializeObject(request);

            foreach (var socket in socketList.Where(x => x.State == WebSocketState.Open))
            {
                await socket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(query), 0, query.Length),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
            }
        }

        private static void Hearthbeat()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Router.Send(Socket.Sockets
                            .Where(x => x.Type == SocketType.IOT)
                            .Select(x => x.Socket)
                            .ToList(),
                            new RequestModel { Command = Command.SWITCH_LED });


                    if ((DateTime.UtcNow - Socket.LastHearthbeatTime).TotalSeconds > 15)
                    {
                        await Send(Socket.Sockets
                         .Where(x => x.Type == SocketType.WEB_BROWSER)
                         .Select(x => x.Socket)
                         .ToList(), new RequestModel() { Command = Command.UNREGISTER_NODE_MCU });

                        break;
                    }

                    Thread.Sleep(2000);
                }
            });
        }
    }
}
