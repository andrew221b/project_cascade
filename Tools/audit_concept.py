"""Counterfactual POC audit. Read-only source data; does not measure human enjoyment."""
from collections import Counter
from copy import deepcopy
from pathlib import Path
import json
import statistics

from cascade_model import from_dict, analyze, initial, step

ROOT = Path(__file__).resolve().parents[1]


def audit():
    rows = []
    for source in json.loads((ROOT / 'Data/levels.json').read_text()):
        level = from_dict(source)
        baseline = analyze(level)
        entry = initial(level)
        effects = []
        for family in ('bomb', 'key', 'repair', 'fragile', 'linked', 'entry', 'bridge'):
            changed = deepcopy(source)
            present = False
            for floor in changed['floors']:
                if family == 'entry':
                    present |= bool(floor['shift'] or floor['mirror'])
                    floor['shift'], floor['mirror'] = 0, False
                elif family == 'bridge':
                    present |= floor[family] is not None
                    floor[family] = None
                else:
                    present |= bool(floor[family])
                    floor[family] = [] if family == 'fragile' else None
            if not present:
                continue
            counterfactual = from_dict(changed)
            result = analyze(counterfactual)
            replay = initial(counterfactual)
            accepted_path = True
            for command in baseline['path']:
                after = step(counterfactual, replay, command)
                if after == replay:
                    accepted_path = False
                    break
                replay = after
            effects.append({
                'family': family,
                'without_solvable': result['solvable'],
                'without_minimum': result['optimal_actions'],
                'baseline_path_still_delivers': accepted_path and replay.terminal == 'won',
                'interpretation': 'Removing a mechanism can simplify the task; unchanged minimum alone is not proof of redundancy.'
            })
        rows.append({
            'id': source['id'], 'lesson': source['lesson'],
            'slots': len(level.start), 'floors': len(level.floors),
            **baseline,
            'on_entry_contacts': [name for name, active in (
                ('key', entry.key_used), ('repair', entry.repair_used),
                ('bomb_defused', entry.bomb_phase == 'defused'), ('stamp', bool(entry.marks))) if active],
            'counterfactuals': effects,
        })
    report = {
        'scope': 'POC/difficulty hypotheses from the complete Python state graph. C# parity is separately established in P07/P09. No human enjoyment claim.',
        'source_changed': False,
        'summary': {
            'levels': len(rows), 'solvable': sum(r['solvable'] for r in rows),
            'mean_minimum_commands': statistics.mean(r['optimal_actions'] for r in rows),
            'minimum_distribution': dict(sorted(Counter(r['optimal_actions'] for r in rows).items())),
            'fully_forced_optimal_graphs': [r['id'] for r in rows if r['forced_ratio'] == 1],
            'on_entry_contact_levels': [r['id'] for r in rows if r['on_entry_contacts']],
            'chapter_mean_minimum': [statistics.mean(r['optimal_actions'] for r in rows[i:i+10]) for i in range(0,40,10)],
        },
        'levels': rows,
    }
    output = ROOT / 'Reports/P11/concept_audit.json'
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps(report['summary'], ensure_ascii=False, indent=2))


if __name__ == '__main__':
    audit()
