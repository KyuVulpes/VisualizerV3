# Visualizer V3
This is my third iteration on my Visualizer. The first one was primitive, second one got way to messy and started breaking
all over the place, this is meant to be the ultimate version of the visualizer I would enjoy on a daily bases.

# How to use it
You can use it however you like. In it's current state it does not have any changeable settings and thus you'd need to
recompile it. But do not worry as future builds should be able to have changeable settings, and on top of that, a wider
modding support to them.

# How to compile
To compile the project is very simple.
1. Download and install [Unity 2021.3.7](unityhub://2021.3.7f1/24e8595d6d43)
   1. That link will open Unity Hub, if you do not have Unity Hub installed, you can download the installer [here](https://unity3d.com/get-unity/download/archive)! 
2. Download the project.
   1. This can be done by either downloading a Zip version and extracting it.
   2. Or you can do it professionally by using git! `git clone https://github.com/KyuVulpes/VisualizerV3.git`
3. Make sure you have a license to Dynamic Bones (optional, but highly recommended).
4. Download CSCore and place it in `Assets/Plugins/CSCore`.
5. Open the project inside Unity.
   1. Inside Unity Hub is easy, as all you have to do it point Unity Hub at the folder.
   2. I don't know how to open it in a non-Hub version of Unity.
6. Once the project is finish loading everything, go to the top bar and select `Build`, then click `Build Everything`.

That is it, now you have built the project for yourself. This project isn't only just meant to contribute to it, but also acts
as an SDK.

## Making Expansions
If you wish to add your own avatar to your visualizer, it is simple. It's like making an avatar in ~~VRChat~~ ChilloutVR.
All you need to do is to make an object that has the component `Object Data`. When you add that to any object, upon selecting it
and going to `Build/Export to Package` will allow you to make the package. While it is making the package, do not be alarmed if you
see things get duplicated and a few files created then deleted. In the end, there should be 1 file left. This file should end in
`.pak`, if it doesn't, then something went wrong.

# LEGAL NOTICE
This project does include Dynamic Bones by Will Hong. If you do not have a license to use Dynamic Bones, go ahead and buy one in order
to use this project/SDK. I am not responsible for your actions in ignoring this notice. Secondly, while this project is Free and Open Source,
not all components are.
