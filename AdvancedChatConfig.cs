using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedChat
{
    public class AdvancedChatConfig : IRocketPluginConfiguration
    {
        public bool BroadcastMute;
        public bool BroadcastUnmute;
        public int WarningsBeforeMute;
        public int AutoMuteDuration;
        public uint AutoBanDuration;
        public List<string> WordsBlackList;

        public void LoadDefaults()
        {
            BroadcastMute = true;
            BroadcastUnmute = true;
            WarningsBeforeMute = 5;
            AutoMuteDuration = 60;
            AutoBanDuration = 3600;
            WordsBlackList = new List<string>() { "fuck", "suck", "bitch" };
        }
    }
}
