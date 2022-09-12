using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Security.Principal;

namespace Server.Model
{
    public class RequestModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Command Command { get; set; }
        public string? Args { get; set; }
       
    }

    public enum Command
    {
        SWITCH_LED_DONE,
        GET_GPIO_LIST,
        REGISTER_NODE_MCU,
        SWITCH_LED,
        UNREGISTER_NODE_MCU,
        REGISTER_WEB_BROWSER,
        GET_GPIO_LIST_DONE,
        SWITCH_GPIO_DONE,
        SWITCH_GPIO,
        OFF_GPIO,
        ON_GPIO,
        OFF_GPIO_DONE,
        ON_GPIO_DONE
    }
}
