# Motion Recorder
A Unity tool to record the motion of objects as an animation.

This tool was built to record physics simulations to an animation file to be played back at runtime. However, it will target any GameObjects assigned to so could be used for other applications.

Note that Motion Recorder is an editor tool and is not intended to be used at Runtime!

## How to use
- Drag the project folder into your Unity assets.
- I recommend you create a separate scene containing only the objects necessary to the recording, since the tool will simulate every physics object in the scene once you hit record.
- Create a GameObject containing any object you want to record. Since the recording will be saved as an Animation Clip, objects will only work properly if they are a child of the Motion Recorder. This will be the object you can add an Animation Controller to later.
- Assign the objects you want to be recorded to the Targets property on the Motion Recorder.
- Then you can hit record. Press record again to end the recording & hit save to create an Animation Clip in your assets folder.

Requires the Editor Coroutines package!