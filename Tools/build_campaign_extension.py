"""Seeded authoring candidates, solver-filtered; never rewrites original content."""
from cascade_model import *
from pathlib import Path
import random, hashlib, time
ROOT=Path(__file__).resolve().parents[1]
R=random.Random(24051005)
REGIONS=['Cloud Post','Glass Lagoon','Moss Archive','Snow Spire','Great Beacon']
IDEAS=[
 'Prepare the landing turn before crossing the one-use mail bridge.',
 'Mend the cracked jetty before spending the remaining strong crossings.',
 'Open the archive gate, then plan the coupled shelves before the next landing.',
 'Choose when to leave the fuse behind and prepare the mirrored landing.',
 'Carry the light through three different landings; save the repair for the final pattern.']
def canonical(l,mirror=False):
    n=len(l.start);mp={s:i for i,s in enumerate(l.target)}
    fs=[]
    for f in l.floors:
        x=asdict(f)
        for k in ['key','repair','bomb','stamp']:
            if x[k]:
                x[k]['symbol']=mp[x[k]['symbol']]
                if mirror:x[k]['slot']=n-1-x[k]['slot']
        if mirror:
            x['edges']=x['edges'][::-1];x['fragile']=tuple(sorted(n-2-e for e in x['fragile']))
            if x['linked']:x['linked']=tuple(sorted(n-2-e for e in x['linked']))
            if x['key']:x['key']['value']=n-2-x['key']['value']
            if x['bridge'] is not None:x['bridge']=n-2-x['bridge']
            x['shift']=-x['shift']
        fs.append(x)
    target=l.target[::-1] if mirror else l.target
    # Normalize against reflected target too, so reflection alone is not a new puzzle.
    mp={s:i for i,s in enumerate(target)}
    if mirror:
        for x,f in zip(fs,l.floors):
            for k in ['key','repair','bomb','stamp']:
                if x[k]:x[k]['symbol']=mp[getattr(f,k).symbol]
    return json.dumps(dict(start=[mp[s] for s in (l.start[::-1] if mirror else l.start)],floors=fs),sort_keys=True)
def featureless(l,z):
    fs=[]
    for f in l.floors:
        if z==0:f=replace(f,shift=0)
        elif z==1:f=replace(f,repair=None)
        elif z==2:
            edges=list(f.edges)
            if f.key:edges[f.key.value]=False
            f=replace(f,key=None,edges=tuple(edges))
        elif z==3:f=replace(f,mirror=False)
        else:f=replace(f,shift=0,mirror=False)
        fs.append(f)
    return replace(l,floors=tuple(fs))
def candidate(z):
    n=5 if z<2 or R.random()<.25 else 6
    target=tuple('s:'+c for c in 'ABCDEF'[:n]);start=list(target);R.shuffle(start)
    count=2 if z<4 else 3
    fs=[]
    for j in range(count):
        edges=[True]*(n-1);hp=R.randint(3,7 if z<3 else 8)
        kw={}
        if j==1 and z in (0,2,4):kw['shift']=R.choice([-1,1])
        if (j==1 and z==3) or (j==2 and z==4):kw['mirror']=True
        if z==0 and j==0:kw['bridge']=R.randrange(n-1)
        if z==1:
            kw['fragile']=(R.randrange(n-1),)
            if j==0:kw['repair']=Sensor(R.randrange(n),R.choice(target),R.randint(1,3))
        if z==2 and j==0:
            edge=R.randrange(n-1);edges[edge]=False
            kw['key']=Sensor(R.randrange(n),R.choice(target),edge)
        if z in (2,4) and j==count-1:kw['linked']=R.choice([(0,2),(1,3)] if n==5 else [(0,2),(1,3),(2,4)])
        if z in (3,4) and j==0:kw['bomb']=Bomb(R.randrange(n),R.choice(target),R.randint(2,4),R.randint(2,4))
        if z==4 and j==1:kw['repair']=Sensor(R.randrange(n),R.choice(target),2);kw['fragile']=(R.randrange(n-1),)
        # A clear optional contact on the first floor; published only when a detour is proven.
        if R.random()<.25 and j==0:kw['stamp']=Sensor(R.randrange(n),R.choice(target))
        fs.append(Floor(hp,tuple(edges),**kw))
    return Level(tuple(start),target,tuple(fs))
