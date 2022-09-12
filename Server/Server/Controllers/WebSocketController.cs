using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server;
using Server.Attribute;
using Server.Model;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketsSample.Controllers;

public class WebSocketController : ControllerBase
{
   
    [HttpConnect("/ws")]
    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            await Processing(await HttpContext.WebSockets.AcceptWebSocketAsync());
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

    }

    private async Task Processing(WebSocket webSocket)
    {
        var buffer = new byte[1024];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), 
            CancellationToken.None);

        var request = Encoding.UTF8.GetString(buffer);

        try
        {
            SocketType socketType = SocketType.UNKNOWN;

            if (JsonConvert.DeserializeObject<RequestModel>(request)?.Command == Command.REGISTER_NODE_MCU)
            {
                Socket.Sockets.Add(new SocketModel()
                {
                    Socket = webSocket,
                    Type = SocketType.IOT,
                    UpdateTime = DateTime.UtcNow
                });

                await Router.TracePackage((new RequestModel
                {
                    Command = Command.REGISTER_NODE_MCU
                }));
            }
            else if (JsonConvert.DeserializeObject<RequestModel>(request)?.Command == Command.REGISTER_WEB_BROWSER)
            {
                Socket.Sockets.Add(new SocketModel()
                {
                    Socket = webSocket,
                    Type = SocketType.WEB_BROWSER,
                    UpdateTime = DateTime.UtcNow
                });

                if (Socket.Sockets.Any(x => x.Type == SocketType.IOT && x.Socket.State == WebSocketState.Open))
                {
                    await Router.TracePackage((new RequestModel
                    {
                        Command = Command.REGISTER_NODE_MCU
                    }));
                }
            }


            while (!receiveResult.CloseStatus.HasValue)
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string command = Encoding.UTF8.GetString(buffer).Substring(0, receiveResult.Count);
                    await Router.TracePackage(JsonConvert.DeserializeObject<RequestModel>(command));
                }
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);

        }
        catch (Exception ex)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, ex.Message, CancellationToken.None);
        }
    }
}
