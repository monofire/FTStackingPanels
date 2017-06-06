# FTStackingPanels

FTStackingPanels is a [Unity 3D](https://unity3d.com) framework. It is created to design and manage game UI as the collection of presentable panels. Feedback events allow you to track presentation state and setup panel's content and animations accordingly.

![ftstackingpanels](https://cloud.githubusercontent.com/assets/25864772/26696204/3cd6d9a4-4716-11e7-80db-765f617bf7f9.gif)


## How To Get Started
- [Download FTStackingPanels](https://github.com/monofire/FTStackingPanels/archive/master.zip) project and try it out.
- Follow instructions below.


## Architecture:
![screen shot 2017-06-06 at 21 15 24](https://user-images.githubusercontent.com/25864772/26844830-60f4a114-4afd-11e7-8464-45c73f626619.png)

### UIPanel

`UIPanel` is designed to host all UI elements and has 4 stages of presentation flow. 

**UIPanel event callbacks**   
Each stage is represented with corresponding callback as described below. 
Transition to the new stage is managed by `UIPanelsStack`. Every callback has a handler which allows moving to the next stage only when it is relevant (useful while dealing with asynchronous operations).

* * *
```c#
void onWillDisappear (Action onWillDisappearFinished)
```
Called when the panel is about to disappear from the visible area.
*Use this callback to provide your response to event and prepare to the next panel stage* *e.g save UI state to model*

`onWillDisappearFinished()` handler should be called to continue with next presentation stage.

* * *

```c#
void onDidDisappear (Action onDidDisappearFinished)
```
Called when the panel has already disappeared from the visible area.
*Use this callback to reconfigure UI objects on panel, release resources, start of stop game process, etc*

`onDidDisappearFinished ()` handler should be called to continue with next presentation stage.

* * *
```c#
void onWillAppear (UIPanel sender, Action onWillAppearFinished)
```
Called when the panel is not visible but is about to be shown.
*Use this callback to prepare UI objects to be presented, recover UI state from model*

`sender` provides name of game object which initiated this panel transition.   
`onWillAppearFinished ()` handler should be called to continue with next presentation stage.

* * *
```c#
void onDidAppear (UIPanel sender, Action onDidAppearFinished)
```
Called when the panel has already appeared on the screen.

`sender` provides name of game object which initiated this panel transition.   
`onDidAppearFinished ()` handler should be called to continue with next presentation stage.
* * *

**UIPanel animation objects**   
Presentation flow of UI panel is managed by two animation objects.
The first object animates how panel appears on screen while the second object 
animates how panel disappears from visible area.
It is up to you what animations to use. However, you must declare explicitly 
start triggers of your animations in the following methods:

```c#
   void startAppearAnimation ()
   {
      /* set trigger that starts present animation */
      _animator.SetTrigger ("presentPanel");
   }

   void startDisappearAnimation ()
   {
      /* set trigger that starts hide animation */
      _animator.SetTrigger ("hidePanel");
   }
```

### UIPanelsStack

`UIPanelsStack` manages UIPanels transitions and routes events between different UIPanels.
***
***Constants***

```c#
 const string _UIPanelTag
```
Used to recognize `UIPanel` among other objects in the hierarchy.

```c#
  const string _initiallyPresentedPanelName
```
Used to recognize first `UIPanel` presented to a player.

```c#
  const string _alwaysActivePanelName
```
Used to recognize `UIPanel` that should be always active
 (pls. see UI Panels and performance).
***
***Attributes***
```c#
  UIPanel previousPanel
```
Points on previously presented `UIPanel`.

```c#
  UIPanel currentPanel 
```
Points on currently presented `UIPanel`.

```c#
  Dictionary <string, UIPanel> panels  
```
Contains pointers on all panels and panels names.


***
***Methods***

```c#
void presentTarget (string targetName, 
                    Action onTransitionToTargetFinishedNullable)
```
Presents target panel without transition to other panel.   
`onTransitionToTargetFinishedNullable ()` handler is called when transition is finished (optional) 
 * * *
```c#
    void hideTarget (string targetName, 
                     Action onHideTargetTransitionFinishedNullable)
```
Hides target panel.   
`onTransitionToTargetFinishedNullable ()` handler is called when transition is finished (optional)
 * * *
```c#
    void present (string senderName, 
                  string targetName, 
                  Action onTransitionToTargetFinishedNullable)
```
Hides sender panel and presents target panel.   
`onTransitionToTargetFinishedNullable ()` handler is called when transition is finished (optional)
* * *

***UI Panels and performance***   
It is suggested to notify UIPanelsStack when you are using / not using your UI panels on Scene,  to free up resources and decrease number of draw-calls. 

 * * *

```c#
    void activateUIPanels() 
```
Activates all panels.

* * *

```c#
    void activateUIPanels() 
```
Deactivates all panels except the one stated in `_alwaysActivePanelName` const.
* * *


## Usage

1. Scene setup   
1.1 Set Main Camera's "Projection" attribute  to "Orthographic".   
1.2 Create and add Canvas game object as a child of the Main Camera.   
1.3 Add UIPanelStack.cs script to the Canvas object.   
1.4 Create and add Panel object as a child of Canvas.   
1.5 Subclass `UIPanel` and add this subclassed script to your panel.   



2. UIPanelsStack.cs script setup   
2.1 Set `_UIPanelTag` const with your panel tag e.g "UIPanel".   
Set this tag for all your Panels, to allow UIPanelStack.cs script recognize and manage all Panel objects.   
2.2. Set `_initiallyPresentedPanelTag` const with the name of the panel that will be presented to the player in the first order e.g. "MainMenuPanel".    
2.3 Set `_alwaysActivePanelName` const with the name of the panel that should be always active e.g. "GameScenePanel".   
(Please see ***UI Panels and performance*** for the detailed explanation).   


3. Panel animations setup   
3.1 Add Animator to the Panel in order to manage panel's "hide" and "present" animations.   
3.2 Create animation controller and your "hide" and "present" animations.   
3.3 Add corresponding triggers e.g. "presentPanel" and "hidePanel" to your animations.   
This triggers will be called from your UIPanel subclass.   
3.4 Add an event in the end of your "present" animation and link it to the onAppearAnimationFinished() method of UIPanel.   
3.5 Add an event in the end of your "hide" animation and link it to the onDisappearAnimationFinished() method of `UIPanel`.  
![screen shot 2017-06-05 at 19 54 38](https://user-images.githubusercontent.com/25864772/26844747-23538cd0-4afd-11e7-821f-bcc55dfbc591.png)



4. UIPanel.cs script setup   
4.1 override `startAppearAnimation ()`method in your `UIPanel` subclass: "presentPanel" animation trigger should be called from this method.   
e.g `_animator.SetTrigger ("presentPanel");`   
4.2 override `startDisappearAnimation ()` method in your `UIPanel` subclass:   
"hidePanel" animation trigger should be called from this method.   
e.g `_animator.SetTrigger ("hidePanel");`   
4.3 Override UIPanels's methods described in ***UIPanel event callbacks*** section and use them according to your tasks.   

Repeat steps 1.3 - 3.2 for all the number of panels you plan to use.


## License
This project is licensed under the MIT License - see the LICENSE.md file for detail
