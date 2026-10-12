"""Append the ninth world using existing rules and dictionary symbols only."""
from build_campaign_extension import ROOT, canonical, shortest
from cascade_model import *
import hashlib
import random

SEED = 26100709
R = random.Random(SEED)
LESSONS = ['observatory_mirror', 'observatory_shutter', 'observatory_fuse',
           'observatory_alignment', 'observatory_crossing', 'observatory_chart']
IDEAS = [
    'Prepare a reflected landing after the one-use crossing.',
    'Open the shutter before the paired star-chart exchanges.',
    'Choose the fuse exit and reserve the repair for the roller landing.',
    'Align paired symbols before opening the final observation shutter.',
    'Budget the fragile crossing before the reflected receiver approach.',
    'Carry the chart through a roller and a reflected landing.']


def candidate(family):
    n = 5 if family in (0, 4) else 6
    target = tuple('s:' + c for c in 'ABCDEF'[:n])
    start = list(target)
    R.shuffle(start)
    floors = []
    linked = R.choice([(0, 2), (1, 3)] if n == 5 else [(0, 2), (1, 3), (2, 4)])
    for index in range(3):
        edges = [True] * (n - 1)
        devices = {}
        if family == 0:
            if index == 0: devices['bridge'] = R.randrange(n - 1)
            if index == 1:
                devices['shift'] = R.choice([-1, 1])
                devices['repair'] = Sensor(R.randrange(n), R.choice(target), 1)
            if index == 2:
                devices['mirror'] = True
                devices['fragile'] = (R.randrange(n - 1),)
        elif family == 1:
            if index == 0:
                edge = R.randrange(n - 1)
                edges[edge] = False
                devices['key'] = Sensor(R.randrange(n), R.choice(target), edge)
            if index == 1: devices['mirror'] = True
            if index == 2: devices['linked'] = linked
        elif family == 2:
            if index == 0:
                devices['bomb'] = Bomb(R.randrange(n), R.choice(target), R.randint(2, 4), R.randint(2, 4))
            if index == 1:
                devices['shift'] = R.choice([-1, 1])
                devices['repair'] = Sensor(R.randrange(n), R.choice(target), 2)
            if index == 2: devices['fragile'] = (R.randrange(n - 1),)
        elif family == 3:
            if index == 0: devices['linked'] = linked
            if index == 1:
                edge = R.randrange(n - 1)
                edges[edge] = False
                devices['key'] = Sensor(R.randrange(n), R.choice(target), edge)
                devices['repair'] = Sensor(R.randrange(n), R.choice(target), 1)
            if index == 2: devices['mirror'] = True
        elif family == 4:
            if index == 0: devices['bridge'] = R.randrange(n - 1)
            if index == 1:
                devices['linked'] = linked
                devices['fragile'] = (R.choice(linked),)
            if index == 2: devices['mirror'] = True
        else:
            if index == 0:
                devices['bomb'] = Bomb(R.randrange(n), R.choice(target), R.randint(2, 4), R.randint(2, 4))
            if index == 1:
                devices['mirror'] = True
                devices['repair'] = Sensor(R.randrange(n), R.choice(target), 2)
            if index == 2:
                devices['shift'] = R.choice([-1, 1])
                devices['linked'] = linked
        floors.append(Floor(R.randint(2, 5 if n == 5 else 6), tuple(edges), **devices))
    return Level(tuple(start), target, tuple(floors))


def without_feature(level, family):
    floors = []
    for floor in level.floors:
        if family in (0, 4, 5): floor = replace(floor, mirror=False)
        elif family in (1, 3):
            edges = list(floor.edges)
            if floor.key: edges[floor.key.value] = True
            floor = replace(floor, key=None, edges=tuple(edges))
        else: floor = replace(floor, shift=0)
        floors.append(floor)
    return replace(level, floors=tuple(floors))


def append_sources(original_text, levels, bindings):
    """Retain incumbent bytes; compact additions avoid misleading repeated-block diffs."""
    level_end = '\n  ],\n  "bindings": ['
    binding_end = '\n  ]\n}'
    if original_text.count(level_end) != 1 or original_text.count(binding_end) != 1:
        raise RuntimeError('Unexpected extension representation; refusing to reformat incumbents.')
    def entries(rows):
        return ',\n'.join('    ' + json.dumps(row, ensure_ascii=False, separators=(',', ':')) for row in rows)
    output = original_text.replace(level_end, ',\n' + entries(levels) + level_end, 1)
    return output.replace(binding_end, ',\n' + entries(bindings) + binding_end, 1)


