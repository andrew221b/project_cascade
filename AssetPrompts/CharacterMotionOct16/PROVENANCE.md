# Character animation plates — 16 October 2026

Built-in `image_gen.imagegen`; no API/CLI fallback. Existing source sprites are unchanged. Packing/cropping only copies generated pixels into the runtime fragments; it does not repaint the original characters.

## Hidden fabric

Edit target: `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/PauseReferenceOct14/RestingMika.png`.

Prompt:

> Use case: precise-object-edit. Asset type: hidden clean plate for 2D character animation, transparent square sprite. Edit target is the attached seated mouse with apple, keep its exact camera, position, proportions, clothing, face, fur, legs, satchel and transparent background. Change ONLY the apple and the viewer-left foreground forearm and hand currently holding it: remove that apple, hand and foreground forearm, reconstruct the sage-green denim overalls and underlying shirt that would be hidden behind them. Keep the viewer-left upper sleeve at the shoulder, keep the right hand on the knee. The reconstruction must match the source's warm light, denim seams and texture precisely. Do not move the mouse or change its silhouette, head, face or either boot. No extra arm, no new props, no text. This is a technical clean plate: the original forearm with apple will be rendered over your reconstructed fabric and moved up toward the mouth. Full original canvas framing, transparent background; no shadow/backdrop, no decorative additions.

Selected output: `exec-d778a1b8-d779-4e8e-ab9b-ca72c9bd2921.png`, from the thread's generated-images directory. Runtime uses only normalized rectangle `(0.305, 0.440)–(0.590, 0.735)`, saved as `RestingCleanPlate.png` (358×370). Its shader samples it only behind the original arm mask; the original face, ears, clothing outside that mask, bag and boots remain original pixels.

## Resident eyelids

Four edit targets, `Journey/{name}.png`: `Pip`, `Rue`, `Aster`, `Residents`. The last is the existing 3×2 sheet. Transparent output for the three cutouts; original sheet background preserved. Exact prompt template (`{name}` substituted per request):

> Use case: precise-object-edit. Asset type: closed-eyelid animation plate for the existing {name} character art. Edit target is the attached image. Change ONLY the eyelids: close both visible eyes naturally in a relaxed blink, with a delicate curved eyelid crease that follows the original eye socket. Keep EXACT original eye locations, brow, head shape, identity, fur or feather texture, glasses and their lenses/frames (where present), nose/beak, mouth, lighting, camera, pose, costume, full canvas layout and every other pixel as closely as possible. For the six-character sheet, blink every one of the six characters, preserving the exact 3 columns by 2 rows layout and individual identity. No closed-eye smile, no squeezed grin, no relocation, no face reshaping. This will be sampled only inside each original eye region to animate a subtle blink. Preserve transparent background if the reference is a cutout; otherwise preserve its existing background. No text, no new elements.

Selected outputs, in target order: `exec-a7be3b58-6338-4369-98ba-b44f4678c440.png`, `exec-fda77de4-46a4-4094-a42b-c8349c2a7cd5.png`, `exec-19900667-4c8a-4ee7-ac03-0377b39c0b03.png`, `exec-f68d8d3e-f5a0-494d-9807-1f9213a1710e.png`.

Runtime `ResidentEyelids.png`: five columns × four rows of 128×128 fragments; two fragments per resident, near/far eye. Resident order: Pip, Rue, Aster, Bram, Sora, Neri, Toma, Lumi, Gran. Each patch covers 1.35 times the measured eye radii; shader feathering stays inside the original eye. Mika reuses the existing welcome eyelids instead.

## Resident articulation

Same four original edit targets and transparency policy. Exact prompt template:

> Use case: precise-object-edit. Asset type: gentle speaking mouth animation plate of the exact existing {name} character art. Edit target is the attached image. Change ONLY the mouth: part the lips or beak slightly in a natural friendly speaking pose, showing a small believable mouth interior. Very restrained opening, as during a soft short syllable, not surprise or laughing. Preserve each animal's original anatomy: frogs have a wide subtle soft mouth, fox/hedgehog/otter/mole/rabbit/mouse use their own muzzle and jaw; birds and owl part their original beak. Keep exact face identity, muzzle size, nose, open eyes, eye locations, glasses, head, expression, light, costume, body position and complete canvas layout. Keep every other pixel as closely as possible. For the six-character sheet make this small speaking mouth for all six, maintaining exact 3 columns by 2 rows layout. Do not add teeth unless already anatomically characteristic (two small incisors only on rabbit/mouse); no human lips, no giant black oval. The result is sampled only inside each original mouth/jaw region. Preserve transparent background if cutout; otherwise original background. No extra elements or text.

Selected outputs: `exec-8757ed86-f825-4391-92ce-e7dc81f75b4a.png`, `exec-1a15043b-47b6-4792-a779-15dd4d55a95b.png`, `exec-41467661-0c11-4b8a-a3d5-be3081cc9d36.png`, `exec-d0f382ae-18f8-42e7-bd4a-fe3c62593e8b.png`.

Runtime `ResidentMouths.png`: three columns × three rows, 128×128 per resident in the same order; source patches cover 1.2 times the mouth radii in `CharacterPoseProfile`. Mika reuses its original welcome mouth. Whole generated portraits remain outside runtime Resources. Source outputs are under `/Users/andrew/.codex/generated_images/01a1089e-7eaf-75a1-8066-dbfc48df27a1/`.
