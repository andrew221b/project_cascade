# CASCADE iPhone device build — 6 October 2026

The requested device build uses Unity 6000.3.8f1 iOS IL2CPP and the single
CascadePlayable scene. Export to `CascadeUnity/Builds/CASCADE-iOS`; preserve
existing Mac apps and the project’s current content, packages and save data.

The Unity project still contains its inherited template iPhone identifier.
For this local device installation, sign the generated Xcode target as
`com.vickand.cascade` with automatic development provisioning and display name
CASCADE. The Apple Developer Team `F8297DBPDU` is selected in every app target
configuration. This is a device build identifier, separate from ShapeMania’s installed
app. No game IDs, prices, rules or profile schema change in the Unity source.

Record the Unity export and Xcode compilation results separately. Installation
and launch must be confirmed by the paired iPhone, not inferred from a built
Xcode project. Physical-device performance and human play quality require their
own observation.

## Current export — 7 October 2026

Build the current source (8adca9c) once into
`CascadeUnity/Builds/CASCADE-iOS-2026-10-07`; keep the previous export
`CascadeUnity/Builds/CASCADE-iOS` and its DerivedData. Use the generated
`Unity-iPhone.xcworkspace` so the LevelPlay CocoaPods dependencies are included.
The same app identifier and team above apply. Xcode generated a specific
CASCADE development profile including In-App Purchase and the paired iPhone;
no capability, package or source setting was disabled to bypass signing.
`testMonetization=1`, the real store catalog is unregistered, and native ad
keys are empty. Deployment does not validate production payments or ads.

## Current export — 8 October 2026

The working tree after the route-record layout revision was exported to
`CascadeUnity/Builds/CASCADE-iOS-2026-10-08` and built from its
`Unity-iPhone.xcworkspace` with Xcode 26.6. The signed app retained
`com.vickand.cascade`, was installed and launched on the paired iPhone 17.
The previous exports and the app's existing device data were preserved.
See `Reports/iOS-2026-10-08/README.md` for the exact build and device evidence.
