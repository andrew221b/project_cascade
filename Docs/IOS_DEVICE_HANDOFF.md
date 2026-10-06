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
