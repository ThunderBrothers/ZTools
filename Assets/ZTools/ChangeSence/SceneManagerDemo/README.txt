Welcome to Scene Manager!
=============================

Thank you for buying Scene Manager. You can find the quick start tutorial and latest documentation
of Scene Manager on our website http://www.ancientlightstudios.com/scenemanager . 

If you require technical support you can shoot us a mail at support@ancientlightstudios.com .

Demo Project Overview
.......................

The demo project consists of an intro, a main menu and 4 levels. There are two scene configurations below
Resources/DemoConfig, one resembles a demo version of the game with only 2 levels, the other one the full version
of the game with all 4 levels. 

Running the project
......................

Select the FullGame scene configuration and make it active (unless it is already active). Then run Assets -> Verify
Scene Configuration to make sure the build settings are matching the selected scene configuration. Then open then Intro scene
and press play. The full version of the game is now running. The intro screen is shown and contains a button to go 
to the main menu. In the main menu you have the options to start a new game or reset the progress. Start a new game and
click through to level 3. Then return to the main menu. As you can see, the progress is tracked and the main menu
now offers a third menu entry which allows you to continue playing at level 3. If you want to reset your progress, press
the reset progress button.


Things to try out
.....................

Try to change the transition effect in the Main menu. Have a look at the SMDemoMainMenu class to see how the transition effect
is set at runtime. Try to activate the DemoGame configuration. Then start the Intro level again. You should now be able 
to walk through 2 levels of the game, only. Also notice that the level progress of the demo version is stored at a different 
location as the progress of the full version. Check SMDemoMainMenu class for examples on how to determine if you play the demo 
or the full version and show different menus for each. Finally try to modify the scene configurations or create your own.