def run():
    path = ROOT / 'Data/campaign_extension.json'
    original_text = path.read_text()
    extension = json.loads(original_text)
    if len(extension['levels']) != 120 or len(extension['bindings']) != 360:
        raise RuntimeError('Expected the unchanged C41–C160 extension; refusing to regenerate published IDs.')
    original = json.loads((ROOT / 'Data/levels.json').read_text()) + extension['levels']
    seen = {min(canonical(from_dict(source)), canonical(from_dict(source), True)) for source in original}
    authored = []
    trials = 0
    for family in range(6):
        batch = []
        while len(batch) < 4:
            trials += 1
            if trials > 2000: raise RuntimeError('Candidate budget exceeded')
            level = candidate(family)
            key = min(canonical(level), canonical(level, True))
            if key in seen: continue
            try:
                minimum = shortest(level)
                if minimum is None or not (8 if len(level.target) == 5 else 9) <= minimum <= 13: continue
                removed = shortest(without_feature(level, family))
                if removed == minimum: continue
                metrics = analyze(level)
                if metrics['near_optimal_first_states_plus2'] < 2 or metrics['forced_ratio'] >= .9: continue
                # A late-world route should use more than one working floor.
                state = initial(level)
                active = set()
                for command in metrics['path']:
                    if command != 'D': active.add(state.floor)
                    state = step(level, state, command)
                if len(active) < 2: continue
            except RuntimeError: continue
            seen.add(key)
            batch.append((level, metrics, removed, key, family))
            print('family', family, 'accepted', len(batch), 'minimum', minimum, 'states', metrics['states'], flush=True)
        batch.sort(key=lambda item: item[1]['optimal_actions'])
        authored.extend([batch[0], batch[2], batch[1], batch[3]])
    levels = []
    reports = []
    for index, (level, metrics, removed, key, family) in enumerate(authored):
        node = f'C{161 + index}'
        levels.append(dict(id=node, title='Star Observatory ' + str(161 + index),
            lesson=LESSONS[family], rules_version='2.0', **asdict(level)))
        reports.append(dict(id=node, region='Star Observatory', route_idea=IDEAS[family],
            **metrics, without_feature_minimum=removed,
            structure_sha256=hashlib.sha256(key.encode()).hexdigest()))
    packs = {locale: json.loads((ROOT / f'Data/words_{locale}.json').read_text()) for locale in ['en', 'ru', 'uk']}
    bindings = []
    for index, source in enumerate(levels):
        level = from_dict(source)
        for locale, words in packs.items():
            choices = [word for word in words if word['equality_signature'] == list(range(len(level.target)))]
            word = choices[(index + 7) % len(choices)]
            mapping = {symbol: f'g:{locale}:{token}' for symbol, token in zip(level.target, word['tokens'])}
            floors = tuple(replace(floor, **{device: replace(getattr(floor, device), symbol=mapping[getattr(floor, device).symbol])
                if getattr(floor, device) else None for device in ['key', 'repair', 'bomb', 'stamp']}) for floor in level.floors)
            bound = Level(tuple(mapping[symbol] for symbol in level.start), tuple(mapping[symbol] for symbol in level.target), floors)
            bound.validate()
            bindings.append(dict(id=source['id'] + '_' + locale, node_id=source['id'], locale=locale,
                mode='words', word_ids=[word['id']], **asdict(bound)))
    output = append_sources(original_text, levels, bindings)
    path.write_text(output)
    (ROOT / 'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent/campaign_extension.json').write_text(output)
    report = ROOT / 'Reports/NightLevels'
    report.mkdir(exist_ok=True)
    (report / 'python_balance.json').write_text(json.dumps(reports, indent=2) + '\n')
    print('DONE', 'seed', SEED, 'trials', trials, 'nodes', len(levels), 'bindings', len(bindings), flush=True)


if __name__ == '__main__': run()
