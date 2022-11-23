# AdvancedChat

**This one plugin may not work now, because of outdated code or old build.**

AdvancedChat is Rocket plugin for Unturned. This plugin is modified version of famous ChatControl plugin. It was created independently and without using ANY part of ChatControl source code.

Features:
- Mute duration
- Mute reason
- Words blacklist and automute
- Mute broadcasting

Plugin AdvancedChat created by Ilya Bezrukov:
- My telegram: https://t.me/brinza888
- My email: bezrukoff888@gmail.com

**This software is licensed under the BSD 3-Clause License (see LICENSE file).**

## Commands
- ### Mute command
  Use for **mute** player in chat
  - Syntax:
    - /mute [nick] [duration] [reason] - mute for duration (in sec) and with reason
    - /mute [nick] [duration] - mute for duration(in sec) without reason
    - /mute [nick]  [reason] - mute permanently with reason
    - /mute [nick] - mute permanently without reason
  - Permission: AdvancedChat.Mute
- ### Unmute command
  Use for **unmute** player in chat
  - Syntax: /umute [nick]
  - Permission: AdvancedChat.Unmute
## Plugin systems
- ### Blacklisted words
  - If player message contains **blacklisted word**, message will not be sent
  - Also player, who used **blacklisted word** will get **warning**
- ### Warnings
  - If player has defined **warnings** count he will get automute for defined duration with defined reason
## Plugin configure
- ### Permissions
  - AdvancedChat.Mute - /mute command permission
  - AdvancedChat.Unmute - /unmute command permission
  - AdvancedChat.BypassBadWords - allows use in chat **blacklisted words** for the player
  - AdvancedChat.BypassAutoMute - **automute** punishment will not be applied to the player
- ### Configuration
  - BroadcastMute - if true plugin will announce about player's mute
  - BroadcastUnmute - if true plugin will announce about player's unmute
  - WarningsBeforeMute - how many warnings player need to get before plugin will **automute** him
  - AutoMuteDuration - duration of **automute**
  - WordsBlackList - list of all **blacklisted words**
- ### Translations
  - mute_broadcast - message that will be broadcast, if player gets mute (if **BroadcastMute** configuration true
    - {0} is a player name
    - {1} is mute time
  - permamute_broadcast - message that will be broadcast, if player gets mute permanently (if **BroadcastMute** configuration true)
    - {0} is a player name
  - unmute_boadcast - message that will be broadcast, if player gets unmute (if **BroadcastUnmute** configuration true)
    - {0} is a player name
  - you_in_mute - message that player will get if try to chat when he mutted
    - {0} is remaining mute time
  - you_use_badword - message that player will get if try to write message with **blacklisted word**
    - {0} is warnings that player had + 1 warning for **blacklisted word** using
    - {1} is warnings that player need to reach for get **automute**
  - player_not_found - error message, if you incorrect define player in /mute or /unmute commands
  - wrong_time - errror message, if you incorrect define time in /mute command (eg string instead number)
