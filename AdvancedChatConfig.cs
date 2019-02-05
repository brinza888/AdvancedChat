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
        public int MaxWarnings;
        public int AutoMuteDuration;
        public List<string> WordsBlackList;

        public void LoadDefaults()
        {
            BroadcastMute = true;
            BroadcastUnmute = true;
            MaxWarnings = 5;
            AutoMuteDuration = 60;
            WordsBlackList = new List<string>() { "fuck", "suck", "bitch" };
        }
    }
}
