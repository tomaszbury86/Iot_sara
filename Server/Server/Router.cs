using Newtonsoft.Json;
using Server.Model;
using System.Net.WebSockets;
using System.Text;

namespace Server
{
    public class Router
    {
        public List<KeyValuePair<SocketType, RequestModel>> TraceMessage(RequestModel? command)
        {
            List<KeyValuePair<SocketType, RequestModel>> result = new List<KeyValuePair<SocketType, RequestModel>>();
            Func<SocketType, RequestModel, bool> AddToResult = new Func<SocketType, RequestModel, bool>((socket, command) => {
                result.Add(new KeyValuePair<SocketType, RequestModel>(socket, command));
                return true;
            });

            if (command != null)
            {
                if (command.Command == Command.GET_GPIO_LIST_DONE)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                }
                else if (command.Command == Command.REGISTER_NODE_MCU)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                    AddToResult(SocketType.IOT, new RequestModel()
                    {
                        Command = Command.GET_GPIO_LIST,
                        Args = "0"
                    });
                }
                else if (command.Command == Command.UNREGISTER_NODE_MCU)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                }
                else if (command.Command == Command.SWITCH_LED)
                {
                    AddToResult(SocketType.IOT, command);
                }
                else if (command.Command == Command.SWITCH_GPIO)
                {
                    AddToResult(SocketType.IOT, command);
                }
                else if (command.Command == Command.SWITCH_LED_DONE)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                }
                else if (command.Command == Command.SWITCH_GPIO_DONE)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                }
                else if (command.Command == Command.OFF_GPIO)
                {
                    AddToResult(SocketType.IOT, command);
                }
                else if (command.Command == Command.ON_GPIO)
                {
                    AddToResult(SocketType.IOT, command);
                }
                else if (command.Command == Command.OFF_GPIO_DONE)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                }
                else if (command.Command == Command.ON_GPIO_DONE)
                {
                    AddToResult(SocketType.WEB_BROWSER, command);
                }
            }

            return result;
        }



        //private static void hearthbeat()
        //{
        //    task.factory.startnew(async () =>
        //    {
        //        while (true)
        //        {
        //            await router.send(socket.all()
        //                    .where(x => x.type == sockettype.iot)
        //                    .select(x => x.socket)
        //                    .tolist(),
        //                    new requestmodel { command = command.switch_led });


        //            if ((datetime.utcnow - socket.lasthearthbeattime).totalseconds > 15)
        //            {
        //                await send(socket.all()
        //                 .where(x => x.type == sockettype.web_browser)
        //                 .select(x => x.socket)
        //                 .tolist(), new requestmodel() { command = command.unregister_node_mcu });

        //                break;
        //            }

        //            thread.sleep(2000);
        //        }
        //    });
        //}
    }
}
