"""Bounded HP-only revision; preserves devices, bindings and legacy HP for pinned saves."""
from build_campaign_extension import shortest, featureless, canonical, ROOT
from cascade_model import *
import hashlib

def wear(l, path):
    s=initial(l); discarded=0; moves=[0]*len(l.floors)
    for c in path:
        if c=='D': discarded+=max(0,s.hp)
        else: moves[s.floor]+=1
        s=step(l,s,c)
    return dict(discarded_hp=discarded, floor_swaps=moves)

def run():
    if (ROOT/'Data/campaign_durability_v1.json').exists():
        raise RuntimeError('Revision already applied; restore the original content before regenerating.')
    path=ROOT/'Data/campaign_extension.json'; extension=json.loads(path.read_text())
    reports=json.loads((ROOT/'Reports/P20/python_campaign_balance.json').read_text())
    legacy={}; notes=[]
    for index,row in enumerate(extension['levels']):
        old=from_dict(row); z=index//24; original=reports[index]; l=old
        def acceptable(candidate):
            minimum=shortest(candidate)
            if minimum is None or not 5+z<=minimum<=9+z: return False
            if shortest(featureless(candidate,z))==minimum: return False
            if original['stamp_objective'] and shortest(candidate,lambda s:s.marks&1) is None: return False
            m=analyze(candidate)
            return m['near_optimal_first_states_plus2']>=2 and m['forced_ratio']<.9
        # A second pass compensates for repairs and forced descent changing later needs.
        for _ in range(2):
            for floor in range(len(l.floors)):
                for hp in range(1,l.floors[floor].hp):
                    fs=list(l.floors);fs[floor]=replace(fs[floor],hp=hp);candidate=replace(l,floors=tuple(fs))
                    if acceptable(candidate): l=candidate;break
        # Interleave tight and one-point-grace tasks, retaining all constraints.
        if index%2==0 and l!=old:
            fs=list(l.floors);last=len(fs)-1
            if fs[last].hp<old.floors[last].hp:
                fs[last]=replace(fs[last],hp=fs[last].hp+1);candidate=replace(l,floors=tuple(fs))
                if acceptable(candidate):l=candidate
        metrics=analyze(l)
        stamp=analyze(l,lambda s:s.marks&1) if original['stamp_objective'] else None
        key=min(canonical(l),canonical(l,True))
        reports[index]=dict(original,**metrics,stamp_objective=stamp,
            without_feature_minimum=shortest(featureless(l,z)),structure_sha256=hashlib.sha256(key.encode()).hexdigest())
        if l!=old:
            sources=[row]+[b for b in extension['bindings'] if b['node_id']==row['id']]
            pins={}
            for source in sources:
                locale=source.get('locale','en')
                words={w['id']:w['text'] for w in json.loads((ROOT/f'Data/words_{locale}.json').read_text())}
                payload='\n'.join(w+':'+words[w] for w in source.get('word_ids',[]))
                canonical_source=json.dumps(source,ensure_ascii=False,sort_keys=True,separators=(',',':'))
                pins[source['id']]=hashlib.sha256(('2.0\n'+canonical_source+'\n'+payload).encode()).hexdigest()
            legacy[row['id']]=dict(hp=[f.hp for f in old.floors],pins=pins)
        hp=[f.hp for f in l.floors]
        for f,h in zip(row['floors'],hp):f['hp']=h
        for bound in extension['bindings']:
            if bound['node_id']==row['id']:
                for f,h in zip(bound['floors'],hp):f['hp']=h
        notes.append(dict(id=row['id'],before_hp=[f.hp for f in old.floors],after_hp=hp,
            before=wear(old,original['path']),after=wear(l,metrics['path']),minimum=metrics['optimal_actions']))
        print(row['id'],notes[-1]['before_hp'],'->',hp,'minimum',metrics['optimal_actions'],flush=True)
    assert len({r['structure_sha256'] for r in reports})==120
    output=json.dumps(extension,ensure_ascii=False,indent=2)+'\n'
    path.write_text(output)
    (ROOT/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent/campaign_extension.json').write_text(output)
    (ROOT/'Reports/P20/python_campaign_balance.json').write_text(json.dumps(reports,indent=2)+'\n')
    compatibility=json.dumps(legacy,indent=2)+'\n'
    (ROOT/'Data/campaign_durability_v1.json').write_text(compatibility)
    (ROOT/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent/campaign_durability_v1.json').write_text(compatibility)
    report=ROOT/'Reports/RouteBalance';report.mkdir(exist_ok=True)
    (report/'budget_revision.json').write_text(json.dumps(dict(changed=len(legacy),levels=notes,
        mean_discarded_before=sum(n['before']['discarded_hp'] for n in notes)/120,
        mean_discarded_after=sum(n['after']['discarded_hp'] for n in notes)/120),indent=2)+'\n')

if __name__=='__main__':run()
