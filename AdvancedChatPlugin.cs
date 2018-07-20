using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedChat
{
    class AdvancedChatPlugin : RocketPlugin<AdvancedChatConfig>
    {
        public static AdvancedChatPlugin Instance;
        private Dictionary<CSteamID, DateTime> MutedPlayersTimeStamps = new Dictionary<CSteamID, DateTime>();
        private Dictionary<CSteamID, uint> MutedPlayers = new Dictionary<CSteamID, uint>();
        private List<CSteamID> PermaMute = new List<CSteamID>();
        private Dictionary<CSteamID, int> WarnedPlayers = new Dictionary<CSteamID, int>();

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"kick_ban_reason", "You used blacklisted word in chat"},
                    {"mute_broadcast", "{0} is now muted for {1} seconds"},
                    {"permamute_broadcast", "{0} is now muted permanently"},
                    {"unmute_broadcast", "{0} is now unmuted"},
                    {"you_in_mute", "You are muted for {0} seconds"},
                    {"you_use_badword", "You used blacklisted word! This {0}/{1} your warning"},
                    {"player_not_found", "Target player not found"},
                    {"wrong_time", "Invalid time parameter"},
                    {"you_in_permamute", "You are muted permanently"}
                };
            }
        }

        protected override void Load()
        {
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin loaded");
            Rocket.Core.Logging.Logger.Log("Broadcast mute: " + Configuration.Instance.BroadcastMute);
            Rocket.Core.Logging.Logger.Log("Broadcast unmute: " + Configuration.Instance.BroadcastUnmute);
            Rocket.Core.Logging.Logger.Log("Warnings before mute: " + Configuration.Instance.WarningsBeforeMute);
            Rocket.Core.Logging.Logger.Log("AutoMute duration: " + Configuration.Instance.AutoMuteDuration);
            Rocket.Core.Logging.Logger.Log("AutoBan duration: " + Configuration.Instance.AutoBanDuration);
            Rocket.Core.Logging.Logger.Log("Created by Brinza Bezrukoff");
            Rocket.Core.Logging.Logger.Log("Vk: vk.com/brinza888");
            Rocket.Core.Logging.Logger.Log("Mail: bezrukoff888@gmail.com");
            UnturnedPlayerEvents.OnPlayerChatted += PlayerChatted;
            Instance = this;
        }

        private void PlayerChatted(UnturnedPlayer player, ref UnityEngine.Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            int warningsBeforeMute = Configuration.Instance.WarningsBeforeMute;
            int waringsBeforeKick = Configuration.Instance.WarningsBeforeKick;
            int warningsBeforeBan = Configuration.Instance.WarningsBeforeBan;

            if (MutedPlayers.ContainsKey(player.CSteamID))
            {
                long last = MutedPlayersTimeStamps[player.CSteamID].Second + MutedPlayers[player.CSteamID] - DateTime.Now.Second;
                UnturnedChat.Say(player, Translate("you_in_mute", last), UnityEngine.Color.red);
                cancel = true;
                return;
            }

            if (PermaMute.Contains(player.CSteamID))
            {
                UnturnedChat.Say(player, Translate("you_in_permamute"), UnityEngine.Color.red);
                cancel = true;
                return;
            }

            if (!player.HasPermission("AdvancedChat.BypassBadWords"))
            {
                foreach (string badword in Configuration.Instance.WordsBlackList)
                {
                    if (message.ToLower().Contains(badword.ToLower()))
                    {
                        if (WarnedPlayers.ContainsKey(player.CSteamID))
                        {
                            WarnedPlayers[player.CSteamID] += 1;
                        }
                        else
                        {
                            WarnedPlayers.Add(player.CSteamID, 1);
                        }
                        UnturnedChat.Say(player, Translate("you_use_badword", WarnedPlayers[player.CSteamID], warningsBeforeMute), UnityEngine.Color.red);
                        cancel = true;
                    }
                }
            }

            if (!player.HasPermission("AdvancedChat.BypassAutoMute"))
            {
                if (Configuration.Instance.WarningsBeforeMute > 0 && WarnedPlayers.ContainsKey(player.CSteamID) && WarnedPlayers[player.CSteamID] == warningsBeforeMute)
                {
                    MutePlayer(player.CSteamID, Configuration.Instance.AutoMuteDuration);
                }
            }

            if (!player.HasPermission("AdvancedChat.BypassAutoKick"))
            {
                if (Configuration.Instance.WarningsBeforeKick > 0 && WarnedPlayers.ContainsKey(player.CSteamID) && WarnedPlayers[player.CSteamID] == Configuration.Instance.WarningsBeforeKick)
                {
                    player.Kick(Translate("kick_ban_reason"));
                }
            }

            if (!player.HasPermission("AdvancedChat.BypassAutoBan"))
            {
                if (Configuration.Instance.WarningsBeforeBan > 0 && WarnedPlayers.ContainsKey(player.CSteamID) && WarnedPlayers[player.CSteamID] == Configuration.Instance.WarningsBeforeBan)
                {
                    player.Ban(Translate("kick_ban_reason"), Configuration.Instance.AutoBanDuration);
                }
            }
        }

        public void FixedUpdate()
        {
            if (MutedPlayers.Count > 0 && MutedPlayersTimeStamps.Count > 0)
            {
                foreach (KeyValuePair<CSteamID, DateTime> pair in MutedPlayersTimeStamps)
                {
                    if ((DateTime.Now - pair.Value).TotalSeconds >= MutedPlayers[pair.Key])
                    {
                        UnmutePlayer(pair.Key);
                        break;
                    }
                }
            }
        }

        public void MutePlayer(CSteamID playerID, uint duration)
        {
            if (MutedPlayers.ContainsKey(playerID))
            {
                MutedPlayers.Remove(playerID);
                MutedPlayersTimeStamps.Remove(playerID);
            }

            if (PermaMute.Contains(playerID))
            {
                PermaMute.Remove(playerID);
            }

            MutedPlayers.Add(playerID, duration);
            MutedPlayersTimeStamps.Add(playerID, DateTime.Now);

            if (Configuration.Instance.BroadcastMute)
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(playerID);
                try
                {
                    UnturnedChat.Say(Translate("mute_broadcast", player.CharacterName, duration), UnityEngine.Color.magenta);
                }
                catch (NullReferenceException)
                {
                    UnturnedChat.Say(Translate("mute_broadcast:", playerID, duration), UnityEngine.Color.magenta);
                }
            }
        }

        public void MutePlayer(CSteamID playerID)
        {
            PermaMute.Add(playerID);

            if (MutedPlayers.ContainsKey(playerID))
            {
                MutedPlayers.Remove(playerID);
                MutedPlayersTimeStamps.Remove(playerID);
            }

            if (Configuration.Instance.BroadcastMute)
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(playerID);
                try
                {
                    UnturnedChat.Say(Translate("permamute_broadcast", player.CharacterName), UnityEngine.Color.magenta);
                }
                catch (NullReferenceException)
                {
                    UnturnedChat.Say(Translate("permamute_broadcast", playerID), UnityEngine.Color.magenta);
                }
            }
        }

        public void UnmutePlayer(CSteamID playerID)
        {
            bool flag = true;
            if (MutedPlayers.ContainsKey(playerID))
            {
                MutedPlayers.Remove(playerID);
                MutedPlayersTimeStamps.Remove(playerID);
                flag = false;
            }

            if (PermaMute.Contains(playerID))
            {
                PermaMute.Remove(playerID);
                flag = false;
            }

            if (flag) { return; }

            if (Configuration.Instance.BroadcastUnmute)
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(playerID);
                try
                {
                    UnturnedChat.Say(Translate("unmute_broadcast", player.CharacterName), UnityEngine.Color.magenta);
                }
                catch (NullReferenceException)
                {
                    UnturnedChat.Say(Translate("unmute_broadcast", playerID), UnityEngine.Color.magenta);
                }
            }
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerChatted -= PlayerChatted;
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin unloaded");
        }
    }
}
