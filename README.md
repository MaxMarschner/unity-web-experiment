# Unity for Behavioral Web Experiments 
This repository provides resources for building behavioral experiments with the Unity game engine and deploying them on a [Jatos](https://www.jatos.org/) server for online data collection.

## Background
For one of my PhD projects, I built a behavioral experiment in Unity to investigate behavioral effects of being imitated by other people. 
In the experiment, participants control an animated avatar hand and respond to visual stimuli with simple finger lift responses. 
The experiment assesses how imitative responses by other avatars in the virtual scene influence participants' response performance.
Click here to try out a [demo of the experiment](https://jatos.psychologie.hu-berlin.de/publix/UTRIz5ZcSRX). 
If you are interested in the theoretical background of this project and the findings of the experiment, here is a link to the [manuscript preprint](https://doi.org/10.31234/osf.io/n4drs_v1). 

## What is in this repository and why should you care?
Unity is a powerful tool for building immersive, interactive virtual environments that can be leveraged to simulate "naturalistic" social interactions while maintaining control over experimental variables of interest.
As such, Unity offers researchers a promising tool to bridge the gap between careful experimental control and ecological validity.
In addition, many behavioral and psychological scientists are moving their data collection online, hosting their surveys and experiments on platforms like [Jatos](https://www.jatos.org/) and recruiting and testing large and diverse samples of participants on [Prolific](https://www.prolific.com/) or [MTurk](https://www.mturk.com/) literally overnight. Unfortunately, Unity was made for game developers, not for experimental psychologists conducting online behavioral experiments, so setting up behavioral experiments in Unity and deploying them on the web presents significant technical challenges. This repository showcases a working process to streamline this process.  

### Files in this repository

**`ExperimentHandler.cs`** --> Master script for experimental control in Unity, implementing:
- Experimental protocol: Block and trial structure, including randomization and counterbalancing of experimental variables.
- Performance tracking: Millisecond-accurate response time (RT) assessment and accuracy recording.
- Animations: Triggering visual events in synchronization with the task logic.
- Data handling: Local data structuring and preparation for server export.

**`JatosInterface.jslib`** --> JavaScript plug-in merging Jatos JavaScript functions into your Unity project

**`JatosInterface.cs`** --> C# wrapper script calling Jatos JavaScript functions from the plug-in to use them in your Unity project

**`Jatos WebGL Template`** --> Custom WebGL template to integrate Jatos functionality
- contains `index.html`, which is the essential bridge between your Unity WebGL build, the web browser, and your Jatos server. 

## How to host your Unity experiment on a Jatos server
This section serves as a comprehensive guide: You can either use the provided files as a 'plug-and-play' solution for your own project or follow the steps below to build the integration files yourself.  

### 1. Integrate Jatos JavaScript functions into your Unity C# scripts
To use Jatos JavaScript functions for sending experiment data to your Jatos server and for progressing to the next study component at the end of your Unity experiment, you must integrate Jatos’ JavaScript functions (stored in a jatos.js file on your Jatos server) into Unity's C# programming language. This involves the following steps:

#### 1.1. [Add a custom WebGL template](https://docs.unity3d.com/Manual/web-templates-add.html) to your Unity project that contains an `index.html` file with the following tweaks:
- Add a line to the `<head>` section to load jatos.js functions from server
- Create a wrapper function for each Jatos function you want to use in the <script> section: 
```
<html>
  <head>
  ...
  <script src="jatos.js"></script>
  ...
  </head>
  <body>
  ...
    <script>
    function sendResultData(jsString) {
      jatos.submitResultData(jsString);
    }
    ...
    </script>
  </body>
</html>
```

#### 1.2. Add a `Plugins` (case-sensitive) folder containing a `.jslib` file to your Unity `Assets` directory
- The .jslib file merges the wrapped Jatos functions defined in the index.html file to your Unity library:
```
mergeInto(LibraryManager.library, {
    // Save results to JATOS
    sendResultDataToJatos: function(utf8String) 
    {
        var jsString = UTF8ToString(utf8String);
        sendResultData(jsString);
    },
    
    // Move to the next JATOS block
    startNextJatosEvent: function() 
    {
        startNextJatosComponent();
    },
});

```

#### 1.3. Create a C# script in Unity to call and use the merged JavaScript functions in your project
- To use the merged functions in your Unity C# scripts, import the merged functions defined in the .jslib file via
```
[DllImport("__Internal")]
private static extern void sendResultDataToJatos(string data);
```
- Wrap the imported functions to use them in your C# scripts
```
public static void SendResultDataToJatos(string data)
{
    sendResultDataToJatos(data);
}
```
- To ensure your code runs and can be tested both within the Unity editor and as a WebGL build in your browser, add conditional compiler code:
```
public static class JatosInterface
{
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD

    [DllImport("__Internal")]
    private static extern void sendResultDataToJatos(string data);

    [DllImport("__Internal")]
    private static extern void startNextJatosEvent();

    // Static method to call the JavaScript function
    public static void SendResultDataToJatos(string data)
    {
        sendResultDataToJatos(data);
    }

    public static void StartNextJatosEvent()
    {
        startNextJatosEvent();
    }

#else
    public static void SendResultDataToJatos(string data)
    {
        Debug.Log(data);
    }

    public static void StartNextJatosEvent()
    {
        Debug.Log("Starting next Jatos component.");
    }
#endif
}
```
- Now, you can use `sendResultDataToJatos(data)` in your C# script. E.g.: `JatosInterface.SendResultDataToJatos(jsonExperimentData);`

### 2. Export your Unity Project as a WebGL build
To run your Unity experiment online, it must be exported as a WebGL build:

First, adjust your WebGL build settings:
- File > Build Settings > WebGL > Adjust Player Settings
  - Publishing Settings > tick “Decompression Fallback”
  - Resolution and Presentation > select your custom WebGL Template created in 1.1. > adjust Resolution to need

Now export your project as WebGL by clicking File > Build Settings > Build
- Choose destination folder on your hard drive 
- Wait until completion. Your environment might change doing the process. Don’t worry, everything will be fine after export completion. 
- The exported folder should contain the following content:
  - Build folder --> contains your Unity project as a set of browser-compatible files
  - index.html file --> HTML file to deploy your project in a web browser
  - other files (Unity icons, list of dependencies, etc.), not crucial for basic functioning

### 3. Setup study on Jatos 
- Launch Jatos on your local computer. 
- Click on “+ New Study” and choose study name (displayed in Jatos GUI) and folder name (displayed in study_assets_roots folder)
- Navigate to your study_assets_roots folder and copy all files created through the WebGL export into the new study folder 
- In Jatos GUI click on the newly created study and click + New Component, add a title (e.g., main experiment) and the path to your index.html file
- Click run and test, and if everything works fine, you can transfer your ready-to-run study from your local Jatos installation to your Jatos server






















