# HollowKnight.RandomTeleport
A mod that randomly Teleports you to another scene.  
For any questions, bugs, or suggestions, [Join the discord](https://discord.gg/F6Y5TeFQ8j)

## Teleport Triggers
There are 2 primary ways to randomly teleport  
1. Time - The teleportation happens every `x` minutes (which can be changed in modmenu). This is the intended way to play the mod.
2. Damage - The teleportation happens every time you take damage

- Alternatively, there is also the option to randomly teleport using the keyboard/controller bind in the modmenu. This can be used regardless of the teleport trigger chosen
#### Note: It is recommended to have a keybind set (even if you dont want to use it) because sometimes leaving a room that you were teleported to and were not supposed to be able to access, causes and infinite transistion (which can be escaped by pressing the keybind to randomly teleporting to another scene) 

## Settings
There are many settings that can be toggled in the modmenu
* `Teleport Trigger`: Choose between the options in [Teleport Triggers](#teleport-triggers)
* Time only settings:
  * `Show Timer`: Decides wheter to show the a display timer ingame or not. (default: true)
  * `Random Teleport Time`: Choose the time between teleports (in minutes) (default: 2)
  * `Time increase from damage`: When you take damage how much of the time in the timer is increased as a penalty/reward. Can be set to 0 if you dont want this feature (default: 0)
  * `Time reduction from geo`: When you collect geo how much of the time in the timer is decreased as a penalty/reward. Can be set to 0 if you dont want this feature (default: 0)
* `Same Area Teleport`: Choose wheter to only teleport you to the same scenes or not (default: false)
* `Only Visited Scenes`: Choose wheter to only teleport you to scenes you have already visited (default: false)
* `Change Scene`: The keybind and button bind that will allow you to teleport on bind press
