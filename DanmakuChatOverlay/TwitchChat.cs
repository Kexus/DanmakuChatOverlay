using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Text.Json.Nodes;
using System.IO;


namespace DankmakuChatOverlay
{
    internal class TwitchBot
    {
        TwitchClient client;
        MainWindow window;

        public TwitchBot(MainWindow _window) 
        {
            window = _window;
            String user;
            String key;
            String channel;

            // read config file
            using (StreamReader sr = new StreamReader("config.json"))
            {
                String j = sr.ReadToEnd();

                JsonNode config = JsonNode.Parse(j);
                user = config["user"].ToString();
                key = config["key"].ToString();
                channel = config["channel"].ToString();

            }

            ConnectionCredentials credentials = new ConnectionCredentials(user, key);
            client = new TwitchClient(new WebSocketClient());
            client.Initialize(credentials, channel);

            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                window.CreateDankmaku(e.ChatMessage.Message);
            });

        }
    }
}
