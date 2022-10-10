[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/SL2Bonfire?label=version)](https://github.com/mi5hmash/SL2Bonfire/releases/latest)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)

<img src="https://github.com/mi5hmash/SL2Bonfire/blob/main/.resources/images/SL2Bonfire Ahead.png" alt="HeroImage"/>

# :fire: SL2Bonfire - What is it :interrobang:
This app can **unpack and create SL2 archives** from/for various FromSofrware's games. It can also **resign saves** with your own SteamID or patch games memory to **bypass the SaveData ownership checks**. Finally it allows you to **rename your characters**.

## Supported titles*
| Game Title                               | Tested Version(s) |
|------------------------------------------|-------------------|
| Dark Souls Remastered                    | 1.03              |
| Dark Souls II - Scholar of the First Sin | 1.02              |
| Dark Souls III                           | 1.15-1.15.1       |
| Sekiro Shadows Die Twice                 | 1.06              |
| Elden Ring                               | 1.06              |

**the latest versions that I've tested and are supported*

# :scream: Is it safe?
**No.** You can corrupt your SaveData files and loose your progress or get banned from playing online if you unreasonably modify your save.

**First of all:** Always make backups of files you want to modify.

**Secondly,** do not patch game's memory while you're connected to the internet. Playing offline is not enough, you have to **disconnect the network cable or disable the network card on your device**.
Personally I'm using [Net Disabler](https://www.sordum.org/9660/net-disabler-v1-1/).

**Finally,** do not play online on save with modified stats. Just modifying your hero's name shouldn't lock you out from online play, unless you'll pick some offensive, too long or containing special characters (like: !/:?#) name.

You have been warned and now you can move on to the next chapter, fully aware of possible consequences.

# :scroll: How to use this tool

<img src="https://github.com/mi5hmash/SL2Bonfire/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Choosing Game Profile
First thing you always need to do is selecting a game profile **(1)**. Game profiles are located in the ***"profiles"*** folder inside the main program directory. Once you've selected the correct profile and the green verification indicator appears, you should see all the available options for that specific game.

## Unicode switch
The switch state will change automatically after unpacking the SL2 archive, to the one that has been used to pack the SL2 archive. In the Japanese version of the game, strings like character names or USER_DATA filenames are encoded with the "Shift JIS" encoding. Unfortunately, I have access only to the "Global" version of the games which use "Unicode" encoding, so I couldn't play with the other one. I had to take the code from the other developers and believe that if I implemented it right then it will be working fine here as well. Nevertheless, I **do not** recommend using my tool to convert the save region.

## Unpacking
If the game uses an encrypted SL2 archive, you will have 4 buttons divided into two rows **(3)**. The yellow ones are the ones you should use, unless you like to experiment, then feel free to play with the gray ones too.The files are unpacked into named folders (in this case it is ***".\\Dark Souls III\\unpacked"***). 

## Loading already unpacked files
Files are immediately loaded after unpacking. If you already have unpacked files in the ***".\\<GAME_TITLE>\\unpacked"*** directory, then you can load them by clicking the "Load" button **(4)**.

## Resigning files
You have to input your valid SteamID in SteamID64 format **(6)**. If you don't know how to find it then you can use [this site](https://www.steamidfinder.com). Once you have it typed in, just simply click the "Resign" button and unpacked files will be resigned.

## Changing character names
If the loaded game profile supports character renaming then you should see all created characters in this section **(8)**. The input fields will not allow you to rename empty slots or enter a longer name than the game allows. When you're done click the "Apply Renaming" button to apply changes.

## Packing
Once you have finished editing the USER_DATA files, you have to pack them back into an SL2 archive. Use yellow button from the "Packing & Unpacking" section **(3)**. Packed archives will be saved in the ***".\\<GAME_TITLE>\\packed"*** directory.

## Directory Shortcuts
You can open important folders in a new explorer window by using any of the three buttons in the "Directory Shortcuts" section **(7)**.

## Patching Memory
If your only goal is to load another player's SaveData and the resign feature is not working, you can try this approach. **!!! DISABLE INTERNET CONNECTION FIRST !!!** Once you run the game, you can click the "Patch" button and it will try to disable SaveData ownership checks. In this state, the game should agree to load the SaveData for you and sign it with your SteamID on the next saving operation.




