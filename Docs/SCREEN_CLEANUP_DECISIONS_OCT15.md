# Screen cleanup decisions — 15 October 2026

The user explicitly requested removing superseded screens and unused duplicate art. This is presentation cleanup: no price, product ID, reward, puzzle rule, save schema or persisted enum value changes.

The actual menu exposes Home → My things / Shop / Settings / Play, with the map's island browser, gameplay help, outcomes, story and account screens. Source-wide incoming-reference inspection found no current UI entry into the old Workshop, Contracts, Records or Lessons pages: only their public compatibility methods, their DrawMenu dispatch and historical tests/editor captures remain. The old journal still had an entry in the journey ending; it now opens the existing My things screen and returns to the ending.

Retire the four remaining obsolete page presentations by forwarding their compatibility entry points to the existing replacements: Workshop → Shop, Contracts and Lessons → Journey map, Records → My things. Keep enum members/numeric values, public method names and all progression/economy/record data. Test the redirects, back navigation and unchanged attempt/receipts before removing the now uncalled renderers. Current island level selection stays available.

For asset retirement, GUID checks alone are insufficient because Resources paths are built dynamically. The explicit 62-file plan is therefore tested in a disposable Unity project with those exact files absent before applying deletion to the working project. Keep a hash manifest and recovery archive outside Unity. Keep source/reference evidence, current shared sprites, all 54 world islands, current sky variants, comic panels, wildlife atlases and rebuild inputs.

Latest map clarification supersedes the earlier floating-button requirement: the action rests on the short brick landing; the mode selector sits above it. The landing is 7.5% of physical viewport height, including phones' gesture area. Controls respect the safe area. Header, island dimensions and per-world offsets remain approved.
