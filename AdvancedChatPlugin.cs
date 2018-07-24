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

        public Dictionary<CSteamID, int> WarnedPlayers = new Dictionary<CSteamID, int>();
        public Dictionary<CSteamID, Mute> Mutes = new Dictionary<CSteamID, Mute>();

        public override TranslationList DefaultTranslations => new TranslationList()
                {
                    {"kick_ban_reason", "You used blacklisted word in chat"},
                    {"mute_broadcast", "{0} is now muted for {1} by {2}"},
                    {"permamute_broadcast", "{0} is now permamuted by {1}"},
                    {"unmute_broadcast", "{0} is now unmuted"},
                    {"you_in_mute", "You are muted for {0}"},
                    {"you_in_permamute", "You are muted permanently"},
                    {"you_use_badword", "You used blacklisted word! This {0}/{1} your warning"},
                    {"player_not_found", "Target player not found"},
                    {"wrong_time", "Wrong time parameter!"},
                    {"wrong_usage", "Wrong syntax!"},
                    {"automute_reason", "Too much warnings"},
                    {"reason_description", "Reason: {0}"},
                    {"already_muted", "This player already muted!"},
                    {"not_muted", "This player not muted yet!"}
                };

        protected override void Load()
        {
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin loaded!");
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
            int warnBeforeMute = Configuration.Instance.WarningsBeforeMute;
            int warnBeforeKick = Configuration.Instance.WarningsBeforeKick;
            int warnBeforeBan = Configuration.Instance.WarningsBeforeBan;

            if (Mutes.ContainsKey(player.CSteamID))
            {
                if (!Mutes[player.CSteamID].Perma)
                {
                    UnturnedChat.Say(player, Translate("you_in_mute", Mutes[player.CSteamID].RemainingTime), UnityEngine.Color.red);
                }
                else
                {
                    UnturnedChat.Say(player, Translate("you_in_permamute"), UnityEngine.Color.red);
                }

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
                        UnturnedChat.Say(player, Translate("you_use_badword", WarnedPlayers[player.CSteamID], warnBeforeMute), UnityEngine.Color.red);
                        cancel = true;
                    }
                }
            }

            if (!player.HasPermission("AdvancedChat.BypassAutoMute"))
            {
                if (Configuration.Instance.WarningsBeforeMute > 0 && WarnedPlayers.ContainsKey(player.CSteamID) && WarnedPlayers[player.CSteamID] == warnBeforeMute)
                {
                    new Mute(player.CSteamID, new CSteamID(0), Configuration.Instance.AutoMuteDuration, Translate("automute_reason"));
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
            if (Mutes.Count > 0)
            {
                try
                {
                    foreach (KeyValuePair<CSteamID, Mute> pair in Mutes)
                    {
                        pair.Value.Check();
                    }
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerChatted -= PlayerChatted;
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin unloaded!");
        }
    }
}
