using System.Net.WebSockets;

namespace Server.Model
{
    public class SocketModel
    {
        public DateTime UpdateTime { get; set; }
        public WebSocket Socket { get; set; }
        public SocketType Type { get; set; }

        public SocketModel(WebSocket socket, SocketType type)
        {
            UpdateTime = DateTime.UtcNow;
            Socket = socket;
            Type = type;    
        }
    }


    public enum SocketType
    {
        UNKNOWN,
        IOT,
        WEB_BROWSER
    }
}
