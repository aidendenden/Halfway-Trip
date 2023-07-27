//
// Virtual Snapshot
// A GUI and script package to include a virtual photographic camera in your game
// Copyright 2016 While Fun Games
// http://whilefun.com
//

===================
>Setup Instructions:
===================

1) Create an empty scene, add a plane and First Person Controller prefab
2) Add SnapshotUICamera prefab to scene anywhere, place it at coordinates (0,0,0)
3) Add CameraFlashAndSounds prefab as a child object of your main player character (e.g. if using First Person Controller, set as child of that Game Object)
4) Drag in Virtual Snapshot prefab that corresponds to the camera type you want to use (DSLR, FilmSLR, Disposable)
5) In VirtualSnapshot Game Object, set Snapshot Camera in Inspector to be your main game camera (e.g. if using First Person Controller, use that Main Camera)
6) In SnapshotUICanvas child object, set Canvas's Render Camera to be SnapshotUICamera
7) In CameraTransitionCanvas child object, set Canvas's Render Camera to be SnapshotUICamera
8) Run scene, press C to show camera, and T to take pictures. Pictures are stored in the snapshots folder.

Refer to the demo scenes for reference on how the prefabs are set up.

Note:
----
DemoScene.unity is set up for the Digital SLR version of the camera. See OneTimeUseDemoScene and FilmSLRDemoScene for their how those respective camera types are used.

If in the editor the camera border looks weird, ensure "16:9" ratio is selected in the Game preview window.

==============
>Photo Storage:
==============

Note that the photos are stored in a folder called "snapshots". If you are running in the Unity Editor, this folder is created in the Assets folder. If you are
running a build on windows, the snapshots folder defaults into the "..._Data" folder of the demo executable. This can be customized in VirtualSnapshotScript
as desired for your target platform. See script and code comments for more details.


===============
>Graphics Guide:
===============
Note:
----
It is mandatory that all GUI textures are imported as Texture Type 
"Sprite(2D and GUI)", otherwise they will likely display incorrectly.


To switch or insert your own custom graphics:
1) If desired, create the following graphics using the provided templates.
2) Replace the following Sprite variables in the Inspector for VirtualSnapshotDSLR prefab child Canvases:

For Example:

>VirtualSnapshotDSLR Prefab, SnapshotUICanvas child:
---------------------------------------------------

-CameraUIBackground
-Battery Level
-Flash Indicator
-Light Meter


============================
>Graphical Element Breakdown:
============================

CameraUIBackground: The main UI; what you see when you look through the viewfinder when using the camera.
Battery Level: 5 Icons representing the charge levels of the battery.
Flash Indicator: 2 Icons representing On/Off states of the flash
Light Meter: A decorative element that represents the camera's light meter

Please see the Textures > Templates folder for the 4 included Photoshop templates.

======================
>Included Camera Types:
======================

Digital SLR:
-----------
-Modern UI and graphics
-Digital SLR sounds
-No film, no image capacity limits

Film SLR:
--------
-Slightly less modern UI and graphics
-Film SLR sounds, including automatic film wind after each exposure
-24 exposures

One Time Use ("Disposable") Camera:
----------------------------------
-Basic UI and graphics
-Crunchy plastic sounds including analog film wind
-No Zoom
-27 exposures


===================
Other Package Notes:
===================

-All Prefabs in the Prefabs > DemoPrefabs folder (e.g. "demoLevelHelper") are not critical for this package. 
-All assets in the Materials > DemoMaterials folder are for demo only, and are not critical for this package.
-All Scripts in the Scripts > DemoScripts are for demo and code reference only, and are not critical for this package.
-All Images in the Textures > DemoTextures are for demo only, and are not critical for this package.

Feel free to delete the above items as you see fit.




If you have any problems using this package, or have tweaks or new features you'd like to see included, please let me know.

Thanks,
Richard

@whilefun
info@whilefun.com



THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


