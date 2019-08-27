**<h1>IslandViz Augmented Reality (for HoloLens)</h1>**

![IslandViz Augmented Reality](/Docs/Images/Logo_IslandViz-1a_RGB.png)

 [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://github.com/DLR-SC/islandviz-hololens/blob/master/LICENSE)

HoloLens port of [IslandViz](https://github.com/DLR-SC/island-viz), a visualization tool for OSGi-based software projects using an island metaphor. The [Wiki](https://github.com/DLR-SC/islandviz-hololens/wiki) includes further information about the project. Instructions on how to synchronize the application across multiple HoloLenses or set it up for remote speech input processing can also be found in the Wiki.


<h2>Project Setup</h2>

This project is developed and built using <b>Unity 2018.1.6f1</b>. To import the cloned assets into a new Unity project, open the repository's root directory from within the Unity project loader. The application relies on external packages Triangle.NET and JSONObject. They are added as submodules and can be acquired from the repository.
```
git submodule init
git submodule update
```
Download the [Mixed Reality Toolkit 2017.4.0.0](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/2017.4.0.0) Unity package. Navigate to `Assets -> Import Package -> Custom Package…`  and import the package. Next, navigate to `Edit -> Project Settings -> Player`. Under 'Other Settings', make sure the Scripting Runtime Version is set to '.NET 4.x Equivalent' and under Scripting Backend select '.NET'. Under 'XR Settings' check 'Virtual Reality Supported'. Add the Windows Mixed Reality SDK.

 
<h2>Building the application</h2>

Under `File -> Build Settings… `, switch the target Platform to <b>Universal Windows Platform</b> . Click `Mixed Reality Toolkit -> Build Window`, and hit 'Build Unity Project'. This will yield a Visual Studio Solution with the compiled Unity project assemblies, ready for deployment to HoloLens. For detailed instructions on how to deploy using Visual Studio, [refer to the documentation](https://docs.microsoft.com/en-us/windows/mixed-reality/install-the-tools).
