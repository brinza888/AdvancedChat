using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.Core.Plugins;
using Rocket.Core.Logging;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using Rocket.Unturned.Player;
using UnityEngine;
using Steamworks;

namespace AdvancedChat
{
    class AdvancedChatPlugin : RocketPlugin<AdvancedChatConfig>
    {
        public static AdvancedChatPlugin Instance;
        private Dictionary<CSteamID, DateTime> MutedPlayersTimeStamps = new Dictionary<CSteamID, DateTime>();
        private Dictionary<CSteamID, uint> MutedPlayers = new Dictionary<CSteamID, uint>();
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
                    {"wrong_time", "Invalid time parameter"}
                };
            }
        }

        protected override void Load()
        {
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin loaded");
            Rocket.Core.Logging.Logger.Log("Broadcast mute " + Configuration.Instance.BroadcastMute);
            Rocket.Core.Logging.Logger.Log("Broadcast unmute " + Configuration.Instance.BroadcastUnmute);
            Rocket.Core.Logging.Logger.Log("Warnings before mute " + Configuration.Instance.WarningsBeforeMute);
            Rocket.Core.Logging.Logger.Log("AutoMute duration " + Configuration.Instance.AutoMuteDuration);
            Rocket.Core.Logging.Logger.Log("AutoBan duration " + Configuration.Instance.AutoBanDuration);
            Rocket.Core.Logging.Logger.Log("Created by Brinza Bezrkoff");
            Rocket.Core.Logging.Logger.Log("About any issues write me on email");
            Rocket.Core.Logging.Logger.Log("My vk page: https://vk.com/brinza888");
            Rocket.Core.Logging.Logger.Log("My email: bezrukoff888@gmail.com");
            UnturnedPlayerEvents.OnPlayerChatted += PlayerChatted;
            Instance = this;
        }

        private void PlayerChatted(UnturnedPlayer player, ref UnityEngine.Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            int warningsBeforeMute = Configuration.Instance.WarningsBeforeMute;
            int waringsBeforeKick = Configuration.Instance.WarningsBeforeKick;
            int warningsBeforeBan = Configuration.Instance.WarningsBeforeBan;

            if(MutedPlayers.ContainsKey(player.CSteamID))
            {
                long last = MutedPlayersTimeStamps[player.CSteamID].Second + MutedPlayers[player.CSteamID] - DateTime.Now.Second;
                UnturnedChat.Say(player, Translate("you_in_mute", last), UnityEngine.Color.red);
                cancel = true;
                return;
            }

            if (!player.HasPermission("AdvancedChat.BypassBadWords"))
            {
                foreach(string badword in Configuration.Instance.WordsBlackList)
                {
                    if (message.ToLower().Contains(badword.ToLower()))
                    {
                        if(WarnedPlayers.ContainsKey(player.CSteamID))
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
                    MutePlayer(player, Configuration.Instance.AutoMuteDuration);
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
                        UnturnedPlayer player = UnturnedPlayer.FromCSteamID(pair.Key);
                        if (player is UnturnedPlayer)
                        {
                            UnmutePlayer(player);
                            break;
                        }
                    }
                }
            }
        }

        public void MutePlayer(UnturnedPlayer player, uint duration)
        {
            MutedPlayers.Add(player.CSteamID, duration);
            MutedPlayersTimeStamps.Add(player.CSteamID, DateTime.Now);
            if (Configuration.Instance.BroadcastMute)
            {
                UnturnedChat.Say(Translate("mute_broadcast", player.CharacterName, duration), UnityEngine.Color.magenta);
            }
        }

        public void MutePlayer(UnturnedPlayer player)
        {
            MutedPlayers.Add(player.CSteamID, 4000000000);
            MutedPlayersTimeStamps.Add(player.CSteamID, DateTime.Now);
            if (Configuration.Instance.BroadcastMute)
            {
                UnturnedChat.Say(Translate("permamute_broadcast", player.CharacterName), UnityEngine.Color.magenta);
            }
        }

        public void UnmutePlayer(UnturnedPlayer player)
        {
            MutedPlayers.Remove(player.CSteamID);
            MutedPlayersTimeStamps.Remove(player.CSteamID);
            if (Configuration.Instance.BroadcastUnmute)
            {
                UnturnedChat.Say(Translate("unmute_broadcast", player.CharacterName), UnityEngine.Color.magenta);
            }
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerChatted -= PlayerChatted;
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin unloaded");
        }
    }
}
