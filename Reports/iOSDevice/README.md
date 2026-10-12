# iPhone device build — 6 October 2026

Unity 6000.3.8f1 switched from Mac to iOS and exported the one-scene IL2CPP
project at `CascadeUnity/Builds/CASCADE-iOS`. BuildPipeline reported Succeeded,
68.336159 s, 0 errors, 346 warnings. Those warnings include installed Unity AI
inference shader variants and missing Localization iOS App Info metadata; this
report does not claim that iOS localization has been verified on a phone.

The generated Xcode app target uses `com.vickand.cascade`, display name
CASCADE, automatic signing and the user's Developer Team `F8297DBPDU` in all
four app configurations (Debug, Release, ReleaseForProfiling,
ReleaseForRunning). The Unity project's template iPhone ID was left alone.
Xcode 26.6 compiled the unsigned app first. An initial
signed build selected the wrong team and failed; after selecting the correct
team, Release was signed and built successfully. `codesign` reports bundle ID `com.vickand.cascade`
and TeamIdentifier `F8297DBPDU`. A separate Mac `codesign --verify --deep
--strict` check returned `CSSMERR_TP_NOT_TRUSTED`; despite that local trust
result, CoreDevice accepted, installed and launched the signed app on iPhone.

CoreDevice confirmed installation on the paired iPhone 17 and launched
`com.vickand.cascade`. A separate process query found CASCADE still running
with PID 46106. This verifies deployment and process start, not visual quality,
gameplay, haptics, frame rate, battery use or localization on the physical
screen. No screenshot or human playtest was captured in this pass.

Unity Cloud settings were not changed. Unity's unrelated temporary font, URP
and P15 report rewrites were restored after export. The generated iOS Addressables
content state and its `.meta` are preserved as build state. The Xcode export
and DerivedData under `CascadeUnity/Builds/` are generated ignored artifacts.
