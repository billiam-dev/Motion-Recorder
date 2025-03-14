# Motion Recorder
Lets you record complex motions (such as a physics simulation) to an animation clip in the Unity editor.

![](https://github.com/JARcraft2/motion-recorder/blob/main/preview.gif)

## How to use
- Drag the project folder into your Unity assets.
- Open up the demo scene.
- Create objects you want to record as children of the motion recorder. Since the output will be an Animation Clip asset, Motion Recorder will only capture the motion of children.
- Assign the objects to the Targets field on the Motion Recorder.
- When you hit record, Motion Recorder will simulate the scene physics in the editor. Press record again to finish and the targets will be reset to before the simulation.
- Then you can save out your recording.

If you are intending to use the Animation Clips with Unity's legacy animation, check MarkClipAsLegacy before recording.

Please note that Motion Recorder requires the Editor Coroutines package!