def shortest(l,goal=None,cap=200000):
    root=initial(l);q=deque([root]);depth={root:0}
    while q:
        s=q.popleft()
        if s.terminal=='won' and (goal is None or goal(s)):return depth[s]
        if not s.terminal:
            for c in commands(s):
                t=step(l,s,c)
                if t!=s and t not in depth:
                    if len(depth)>=cap:raise RuntimeError('UNKNOWN')
                    depth[t]=depth[s]+1;q.append(t)
    return None

def run():
    levels=[];reports=[];seen=set();trials=0
    original=json.loads((ROOT/'Data/levels.json').read_text())
    for x in original:
        l=from_dict(x);seen.add(min(canonical(l),canonical(l,True)))
    for z in range(5):
        batch=[]
        while len(batch)<24:
            trials+=1;l=candidate(z);key=min(canonical(l),canonical(l,True))
            if key in seen:continue
            try:
                minimum=shortest(l)
                if minimum is None or not 5+z<=minimum<=9+z:continue
                removed=shortest(featureless(l,z))
                if removed==minimum:continue
                stamp=None
                if any(f.stamp for f in l.floors):
                    sm=shortest(l,lambda s:s.marks&1)
                    if sm is None or sm<minimum+2 or sm>minimum+5:
                        l=replace(l,floors=tuple(replace(f,stamp=None) for f in l.floors));key=min(canonical(l),canonical(l,True))
                        if key in seen:continue
                    else:stamp=analyze(l,lambda s:s.marks&1)
                metrics=analyze(l)
                if metrics['near_optimal_first_states_plus2']<2 or metrics['forced_ratio']>=.9:continue
            except RuntimeError:continue
            seen.add(key);batch.append((l,metrics,stamp,removed,key))
            print(REGIONS[z],len(batch),minimum,metrics['states'],flush=True)
            if trials>10000:raise RuntimeError('Candidate budget exceeded')
        # Deliberately alternate shorter relief tasks and harder routes within each region.
        batch.sort(key=lambda x:x[1]['optimal_actions'])
        order=[]
        for i in range(12):order.extend([batch[i],batch[i+12]])
        for l,m,stamp,removed,key in order:
            i=41+len(levels);node=f'C{i:02d}'
            row=dict(id=node,title=REGIONS[z]+f' {i}',lesson=['post_landing','jetty_repair','archive_key','snow_mirror','beacon_route'][z],rules_version='2.0',**asdict(l))
            levels.append(row);reports.append(dict(id=node,region=REGIONS[z],route_idea=IDEAS[z],**m,stamp_objective=stamp,
                without_feature_minimum=removed,structure_sha256=hashlib.sha256(key.encode()).hexdigest()))
    bindings=[]
    packs={loc:json.loads((ROOT/f'Data/words_{loc}.json').read_text()) for loc in ['en','ru','uk']}
    for idx,row in enumerate(levels):
        l=from_dict(row)
        for loc,words in packs.items():
            choices=[w for w in words if w['equality_signature']==list(range(len(l.target)))]
            w=choices[idx%len(choices)];mapping={s:f'g:{loc}:{c}' for s,c in zip(l.target,w['tokens'])}
            fs=[]
            for f in l.floors:
                fs.append(replace(f,**{k:replace(getattr(f,k),symbol=mapping[getattr(f,k).symbol]) if getattr(f,k) else None for k in ['key','repair','bomb','stamp']}))
            bound=Level(tuple(mapping[s] for s in l.start),tuple(mapping[s] for s in l.target),tuple(fs))
            bindings.append(dict(id=row['id']+'_'+loc,node_id=row['id'],locale=loc,mode='words',word_ids=[w['id']],**asdict(bound)))
    extension=dict(levels=levels,bindings=bindings)
    text=json.dumps(extension,ensure_ascii=False,indent=2)+'\n'
    (ROOT/'Data/campaign_extension.json').write_text(text)
    (ROOT/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent/campaign_extension.json').write_text(text)
    path=ROOT/'Reports/P20';path.mkdir(exist_ok=True)
    (path/'python_campaign_balance.json').write_text(json.dumps(reports,indent=2)+'\n')
    (path/'generation-summary.json').write_text(json.dumps(dict(seed=24051005,trials=trials,new_levels=120,word_bindings=len(bindings),minimum=min(r['optimal_actions'] for r in reports),maximum=max(r['optimal_actions'] for r in reports),mean=sum(r['optimal_actions'] for r in reports)/120,stamp_contracts=sum(bool(r['stamp_objective']) for r in reports),note='Solver evidence, not human enjoyment.'),indent=2)+'\n')
    print('DONE',trials,flush=True)
if __name__=='__main__':run()
