# AltspaceVR Programming Project - Unity Cursor

## Executables

The MacOS and Windows builds of this project are located in the Builds directory and are called `Cursor_Mac` and `Cursor_Windows` respectively.  `Cursor_Windows` was built from Unity on the Mac but has not been tested.

The Unity scene file was edited using Unity 5.6.0.

## Instructions

This version enhances the cursor from the reference implementation by providing the ability to teleport and pan around the scene.  When the cursor is aimed at the floor, a ring will display indicating that you can teleport to this location by left-clicking with the mouse.  Pointing at the furniture in the scene will cause it to highlight.  Moving the cursor towards the left and right edges of the screen will pan the camera.  The cursor height is limited to screen space.

It is also possible to manipulate the furniture in the scene.  By default, this is disabled by the `O` [(O)ff] keyboard key.  By selecting the `T` [(T)ranslate] key, the furniture can be translated across the ground by clicking and dragging.  Likewise, the `R`  [(R)otate] key can rotate the furniture and the `S` key can scale the furniture, again by clicking and dragging.  The action is stopped by releasing the mouse selection key.

## Implementation Details

The cursor rotation is handled using vector math to calculate the angle based on the player's forward direction and the cursor's rotation orientation.  If the angle is too great, it is restricted to a maximum amount to avoid rotating too fast.

Teleportation is handled by translating the First Person Controller in the scene, maintaining the height.  The rotation logic for the cursor inherits its position from the First Person Controller's transform.  

Teleportation logic is handled in the `TeleportModule.cs` script.  The rotation and cursor logic is in the `SphericalCursorModule.cs` script.

A custom solid shader was applied to the Cursor sphere which ignores lighting and makes sure that the Cursor always appears on top of all geometry.  This simple shader is handled in `CursorShader.shader`.  Another shader is applied to the ground ring for the Cursor module.  It uses a ring texture with an alpha channel.  This logic is handled in `GroundCursorShader.shader`.

The `TransformModeHandler.cs` class keeps track of the transformation state.

A minor syntax update was applied to the `LockCursor.cs` script since the previous calls were deprecated in Unity 5.


#Known Issues
- Sometimes furniture will fall through the ground after being released.
- The logic for selecting objects should be reimplemented if there is no floor intersection.