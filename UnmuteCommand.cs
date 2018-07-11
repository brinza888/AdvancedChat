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
    class UnmuteCommand : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer target = command.GetUnturnedPlayerParameter(0);
            if (target == null)
            {
                UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("player_not_found"), UnityEngine.Color.red);
            }
            else
            {
                AdvancedChatPlugin.Instance.UnmutePlayer(target.CSteamID);
            }
        }

        public string Help
        {
            get { return "Unmute player in chat"; }
        }

        public string Name
        {
            get { return "unmute"; }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "AdvancedChat.Unmute" }; }
        }

        public string Syntax
        {
            get { return "<player>"; }
        }
    }
}