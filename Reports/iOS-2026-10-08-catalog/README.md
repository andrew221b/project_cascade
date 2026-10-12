# Catalogue-flow iPhone update — 8 October 2026

Unity 6000.3.8f1 exported the corrected shop/trophy/tool-bag UI for iOS IL2CPP: BuildReport Succeeded, 0 errors, 346 warnings (`unity-build.txt`; principally existing iOS localization metadata and Sentis/Metal shader warnings). Xcode 26.6 Release workspace build succeeded (`xcode-build.log`). The generated app configurations use development signing and `com.vickand.cascade` with display name CASCADE. The permanent Unity bundle identifier remains a release task.

CoreDevice installed the app over the existing CASCADE copy on the paired iPhone 17 (`install.json`: success). No uninstall or save reset was requested. The immediate launch attempt failed because the iPhone was locked (`launch.log`, CoreDevice 10002). Launch and on-device interaction remain pending until the phone is unlocked. Native Unity Editor captures and flow assertions passed in `Reports/CatalogFlow-Oct08`.
