# Taskbar Legends

A Solo Leveling themed idle RPG that lives in a small strip above the Windows taskbar. A party of
heroes fights its way down an endless dungeon on its own while you work - you only step in to pick
an upgrade when they level up.

![Gameplay](Docs/screenshot.png)

## Features

- **Idle auto-battle** - a side-on pixel-art stage: the party walks the dungeon, swings, shoots and
  casts at whatever comes next, with damage numbers, crits, hit sparks and a scrolling parallax
  background.
- **A party of five** - start as a lone Warrior and recruit an Archer, Mage, Priest (heals the most
  hurt member) and Rogue (two strikes a turn, frequent crits) as you level.
- **Level-up choices** - every level offers three cards: more attack or HP for one hero or the whole
  party, crit chance and damage, regeneration, gold, EXP, speed, or a new recruit. Not looking? The
  first card is taken for you after 12 seconds.
- **Floors and bosses** - ten kills per floor, then the floor's boss. Creatures grow stronger and new
  kinds appear the deeper you go, with the odd Elite among them.
- **Ranks E to S** - the System announces every level, rank and floor you reach.
- **Offline rewards** - come back to the kills, EXP and gold your party earned while the game was closed (up to 8 hours).
- **Shop** - spend gold on permanent upgrades: attack, HP, crit, regeneration, gold and EXP.
- **Shadow army** - defeated bosses may rise as shadow soldiers ("Arise!"), each adding party attack and standing behind your heroes.
- **Reawakening** - from floor 10, start the climb over for Essence, a permanent bonus to attack, HP, gold and EXP. The shop and the shadows stay.
- **Taskbar mode** - a borderless, always-on-top 640x240 window docked above the taskbar. Drag it
  anywhere; it keeps fighting while another window has focus. Settings let you change its opacity, turn
  always-on-top off, start it with Windows, or hide it to the notification area.
- **Progress is saved** after every kill.

## Running it

- **Unity:** 6000.3 (Unity 6). Open the project and play `Assets/Modules/MainModule/Scenes/MainScene.unity`.
  In the Editor the game plays in the Game view; the taskbar docking only happens in a Windows build.
- **Windows build:** *File > Build Profiles*, Windows, Build. Addressables content is built with the player.

## Architecture

Built on [FlowIoC](https://github.com/BirrStudio/FlowIoC), a signal-driven IoC framework: the game is
split into modules that never reference each other and meet only in a Connector. The rules the code
follows are in [AGENTS.md](AGENTS.md); each module describes itself in its `MODULE.md`.

| Module | What it does |
|---|---|
| `GameplayModule` | The battle loop, party, upgrades, floors and saving. Balance lives in `CD_Gameplay`. |
| `GameplayScreenModule` | The stage, the HUD and the level-up panel. |
| `DesktopWindowModule` | A service that docks and drags the borderless window (user32). |
| `MainModule` | Boots the game and docks the window. |
| `ConnectorModule` | Wires the modules' signals together. |

## Credits

The pixel art (heroes, creatures, effects and the dungeon) was generated in code for this project.
