using Server.Model;

namespace Server
{
    public static class Socket
    {
        public static List<SocketModel> Sockets = new List<SocketModel>();
        public static DateTime LastHearthbeatTime;
    }
}
