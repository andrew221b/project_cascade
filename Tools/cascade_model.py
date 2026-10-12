"""CASCADE v2 reference. Deterministic persistent row, NOT a matching game.
Python 3.10+, standard library. Illustrative reference, not a Unity build.
"""
from dataclasses import dataclass, replace, asdict
from collections import Counter, deque, defaultdict
from typing import Optional
import json

@dataclass(frozen=True)
class Sensor:
    slot: int
    symbol: str
    value: int = 1   # repair amount; key's edge; unused for stamp

@dataclass(frozen=True)
class Bomb:
    slot: int
    symbol: str
    ticks: int = 2
    damage: int = 2

@dataclass(frozen=True)
class Floor:
    hp: int
    edges: tuple[bool, ...]
    fragile: tuple[int, ...] = ()
    linked: Optional[tuple[int, int]] = None
    shift: int = 0
    mirror: bool = False
    key: Optional[Sensor] = None
    repair: Optional[Sensor] = None
    bomb: Optional[Bomb] = None
    bridge: Optional[int] = None
    stamp: Optional[Sensor] = None

@dataclass(frozen=True)
class Level:
    start: tuple[str, ...]
    target: tuple[str, ...]
    floors: tuple[Floor, ...]

    def validate(self):
        n=len(self.start)
        if not 3 <= n <= 8 or len(self.target)!=n or Counter(self.start)!=Counter(self.target):
            raise ValueError('Row length/multiset mismatch')
        if not 1 <= len(self.floors)<=4: raise ValueError('Floor count')
        for f in self.floors:
            if not 1<=f.hp<=12 or len(f.edges)!=n-1: raise ValueError('HP/edges')
            if f.shift not in (-1,0,1) or (f.shift and f.mirror): raise ValueError('One entry transform')
            if len(set(f.fragile))!=len(f.fragile) or any(not 0<=e<n-1 for e in f.fragile): raise ValueError('Fragile')
            if f.linked:
                a,b=f.linked
                if not(0<=a<n-1 and 0<=b<n-1 and abs(a-b)>1): raise ValueError('Linked overlap')
            if f.bridge is not None:
                if not 0<=f.bridge<n-1 or not f.edges[f.bridge]: raise ValueError('Bridge must start open')
                if f.key and f.key.value==f.bridge: raise ValueError('Bridge/key collision')
            for d in (f.key,f.repair,f.bomb,f.stamp):
                if d and (not 0<=d.slot<n or d.symbol not in self.target): raise ValueError('Sensor target')
            if f.key and (not 0<=f.key.value<n-1 or f.edges[f.key.value]): raise ValueError('Key must open closed edge')
            if f.repair and not 1<=f.repair.value<=3: raise ValueError('Repair amount')
            if f.bomb and (not 1<=f.bomb.ticks<=6 or not 1<=f.bomb.damage<=4): raise ValueError('Bomb')

@dataclass(frozen=True)
class State:
    row: tuple[str,...]
    floor: int
    hp: int
    timer: int = 0
    bomb_phase: str = 'absent'
    key_used: bool = False
    repair_used: bool = False
    bridge_used: bool = False
    marks: int = 0
    defused: int = 0
    exploded: int = 0
    abandoned: int = 0
    terminal: str = ''


def contacts(l,s):
    f=l.floors[s.floor]
    if f.bomb and s.bomb_phase=='armed' and s.row[f.bomb.slot]==f.bomb.symbol:
        s=replace(s,bomb_phase='defused',timer=0,defused=s.defused+1)
    if f.key and not s.key_used and s.row[f.key.slot]==f.key.symbol:
        s=replace(s,key_used=True)
    if f.repair and not s.repair_used and s.row[f.repair.slot]==f.repair.symbol:
        s=replace(s,hp=s.hp+f.repair.value,repair_used=True)
    if f.stamp and s.row[f.stamp.slot]==f.stamp.symbol:
        s=replace(s,marks=s.marks | (1<<s.floor))
    return s


def enter(l,s,index):
    if index==len(l.floors):
        return replace(s,floor=index,hp=0,timer=0,bomb_phase='absent',key_used=False,
                       repair_used=False,bridge_used=False,terminal='won' if s.row==l.target else 'lost')
    f=l.floors[index];row=s.row
    if f.mirror: row=tuple(reversed(row))
    elif f.shift:
        k=f.shift%len(row);row=row[-k:]+row[:-k]
    s=replace(s,row=row,floor=index,hp=f.hp,timer=f.bomb.ticks if f.bomb else 0,
              bomb_phase='armed' if f.bomb else 'absent',key_used=False,
              repair_used=False,bridge_used=False)
    return contacts(l,s)


