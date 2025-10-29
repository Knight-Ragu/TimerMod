# Overview

Timer mod for Airframe Ultra, for racing against yourself on the track!

Adds an in-game timer that automatically starts/stops when races begin/end

Keeps track of your best times for each map, and each bike on each map! Individual 'splits' as well as your fastest pace finishing each race

Works for beta 2 and 3, don't know about beta 1

# Installation

## BepinEx

In order to use this mod, BepinEx must be installed on the version of the game you wish to use it on

BepinEx has a lovely guide for installing it onto an IL2cpp game [here](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html)

And make sure to run the game once with BepinEx installed so it can set itself up!

## Acquire Dll

To run the mod, you need the plugin file ([`TimerMod.dll`](bin/Debug/net6.0/TimerMod.dll)), which once the project is built, is located in `bin/`

To build the mod yourself you must have the repository cloned/downloaded, get all the required dependecies listed in [`lib-names.txt`](lib/lib-names.txt), and place them in [`lib/`](lib), they are located in `BepinEx/interop/`, and `BepinEx/core/`

If you're feeling lazy, then you can just copy everything in those folders and paste them in

Then just open a terminal in the repo's root dirctory and run `dotnet build`

If you have dotnet installed then it will build the project and leave `TimerMod.dll` in [`bin/`](bin/)

## Done!

Take the .dll and place it in `BepinEx/plugins/`, launch the game and see if it works!

# Usage

All you need to do is start a game with the timer option enabled in the game settings!

If you want to look at your best times, they are saved in `BepinEx/plugins/` next to `TimerMod.dll`

# Issues

The largest issue with this mod is that it doesn't know if a quickfight happened or not, so if you skip one, and then do the next race, it never split for the quickfight finishline, and could overwrite the wrong splits!

This usually isn't a problem, since missing quickfights typically makes you slower, but it's something to be aware of

Generally I try to not complete races after missing a quickfight, but it really sucks since they're seemingly random :(
