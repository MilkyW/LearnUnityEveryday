# iOS Develope

## Build Environment & Permission
![Apple Develop](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/Apple%20Develop.png?raw=true)

**Apple Developer account:**

[Building iOS Application](https://confluence.hq.unity3d.com/display/QA/Building+iOS+Application)

**macOS, Xcode and iOS SDK:**

**Xcode 10.1:** highest for macOS 10.13.x

[Xcode_10.1.xip](https://drive.google.com/file/d/1L0A81DdYBrOUYlG1Xi1C49_uSvSG_71l/view?usp=sharing)

[Xcode_10.1.xip](https://download.developer.apple.com/Developer_Tools/Xcode_10.1/Xcode_10.1.xip)

**Older Xcode with Newer iOS:** 

![Xcode iOS intallation dictory](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/Xcode%20iOS%20intallation%20dictory.png?raw=true)

**1. Copy newer iOS SDK to Xcode Intallation Dictory**

/Developer/Platforms/iPhoneOS.platform/DeviceSupport

![SDKSettings](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/SDKSettings.png?raw=true)

**2. Add newer iOS SDK version to SDKSettings.plist**

/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/IphoneOS.sdk/SDKSettings.plist

![Deployment](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/Deployment.png?raw=true)

**3. Restart Xcode and choose deployment target in targets settings**

**iOS SDK:** copy from higher versions of Xcode

## Integrating Unity Project into Xcode Swift Project

To automatic refresh Xcode Swift project when built-in Unity project is re-built.

[swift-unity · GitHub](https://github.com/jiulongw/swift-unity)

**1. Open Unity project, open Demo scene, open Build Settings and switch to iOS platform**

![XcodePostBuild](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/XcodePostBuild.png?raw=true)

**2. Click on Tools/SwiftUnity in manu and put in Xcode Project File in Settings**

**3. Build and output to some temporary folder such as /tmp.**

**4. Once build succeeded, open Xcode project, select target device (Unity does not support x86_64) and hit Build and then run. Change bundle identifier if Xcode has problem creating provisioning profile.**
