TODO: list of tasks
- Double the Voxel shell thickness and double count of 3D objects, and use this to generate the slopes/corners/inversecorners instead of trying to interpret them.
- Refresh/Save needs to check if save directory no longer exists.
- tree view.
- Property Grid.
- Close Dialog on IsModified.
- Auto Updater, detect current and available version, prompt for update.
- Wix installer project,
  * Desktop Icon.
- View subscribed workshop Items (to allow exporting/copying from).
  * C:\Users\%Username%\AppData\Roaming\SpaceEngineers\WorkshopWorlds\*.sbw  These are .zip files.
  * It's not until the user presses "Copy & Load" that it's acually saved there.
  * The workshopId may or may not be present, depending on if it is the first version, or a subsequent upload by the author.


- Improve Import Image:
  * Preview.
  * Scale.
- Improve Import 3D Model:
  * Recode Poly2Vox to VisualC CLR to integrate with app, and enhance.
  * 3D Preview?

- Exit Pilot from ship to Astronaut.
- Set/Reset Astronaut Vector. (Direction and Speed)
- Move Astronaut to position.
- Switch Jetpack on/off.
- Switch Inertia Dampeners on/off.

- Import multiple ships/stations/objects from another 'world' as a group.  (Required to keep attached items. ie, Station/asteroid, Large Carrier/fighters.)
  Need a way to detect attached landing skids, or overlapping objects (sigh).  Check if landing gear activated?

- Replace Asteroid/Voxel.
- Move Asteroid/Voxel.
- Rotate Asteroid/Voxel (not supported by sandbox currently, but procedual voxel rotation could be accomplished.)

- Show ship status. Size. Item count. Weight. Thrust.
- Switch power on/off.
- Switch Inertia Dampeners on/off.
- Set/Reset Ship Vector. (Direction and Speed)
- Move Ship to position.
- Replace Armor light/heavy/color.


Completed:
- Check if SpaceEngineers Game is installed first.
- Refresh button, to reload selected Sandbox content.
- Change Astronaut color.
- Import Image.
- Import 3D model.
- Delete ship/station/object.
- Show Ship Pilot/Roaming Astronaut status.
- Repair ship.
- Import Asteroid/Voxel.
- Link to open Workshop ID.
- Add collection for temporary files, to allow cleanup during shutdown.


Dropped:
- Obfuscate/dotfuscator. Don't think it's worth it, considering I've gone open source. :D
- Text Unicode support. SpaceEngineers doesn't appear to support Unicode.
  * testing text
  * テキストをテスト



References:
Why are the blocks 0.5 and 2.5 meter blocks?
Sectors have the same size as in Miner Wars: 50x50x50km. It would take a couple of minutes to get from one border to the other.
http://spaceengineerswiki.com/index.php?title=FAQs


My experiments turning 3D models into in-game ships
http://forums.keenswh.com/post/my-experiments-turning-3d-models-into-ingame-ships-6588883?&trail=15


Python v3.1.5 documentation » The Python Standard Library »
http://docs.python.org/3.1/library/functions.html


ILMerge
http://www.microsoft.com/en-us/download/details.aspx?id=17630
http://research.microsoft.com/en-us/people/mbarnett/ILMerge.aspx
https://www.nuget.org/packages/ilmerge
ilmerge /target:winexe /out:SEToolbox.exe SEToolbox.exe SEToolbox.ImageLibrary.dll


Requirements:
*   7Zip - used in Build script on Setup project, to generate a .Zip of all release files without a .MSI.
    http://www.7-zip.org/

*	Windows Installer XML (WiX) toolset 3.7
	http://wixtoolset.org/releases/

*	Shaders:
	*	DirectX Software Development Kit 
		http://www.microsoft.com/en-au/download/details.aspx?id=6812

	*	Shader Effects BuildTask and Templates.zip
		https://wpf.codeplex.com/downloads/get/40167

	-	Documentation:
		https://wpffx.codeplex.com/

	-	Issues:
		http://blogs.msdn.com/b/chuckw/archive/2011/12/09/known-issue-directx-sdk-june-2010-setup-and-the-s1023-error.aspx
