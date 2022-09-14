using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server;
using Server.Attribute;
using Server.Extensions;
using Server.Model;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketsSample.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly SocketManager _socketManager;
    public WebSocketController(SocketManager socketManager)
    {
        _socketManager = socketManager;
    }

    [HttpConnect("/ws")]
    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            await _socketManager.ProcessSocketMessage(await HttpContext.WebSockets.AcceptWebSocketAsync());
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

    }
}
