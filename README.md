[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/SL2Bonfire?label=version)](https://github.com/mi5hmash/SL2Bonfire/releases/latest)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)

<img src="https://github.com/mi5hmash/SL2Bonfire/blob/main/.resources/images/SL2Bonfire Ahead.png" alt="HeroImage"/>

# :fire: SL2Bonfire - What is it :interrobang:
This app can **unpack and create SL2 archives** from/for various FromSofrware's games. It can also **resign saves** with your own SteamID or patch games memory to **bypass the SaveData ownership checks**. Finally, it allows you to **rename your characters**.

## Supported titles*
| Game Title                               | Tested Version(s) |
|------------------------------------------|-------------------|
| Dark Souls Remastered                    | 1.03-1.03.1       |
| Dark Souls II - Scholar of the First Sin | 1.02-1.03         |
| Dark Souls III                           | 1.15-1.15.1       |
| Sekiro Shadows Die Twice                 | 1.06              |
| Elden Ring                               | 1.06-1.07         |

**the latest versions that I've tested and are supported*

# :scream: Is it safe?
**No.** You can corrupt your SaveData files and lose your progress or get banned from playing online if you unreasonably modify your save.

**First of all:** Always make backups of files you want to modify.

**Secondly,** do not patch the game's memory while you're connected to the internet. Playing offline is not enough, you have to **disconnect the network cable or disable the network card on your device**.
Personally, I'm using [Net Disabler](https://www.sordum.org/9660/net-disabler-v1-1/).

**Finally,** do not play online on save with modified stats. Just modifying your hero's name shouldn't lock you out from online play, unless you'll pick some offensive, too long or containing special characters (like: !/:?#) name.

You have been warned and now you can move on to the next chapter, fully aware of possible consequences.

# :scroll: How to use this tool

<img src="https://github.com/mi5hmash/SL2Bonfire/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Choosing Game Profile
The first thing you always need to do is select a game profile **(1)**. Game profiles are located in the ***"profiles"*** folder inside the main program directory. Once you've selected the correct profile and the green verification indicator appears, you should see all the available options for that specific game.

## Unicode switch
The switch state will change automatically after unpacking the SL2 archive, to the one that has been used to pack the SL2 archive. In the Japanese version of the game, strings like character names or USER_DATA filenames are encoded with the "Shift JIS" encoding. Unfortunately, I have access only to the "Global" version of the games which use "Unicode" encoding, so I couldn't play with the other one. I had to take the code from the other developers and believe that if I implemented it right then it will be working fine here as well. Nevertheless, I **do not** recommend using my tool to convert the save region.

## Unpacking
If the game uses an encrypted SL2 archive, you will have 4 buttons divided into two rows **(3)**. The yellow ones are the ones you should use, unless you like to experiment, then feel free to play with the gray ones too. The files are unpacked into named folders (in this case it is ***".\\Dark Souls III\\unpacked"***). 

## Loading already unpacked files
Files are immediately getting loaded after unpacking. If you already have unpacked files in the ***".\\<GAME_TITLE>\\unpacked"*** directory, then you can load them by clicking the "Load" button **(4)**.

## Resigning files
You have to input your valid SteamID in SteamID64 format **(6)**. If you don't know how to find it then you can use [this site](https://www.steamidfinder.com). Once you have it typed in, just simply click the "Resign" button to resign unpacked files.

## Changing character names
If the loaded game profile supports character renaming then you should see all created characters in this section **(8)**. The input fields will not allow you to rename empty slots or enter a longer name than the game allows. When you're done click the "Apply Renaming" button to apply changes.

## Packing
Once you have finished editing the USER_DATA files, you have to pack them back into an SL2 archive. Use the yellow button from the "Packing & Unpacking" section **(3)**. Packed archives will be saved in the ***".\\<GAME_TITLE>\\packed"*** directory.

## Directory Shortcuts
You can open important folders in a new explorer window by using any of the three buttons in the "Directory Shortcuts" section **(7)**.

## Patching Memory
If your only goal is to load another player's SaveData and the resign feature is not working, you can try this approach. **!!! DISABLE INTERNET CONNECTION FIRST !!!** Once you run the game, you can click the "Patch" button so it will try to disable SaveData ownership checks. In this state, the game should agree to load the SaveData for you and sign it with your SteamID on the next saving operation.

# :sparkles: Extras
## CheatEngine Tables
For each game [there](https://github.com/mi5hmash/SL2Bonfire/tree/main/CheatEngineTables) is a CheatEngine Table which, depending on the game title, can:
* rename characters;
* retrieve the encryption key;
* disable the SaveData ownership checks;

Nonetheless, I wouldn't use this way to change character names.
That's because renaming characters with CheatEngine tables is a little messy. First of all, it doesn't have a name length restriction. I could only put a reminder that tells you what is the maximum length allowed by the game.
The second issue is when the previous character's name was longer than the new one then the extra characters won't get erased from the SaveData. For example, when you rename "JonSnow" to "Hodor", it will be displayed correctly in the game, but in the SaveData file it will be "Hodor ow". Moreover, Dark Souls 2 additionally stores the original character name, which my CheatEngine table cannot change.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issue (hope you won't) then please, feel free to report it [there](https://github.com/mi5hmash/SL2Bonfire/issues).
# :star: Sources:
* https://github.com/tremwil/DS3SaveUnpacker
* https://github.com/lepoco/wpfui
* https://newagesoldier.com/memory.dll
* https://github.com/CommunityToolkit/dotnet
