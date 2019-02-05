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

        public override TranslationList DefaultTranslations => new TranslationList()
                {
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
                    {"not_muted", "This player not muted yet!"},
                    {"player_hasnt_warnings", "This player hasn't warnings yet!"}
                };

        protected override void Load()
        {
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin loaded!");
            Rocket.Core.Logging.Logger.Log("Broadcast mute: " + Configuration.Instance.BroadcastMute);
            Rocket.Core.Logging.Logger.Log("Broadcast unmute: " + Configuration.Instance.BroadcastUnmute);
            Rocket.Core.Logging.Logger.Log("Warnings before mute: " + Configuration.Instance.MaxWarnings);
            Rocket.Core.Logging.Logger.Log("AutoMute duration: " + Configuration.Instance.AutoMuteDuration);
            Rocket.Core.Logging.Logger.Log("Created by Brinza Bezrukoff");
            Rocket.Core.Logging.Logger.Log("Vk: vk.com/brinza888");
            Rocket.Core.Logging.Logger.Log("Mail: bezrukoff888@gmail.com");
            UnturnedPlayerEvents.OnPlayerChatted += PlayerChatted;
            Instance = this;
        }

        private void PlayerChatted(UnturnedPlayer player, ref UnityEngine.Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            int maxWarnings = Configuration.Instance.MaxWarnings;

            if (Mute.IsMuted(player.CSteamID))
            {
                if (!Mute.GetMute(player.CSteamID).Perma)
                    UnturnedChat.Say(player, Translate("you_in_mute", Mute.GetMute(player.CSteamID).RemainingTime), UnityEngine.Color.red);
                else
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
                            WarnedPlayers[player.CSteamID] += 1;
                        else
                            WarnedPlayers.Add(player.CSteamID, 1);
                        UnturnedChat.Say(player, Translate("you_use_badword", WarnedPlayers[player.CSteamID], maxWarnings), UnityEngine.Color.red);
                        cancel = true;
                    }
                }
            }

            if (maxWarnings > 0 && !player.HasPermission("AdvancedChat.BypassAutoMute"))
            {
                if (WarnedPlayers.ContainsKey(player.CSteamID) && WarnedPlayers[player.CSteamID] == maxWarnings)
                {
                    new Mute(player.CSteamID, new CSteamID(0), Configuration.Instance.AutoMuteDuration, Translate("automute_reason"));
                    Instance.WarnedPlayers.Remove(player.CSteamID);
                }
            }
        }

        public void FixedUpdate()
        {
            Mute.UpdateMutes();
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerChatted -= PlayerChatted;
            Rocket.Core.Logging.Logger.Log("AdvancedChat plugin unloaded!");
        }
    }
}
