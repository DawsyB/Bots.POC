using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot.Multipurpose
{
    public class ChannelUrl
    {
        private static string appId = "cd4440f5-9c13-4843-af36-65e946ad5545";
        private static string appName = "Transfer Bot";

        public static string GetTeamsURL()
        {
            return $"https://teams.microsoft.com/dl/launcher/launcher.html?url=%2f_%23%2fl%2fchat%2f0%2f0%3fusers%3d28%3a{appId}&type=chat";
        }

        public static string GetSkypeURL()
        {
            return $"https://join.skype.com/bot/{appId}";           
        }   

        public static string GetWebChatURL()
        {
            return "http://dawsagenttransferbot.azurewebsites.net/";
        }
    }
}