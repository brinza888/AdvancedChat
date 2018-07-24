using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using Steamworks;
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
            CSteamID judgeID;
            try
            {
                judgeID = ((UnturnedPlayer)caller).CSteamID;
            }
            catch (InvalidCastException)
            {
                judgeID = new CSteamID();
            }
            UnturnedPlayer target = command.GetUnturnedPlayerParameter(0);
            if (target == null)
            {
                UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("player_not_found"), UnityEngine.Color.red);
                return;
            }
            if (AdvancedChatPlugin.Instance.Mutes.ContainsKey(target.CSteamID))
            {
                UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("already_muted"), UnityEngine.Color.red);
                return;
            }
            switch (command.Length)
            {
                case 1:
                    new Mute(target.CSteamID, judgeID);
                    break;
                case 2:
                    int seconds;
                    if (int.TryParse(command[1], out seconds))
                    {
                        new Mute(target.CSteamID, judgeID, seconds);
                    }
                    else
                    {
                        new Mute(target.CSteamID, judgeID, reason: command[1]);
                    }
                    break;
                case 3:
                    int time;
                    if (!int.TryParse(command[1], out time))
                    {
                        UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("wrong_time"), UnityEngine.Color.red);
                    }
                    new Mute(target.CSteamID, judgeID, time, command[2]);
                    break;
                default:
                    UnturnedChat.Say(caller, AdvancedChatPlugin.Instance.Translate("wrong_usage"), UnityEngine.Color.red);
                    return;
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
            get { return "<player> [duration] [reason]"; }
        }
    }
}