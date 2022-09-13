using Server.Model;
using System.Net.WebSockets;

namespace Server
{
    public static class Socket
    {
        private static List<SocketModel> Sockets = new List<SocketModel>();
        private static DateTime LastHearthbeatTime;

        public static void Add(SocketModel model)
        {
            Sockets.Add(model);
        }

        public static List<SocketModel> OpenIotSockets()
        {
            return Sockets
                .Where(x => x.Type == SocketType.IOT && x.Socket.State == WebSocketState.Open)
                .ToList();
        }

        public static List<SocketModel> All()
        {
            return Sockets;
        }
    }
}
