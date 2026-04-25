# SimpleFreeCam
A simple mod that adds a freecam to Lethal Company. This mod is intended to be used for creating screenshots or clips.

![Cinematic preview 01](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_cinematic_view_03.gif?raw=true)
![Cinematic preview 02](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_cinematic_view_01.gif?raw=true)
![Cinematic preview 03](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_cinematic_view_02.gif?raw=true)
![Cinematic preview 04](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_cinematic_view_04.gif?raw=true)

> As of version 1.4.0, the issue where held items appeared incorrectly in freecam has been fixed!

> The videos on this page are from an older version where the bug was still present

## Installation
1. Install `BepInEx`
2. Install `LethalConfig`
2. Install `LethalCompany InputUtils`
3. Drop 'BepInEx' **from the SimpleFreeCam.zip** into your Lethal Company folder.

## What it does
- Lets you fly a freecam around the scene independently of your player character
![Clip showing the freecam lock feature](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_explain_01.gif?raw=true)
- Scroll wheel adjusts the freecam movement speed
- Sprint key boosts camera speed while held
- Locking the freecam returns movement control to your player while retaining the view of the freecam
- Lets you change the freecam's FOV
![Clip showing the freecam FOV changing](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_explain_04.gif?raw=true)
- Fully works with the Company Cruiser
- Customizable freecam behaviour
- UI showing the freecam speed, FOV and other useful information
![Clip showing the distance warning](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_explain_02.gif?raw=true)
![Clip showing the reset transform warning](https://github.com/PixelIndieDev/SimpleFreeCam/blob/main/Docs/clip_explain_03.gif?raw=true)

## Controls
| Action | Default Key |
| ------------- | ------------- |
| Enable/Disable FreeCam (Toggle) | C |
| Enable/Disable FreeCam Player Movement (Toggle) | Z |
| Teleport FreeCam To Player | T |
| Scroll Changes FOV (Hold) | LeftAlt |
| Reset FOV | F |
| Hide UI | H |
| Increase / decrease camera speed/FOV | Scroll wheel (Up - Down) |

## Configuration
All settings are available in the LethalConfig menu in-game.

### Transform
| Setting | Default Key |
| ------------- | ------------- |
| Reset freecam transform on freecam | false |
| Reset freecam when too far away | false |
| Max freecam distance | 100 |
### Speed
| Setting | Default Key |
| ------------- | ------------- |
| Reset speed on freecam | false |
| Default freecam speed | 5.0 |
### UI
| Setting | Default Key |
| ------------- | ------------- |
| Opacity | 0.9 |
| Is UI visible by default | true |
| Reset UI visibility on freecam | false |

> All players in the lobby are required to have the `SimpleFreeCam` mod installed. Clients without the mod will receive an generic "an error occurred" message and be sent back to the main menu.

## Compatibility
Works for V81

This mod should work alongside most other mods.