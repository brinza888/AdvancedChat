using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedChat
{
    public class CWarnsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "cwarns";

        public string Help => "Clear player warnings";

        public string Syntax => "<player>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "AdvancedChat.CWarns" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer target = command.GetUnturnedPlayerParameter(0);
            if (target == null)
            {
                UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("player_not_found"), UnityEngine.Color.red);
                return;
            }
            if (!AdvancedChatPlugin.Instance.WarnedPlayers.ContainsKey(target.CSteamID))
            {
                UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("player_hasnt_warnings"), UnityEngine.Color.red);
                return;
            }
            AdvancedChatPlugin.Instance.WarnedPlayers.Remove(target.CSteamID);
        }
    }
}
