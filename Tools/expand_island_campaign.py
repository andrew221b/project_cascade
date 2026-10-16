"""Append distinct consolidation puzzles without rewriting any incumbent definition."""
from build_campaign_extension import ROOT, canonical
from build_star_observatory import append_sources
from cascade_model import *
import hashlib
import random
import time

SEED = 26100712
R = random.Random(SEED)
OLD_STARTS = [0, 15, 30, 40, 64, 88, 112, 136, 160, 184]


def identity(level):
    # More durability alone does not make another puzzle.
    def normalized(mirror):
        value = json.loads(canonical(level, mirror))
        for floor in value['floors']: floor.pop('hp')
        return json.dumps(value, sort_keys=True)
    return min(normalized(False), normalized(True))


def prove(level, upper, cap=18000):
    root = initial(level)
    queue = deque([root])
    previous = {root: None}
    depth = {root: 0}
    while queue:
        state = queue.popleft()
        if state.terminal == 'won':
            path = []
            while previous[state] is not None:
                state, command = previous[state]
                path.append(command)
            return path[::-1], len(previous)
        if state.terminal or depth[state] >= upper: continue
        for command in commands(state):
            result = step(level, state, command)
            if result == state or result in previous: continue
            if len(previous) >= cap: return None, len(previous)
            previous[result] = state, command
            depth[result] = depth[state] + 1
            queue.append(result)
    return None, len(previous)


def variant(source, world):
    level = from_dict(source)
    start = list(level.target)
    R.shuffle(start)
    floors = []
    for floor in level.floors:
        updates = {'hp': min(12, max(1, floor.hp + R.choice([-1, 0, 1, 2])))}
        for device in ['key', 'repair', 'bomb', 'stamp']:
            item = getattr(floor, device)
            if item:
                updates[device] = replace(item, slot=R.randrange(len(start)), symbol=R.choice(level.target))
        floors.append(replace(floor, **updates))
    if world > 0 and len(floors) < 3 and R.random() < .72:
        for _ in range(R.randint(1, 3-len(floors))):
            floors.append(Floor(R.randint(1, 3), (True,) * (len(start)-1)))
    if world == 0 and not any(f.shift or f.mirror or f.key or f.bomb or f.linked or f.fragile or f.bridge is not None for f in floors):
        # Ordinary intermediate boards introduce no additional mechanic.
        floors = [Floor(R.randint(1, 3), (True,) * (len(start)-1)) for _ in range(R.randint(1, 3))]
    return Level(tuple(start), level.target, tuple(floors))


def band(world, island, index):
    if world == 0: return [(2, 4), (3, 5), (4, 6), (5, 7), (5, 8)][island]
    if world == 1: return (5, 8) if island < 2 else (6, 9)
    if world == 2: return (7, 10)
    # Relief beats vary the rhythm without raising the eventual difficulty ceiling.
    return (6, 9) if index % 4 == 2 else (8, 12)


def run():
    path = ROOT / 'Data/campaign_extension.json'
    original_text = path.read_text()
    extension = json.loads(original_text)
    if extension.get('journey_order'): raise RuntimeError('Already expanded; refusing to rewrite published nodes.')
    incumbents = json.loads((ROOT/'Data/levels.json').read_text()) + extension['levels']
    assert len(incumbents) == 184
    seen = {identity(from_dict(source)) for source in incumbents}
    route, additions, proofs = [], [], []
    trials = 0
    for world in range(9):
        first, last = OLD_STARTS[world:world+2]
        for island in range(5):
            begin = first + ((last-first)*island+4)//5
            end = first + ((last-first)*(island+1)+4)//5
            core = incumbents[begin:end]
            route.extend(source['id'] for source in core)
            templates = [s for s in core if len(s['start']) <= 6]
            previous = [s for s in incumbents[:begin] if 4 <= len(s['start']) <= 6][-16:]
            templates += previous
            if not templates: templates = [s for s in incumbents[:end] if 4 <= len(s['start']) <= 6][-8:]
            for index in range(12-len(core)):
                lower, upper = band(world, island, index)
                for attempt in range(2000):
                    trials += 1
                    source = R.choice(templates)
                    level = variant(source, world)
                    key = identity(level)
                    if key in seen: continue
                    witness, states = prove(level, upper)
                    if witness is None or len(witness) < lower: continue
                    if not any(c != 'D' for c in witness): continue
                    seen.add(key)
                    node = 'C' + str(185+len(additions))
                    anchor = int(source['id'][1:])
                    row = dict(id=node, title=source['title'], lesson=source['lesson'], rules_version='2.0',
                               teaching_node=anchor, **asdict(level))
                    additions.append(row)
                    route.append(node)
                    proofs.append(dict(id=node, world=world, island=island, teaching_node=anchor,
                                       minimum=len(witness), witness=witness, states=states,
                                       structure_sha256=hashlib.sha256(key.encode()).hexdigest()))
                    time.sleep(.025)
                    break
                else: raise RuntimeError(f'No distinct bounded puzzle for world {world}, island {island}, index {index}')
            print(f'world {world+1}, island {island+1}: 12 levels', flush=True)
            time.sleep(.1)
    assert len(route) == len(set(route)) == 540 and len(additions) == 356
    bindings = []
    packs = {locale: json.loads((ROOT/f'Data/words_{locale}.json').read_text()) for locale in ['en','ru','uk']}
    def signature(symbols):
        seen = {}
        return [seen.setdefault(symbol, len(seen)) for symbol in symbols]
    for index, source in enumerate(additions):
        level = from_dict(source)
        for locale, words in packs.items():
            choices = [w for w in words if w['equality_signature'] == signature(level.target)]
            if not choices: raise RuntimeError(f'No {locale} word with the required repeated-symbol pattern: {source["id"]}')
            word = choices[index % len(choices)]
            mapping = {s: f'g:{locale}:{token}' for s, token in zip(level.target, word['tokens'])}
            floors = tuple(replace(f, **{k: replace(getattr(f,k), symbol=mapping[getattr(f,k).symbol])
                           if getattr(f,k) else None for k in ['key','repair','bomb','stamp']}) for f in level.floors)
            bound = Level(tuple(mapping[s] for s in level.start), tuple(mapping[s] for s in level.target), floors)
            bound.validate()
            bindings.append(dict(id=source['id']+'_'+locale, node_id=source['id'], locale=locale,
                                 mode='words', word_ids=[word['id']], teaching_node=source['teaching_node'], **asdict(bound)))
    output = append_sources(original_text, additions, bindings)
    output = output.rstrip()[:-1] + ',\n  "journey_order": ' + json.dumps(route, separators=(',', ':')) + '\n}\n'
    path.write_text(output)
    (ROOT/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent/campaign_extension.json').write_text(output)
    report = ROOT/'Reports/CampaignPacing'
    report.mkdir(exist_ok=True)
    pins = {source['id']: hashlib.sha256(json.dumps(source, sort_keys=True, ensure_ascii=False).encode()).hexdigest()
            for source in incumbents + extension['bindings']}
    (report/'authoring.json').write_text(json.dumps(dict(seed=SEED, trials=trials, levels=proofs, incumbent_pins=pins), indent=2)+'\n')
    print(f'DONE: 540 nodes, 356 additions, {len(bindings)} new bindings, {trials} candidates', flush=True)


if __name__ == '__main__': run()
