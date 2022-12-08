# HollowKnight.RandomTeleport
A mod that randomly Teleports you to another scene.  
For any questions, bugs, or suggestions, [Join the discord](https://discord.gg/F6Y5TeFQ8j)

## Main Features
- Randomly teleports you to a new scene in a random location
- 3 methods available to teleport to new scene ([see below](#teleport-triggers))
- An option to have seeded teleports to allow 2 people to be teleported to same location at each teleport. Can be used for races and such. ([see below](#extra-settings))

## Teleport Triggers
There are 3 primary ways to randomly teleport each of which can be enabled or disabled in the modmenu.  
1. Time - The teleportation happens every `x` minutes (which can be changed in modmenu). This is the intended way to play the mod.
2. Damage - The teleportation happens every time you take damage
3. KeyPress - The teleport happens using the keyboard/controller bind in the modmenu. This also has the option to teleport back to the previous teleport (only works when not in same scene)

## Settings
There are many settings that can be toggled in the modmenu
### `Active Trigger`: 
  Use the options below this to choose which triggers are active. (clicking on this will hide the options below)
  * `Trigger: Time`: see [Teleport Triggers](#teleport-triggers)
  * `Trigger: Damage`: see [Teleport Triggers](#teleport-triggers)
  * `Trigger: KeyPress`: see [Teleport Triggers](#teleport-triggers)
### Settings for the enabled triggers:
  Use the options below to set settings.only enabled triggers settings will show
#### Time Trigger Settings
  * `Show Timer`: Decides wheter to show the a display timer ingame or not. (default: true)
  * `Random Teleport Time`: Choose the time between teleports (in seconds) (default: 120)
  * `Time reduction from damage`: How much time is removed from the timer when the player takes damages. Can be set to 0 if you dont want this feature (default: 0)
  * `Time increase from geo`: How much time is gained in the timer when the player collects geo. Can be set to 0 if you dont want this feature (default: 0)
#### Damage Trigger Settings
  * `Chance of Teleport On Damage`: When you take damage how likely are you to be teleported in % (default: 100).
#### KeyPress Trigger Settings
  * `Change Scene`: The keybind and button bind that will allow you to teleport on bind press
  * `Go To Previous Teleport`: The keybind and button bind that will allow you to go to your previous teleport (only works if you arent in the same scene).
## Extra Settings:
These can be accessed by clicking the extra settings button at the end of the mod menu.
  * `RNG Seed for new saves`: Allows you to choose the seed that will be used to generate the rng for teleports. only will affect new saves
  * `Same Area Teleport`: Choose whether to only teleport you to the same scenes or not (default: false)
  * `Only Visited Scenes`: Choose whether to only teleport you to scenes you have already visited (default: false)
  * `AllowGodHomeBosses`: Choose whether to include godhome bosses in the pool. Note: it will cause softlock if you die/kill the boss so using the keybind is required to exit (defualt: false)
  * `AllowTHK`: Choose whether to include THK in the pool. its defaulted to false because allowing a chance to end game early kinda boring.
  * `Reset Timer`: Resets timer back to max value.
  * `Only Spawn in Transitions`: Only makes you randomly teleport to transitions and not hazard respawns

Note: Any dream scenes cant be teleported to ever with exception of godhome scenes.  