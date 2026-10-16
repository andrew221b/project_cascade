# Victory animation — 12 October

Presentation of an already committed receipt. All three empty stars arrive in sequence (0.20 s start, 0.22 s stagger, 0.52 s flight); no gold appears during arrival. Filling begins at 1.50 s, after a short all-empty hold, proceeds left to right with a 0.42 s stagger and a 0.50 s sweep per star. Score, coins, lanterns, active time, moves and undos count up from zero using the same eased clock and settle before the 3.20 s ceremony ends. Reduced motion presents final values immediately; background time pauses animation.

No reward grant or puzzle transition comes from the animation. User explicitly requested real 1.5/2.5 ratings. New rating version 3 stores the legacy whole-star integer plus an omitted-by-default HalfStar flag. Clear receipts and replay credits use matching additive flags; absent flags preserve old JSON hashes and ratings. Old completed results are not rescored. Map/browse totals include halves.

New deterministic thresholds retain the previous whole-star policy: 3 for a fast clean optimal run with no undo; 2.5 for a clean unassisted optimal run with at most one undo when 3 is not earned; 2 for the prior allowance/undo conditions; 1.5 for a clean unassisted run within twice the extra-move allowance and at most three undos; 1 otherwise. Score awards 50 points per half-star. First-clear/parcel prices and reward schedules stay intact; replay credit is two coins per full star, hence one per half, recorded as a maximum per node. Server replay validation and cloud merges compare exact half-units.

Cloud code is currently disabled in authentication.json; updated verifier/module is checked locally. Deployment/device verification remains a separate activation requirement.
