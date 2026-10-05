"""Compare complete reachable transition traces, including rejected/terminal commands."""
from pathlib import Path
from collections import deque
import hashlib,json
from cascade_model import from_dict,initial,step

ROOT=Path(__file__).resolve().parents[1]
def state(s):
    return '|'.join(map(str,(','.join(s.row),s.floor,s.hp,s.timer,s.bomb_phase,
        int(s.key_used),int(s.repair_used),int(s.bridge_used),s.marks,s.defused,s.exploded,s.abandoned,s.terminal)))
def digest(source):
    level=from_dict(source);first=initial(level);queue=deque([first]);seen={first}
    h=hashlib.sha256();h.update((state(first)+'\n').encode());count=accepted=0
    while queue:
        before=queue.popleft()
        for cmd in [*range(-1,len(before.row)), 'D']:
            after=step(level,before,cmd);ok=after!=before;count+=1;accepted+=ok
            h.update((f'{cmd}|{int(ok)}|{state(after)}\n').encode())
            if ok and after not in seen:seen.add(after);queue.append(after)
    return dict(id=source['id'],states=len(seen),transitions=count,accepted=accepted,sha256=h.hexdigest())
def main():
    # C# includes invalid indices -1 and n-1. Python reproduces the exact command stream.
    source=json.loads((ROOT/'Data/levels.json').read_text())+json.loads((ROOT/'Data/localized_levels.json').read_text())
    actual={r['id']:r for r in json.loads((ROOT/'Reports/P18/csharp-trace-digests.json').read_text())}
    expected=[digest(s) for s in source];diff=next(({'id':e['id'],'python':e,'csharp':actual.get(e['id'])} for e in expected if e!=actual.get(e['id'])),None)
    folder=ROOT/'Reports/P18';(folder/'python-trace-digests.json').write_text(json.dumps(expected,indent=2))
    report=dict(puzzles=len(expected),states=sum(e['states'] for e in expected),commands=sum(e['transitions'] for e in expected),accepted=sum(e['accepted'] for e in expected),first_diff=diff,matched=diff is None,
        scope='All reachable state/acceptance traces. Event animation and enjoyment are separate.')
    (folder/'trace-comparison.json').write_text(json.dumps(report,indent=2));print(json.dumps(report));assert diff is None
if __name__=='__main__':main()