def initial(l):
    l.validate()
    return enter(l,State(l.start,0,0),0)


def is_open(f,s,e):
    if f.bridge==e and s.bridge_used: return False
    return f.edges[e] or bool(f.key and s.key_used and f.key.value==e)


def step(l,s,cmd):
    """Return unchanged state for rejected command. Indices are ZERO based.
    D is one explicit Drop command: contacts/timer on OLD floor, then forced exit.
    Linked swap costs 1 + number of fragile edges crossed, including no-op subpair.
    """
    if s.terminal:return s
    f=l.floors[s.floor];drop=cmd=='D';r=list(s.row);cost=0;used=s.bridge_used
    if not drop:
        if type(cmd) is not int or not 0<=cmd<len(r)-1:return s
        es=f.linked if f.linked and cmd in f.linked else (cmd,)
        if any(not is_open(f,s,e) for e in es):return s
        for e in es:r[e],r[e+1]=r[e+1],r[e]
        if tuple(r)==s.row:return s
        cost=1+sum(e in f.fragile for e in es)
        used=used or (f.bridge is not None and f.bridge in es)
    n=contacts(l,replace(s,row=tuple(r),hp=s.hp-cost,bridge_used=used))
    if n.bomb_phase=='armed':
        t=n.timer-1
        if t==0:n=replace(n,timer=0,bomb_phase='exploded',exploded=n.exploded+1,hp=n.hp-f.bomb.damage)
        else:n=replace(n,timer=t)
    if drop or n.hp<=0:
        if n.bomb_phase=='armed':n=replace(n,bomb_phase='abandoned',abandoned=n.abandoned+1)
        n=enter(l,n,s.floor+1)
    return n


def commands(s):return tuple(range(len(s.row)-1))+('D',)


def graph(l,cap=200000):
    root=initial(l);q=deque([root]);g={};depth={root:0};rev=defaultdict(set)
    while q:
        s=q.popleft();out=[]
        if not s.terminal:
            for a in commands(s):
                t=step(l,s,a)
                if t==s:continue
                assert Counter(t.row)==Counter(l.target)
                assert t.terminal or t.hp>0
                out.append((a,t));rev[t].add(s)
                if t not in depth:
                    if len(depth)>=cap:raise RuntimeError('UNKNOWN: search budget exceeded')
                    depth[t]=depth[s]+1;q.append(t)
        g[s]=out
    return root,g,rev,depth


def analyze(l, objective=None):
    root,g,rev,depth=graph(l);goals=[s for s in g if s.terminal=='won' and (objective is None or objective(s))]
    d={s:0 for s in goals};q=deque(goals)
    while q:
        s=q.popleft()
        for p in rev[s]:
            if p not in d:d[p]=d[s]+1;q.append(p)
    path=[];s=root
    if s in d:
        while d[s]:
            a,t=next((a,t) for a,t in g[s] if t in d and d[t]==d[s]-1)
            path.append(a);s=t
    opt=d.get(root);dead=[s for s in g if not s.terminal and s not in d]
    viable={root} if opt is not None else set();q=deque(viable);forced=0;count=0
    while q:
        s=q.popleft()
        if not d[s]:continue
        ns={t for _,t in g[s] if d.get(t)==d[s]-1};count+=1;forced+=len(ns)==1
        for t in ns:
            if t not in viable:viable.add(t);q.append(t)
    near=len({t for _,t in g[root] if t in d and 1+d[t]<=opt+2}) if opt is not None else 0
    return dict(solvable=opt is not None,optimal_actions=opt,path=path,states=len(g),
                transitions=sum(map(len,g.values())),dead_states=len(dead),
                earliest_dead_depth=min((depth[s] for s in dead),default=None),
                near_optimal_first_states_plus2=near,forced_ratio=round(forced/max(1,count),4),
                terminal_variants=len(goals))


def from_dict(d):
    fs=[]
    for x in d['floors']:
        x=dict(x)
        for k in ('key','repair','stamp'):
            if x.get(k):x[k]=Sensor(**x[k])
        if x.get('bomb'):x['bomb']=Bomb(**x['bomb'])
        for k in ('edges','fragile','linked'):
            if x.get(k) is not None:x[k]=tuple(x[k])
        fs.append(Floor(**x))
    return Level(tuple(d['start']),tuple(d['target']),tuple(fs))
