# P15 — localization implementation note

5 October 2026. User-authorized EN/RU/UK UI and simple copy. English remains
initial UI and puzzle locale. Existing puzzle IDs, rules, hashes and packages
of word data stay pinned to each attempt; changing UI must not restart it.

Package exception: add Unity's `com.unity.localization` through PackageManager
Client.Add, with its required Addressables dependencies. This provides one
SelectedLocale source, authored String Tables and LocalizeStringEvent bindings.
No gameplay/network/monetization package is added. Record resolved versions
and actual validation below before P15 completion.

UI text uses complete phrases with format arguments, not translated fragments
of sentences. Word symbols and shape IDs never go through UI string tables.
A separate explicit word-language choice applies to the next started puzzle.
Corrupt external content must not silently substitute a different word; the
built-in compatible catalog remains available, with an explicit recovery result.

Translations are original editorial drafts. Automated glyph/overflow tests and
Editor inspection do not constitute review by native-language editors or a
physical-device test. Online remains design only.

## Implementation and evidence

Resolved pins: Localization 1.5.13, Addressables 2.8.1, Scriptable Build Pipeline
2.5.2. Installed through Unity PackageManager Client.Add; no other direct package
changed. LocalizationSettings.SelectedLocale and LocalizeStringEvent are the
runtime locale/binding source. `UiCopy` supplies complete keys/format arguments;
raw SymbolIds bypass translation. UI preference is only a future persisted mirror.
215 source keys, 645 non-empty entries in Data/ui_localization.tsv and Unity String
Tables. Shared art has an empty locale AssetTable collection; existing Resources
fonts are not duplicated into Addressables. Both fonts cover UI and word glyphs.

An explicit word-language confirmation changes the next new puzzle only. Existing
attempt locale/content/rules pins and exact symbols survive UI-language changes.
Importer rejects non-uppercase word data and corrupt packs; it never silently
substitutes another word. There is no external pack downloader in this phase.
P16 owns saved-attempt compatibility/recovery and the player-facing recovery offer.
NFC, supported reference graphemes, distinct Cyrillic letters and O versus circle
are tested. Universal extended emoji/ZWJ segmentation is not claimed.

Final EditMode 313/313 pass at 07:59:57–07:59:59 UTC; PlayMode 38/38 at
08:00:31–08:02:22 UTC on 5 October 2026. Real InputSystem tests cover confirmation,
cancel and next-start application. 39 native Editor captures cover 13 states in
each language, 320×693 and Words at 390×844. Two self-QA batches; empty joins no
longer show Unity's missing-string message, and Map Menu now has a live binding.
Active TMP overflow/missing-translation scan is empty in the final matrix.

Addressables StandaloneOSX content build succeeded (5.084 s); this also compiled
player scripts and exposed a Mac-only Handheld API error. The haptic hardware call
is now guarded to Android/iOS players, preserving Editor request counters. This
is not a complete player build or hardware vibration test. Unity generated URP
shader prefilter metadata during this build; rendering settings were not retuned.

No physical-device, native-language-editor, acoustic mix or human-enjoyment review
was run. EN/RU/UK text remains an original editorial draft. Evidence: Reports/P15.
