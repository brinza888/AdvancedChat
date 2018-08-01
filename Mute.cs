using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedChat
{
    public class Mute
    {
        private static Dictionary<CSteamID, Mute> mutes = new Dictionary<CSteamID, Mute>();

        public static bool IsMuted(CSteamID id)
        {
            return mutes.ContainsKey(id);
        }

        public static Mute GetMute(CSteamID id)
        {
            return mutes[id];
        }

        public static void UpdateMutes()
        {
            try
            {
                foreach (KeyValuePair<CSteamID, Mute> m in mutes)
                {
                    m.Value.Check();
                }
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        public DateTime TimeStamp { get; private set; }
        public int Seconds { get; private set; }
        public CSteamID PlayerID { get; private set; }
        public CSteamID JudgeID { get; private set; }
        public String Reason { get; private set; }
        public bool Perma { get; private set; }

        public Mute(CSteamID player, CSteamID judge, int seconds, String reason = "*none*")
        {
            TimeStamp = DateTime.Now;
            PlayerID = player;
            Seconds = seconds;
            Reason = reason;
            JudgeID = judge;
            Perma = false;
            mutes.Add(PlayerID, this);

            if (AdvancedChatPlugin.Instance.Configuration.Instance.BroadcastMute)
            {
                UnturnedChat.Say(AdvancedChatPlugin.Instance.Translate("mute_broadcast", PlayerName, Seconds, JudgeName), UnityEngine.Color.magenta);
                if (reason != "*none*")
                {
                    UnturnedChat.Say(AdvancedChatPlugin.Instance.Translate("reason_description", Reason), UnityEngine.Color.magenta);
                }
            }
        }

        public Mute(CSteamID player, CSteamID judge, String reason = "*none*")
        {
            TimeStamp = DateTime.Now;
            PlayerID = player;
            Perma = true;
            Reason = reason;
            JudgeID = judge;
            mutes.Add(PlayerID, this);

            if (AdvancedChatPlugin.Instance.Configuration.Instance.BroadcastMute)
            {
                UnturnedChat.Say(AdvancedChatPlugin.Instance.Translate("permamute_broadcast", PlayerName, JudgeName), UnityEngine.Color.magenta);
                if (reason != "*none*")
                {
                    UnturnedChat.Say(AdvancedChatPlugin.Instance.Translate("reason_description", Reason), UnityEngine.Color.magenta);
                }  
            }
        }

        public void Check()
        {
            if (Perma)
            {
                return;
            }

            TimeSpan result = DateTime.Now - TimeStamp;
            if (result.TotalSeconds >= Seconds)
            {
                Unmute();
            }
        }

        public void Unmute()
        {
            mutes.Remove(PlayerID);

            if (AdvancedChatPlugin.Instance.Configuration.Instance.BroadcastUnmute)
            {
                UnturnedChat.Say(AdvancedChatPlugin.Instance.Translate("unmute_broadcast", PlayerName), UnityEngine.Color.magenta);
            }
            
        }

        public string PlayerName
        {
            get
            {
                UnturnedPlayer pl = UnturnedPlayer.FromCSteamID(PlayerID);
                try
                {
                    return pl.CharacterName;
                }
                catch (NullReferenceException)
                {
                    return PlayerID.ToString();
                }
            }
        }

        public string JudgeName
        {
            get
            {
                UnturnedPlayer pl = UnturnedPlayer.FromCSteamID(JudgeID);
                try
                {
                    return pl.CharacterName;
                }
                catch (NullReferenceException)
                {
                    return JudgeID.ToString();
                }
            }
        }

        public string RemainingTime
        {
            get
            {
                TimeSpan result = DateTime.Now - TimeStamp;
                string remain = $"{result.Minutes}m {result.Seconds}s";
                return remain;
            }
        }

    }
}
