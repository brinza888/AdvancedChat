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
    class MuteCommand : IRocketCommand
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
                if (command.Length < 2)
                {
                    AdvancedChatPlugin.Instance.MutePlayer(target.CSteamID);
                }
                else
                {
                    uint duration;
                    if (uint.TryParse(command[1], out duration))
                    {
                        AdvancedChatPlugin.Instance.MutePlayer(target.CSteamID, duration);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("wrong_time"), UnityEngine.Color.red);
                    }
                }
            }
        }

        public string Help
        {
            get { return "Mute player in chat"; }
        }

        public string Name
        {
            get { return "mute"; }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "AdvancedChat.Mute" }; }
        }

        public string Syntax
        {
            get { return "<player> <duration>"; }
        }
    }
}