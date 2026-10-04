from cascade_model import *
from pathlib import Path
import unicodedata
ROOT=Path(__file__).resolve().parents[1]

def F(hp,n=4,**kw):return Floor(hp,(True,)*(n-1),**kw)
def L(start,target,*floors):return Level(tuple(start),tuple(target),tuple(floors))
specs=[
('Первый обмен','swap',L('BAC','ABC',F(1,3))),
('Два обмена','swap',L('BCA','ABC',F(2,3))),
('Сохраняем весь ряд','collapse',L('BADC','ABCD',F(1),F(1))),
('Собрать ещё не доставить','drop',L('BACD','ABCD',F(2))),
('Повторяющиеся символы','duplicates',L('BABA','ABAB',F(1),F(2))),
('Разные связи','locked',L('BADC','ABCD',Floor(2,(True,False,False)),Floor(1,(False,False,True)))),
('Спуститься раньше','drop',L('BACD','ABCD',Floor(4,(False,True,True)),Floor(1,(True,False,False)))),
('Средняя пара внизу','locked',L('ACBD','ABCD',Floor(2,(False,False,True)),Floor(1,(False,True,False)))),
('Посадочный сдвиг','conveyor',L('DABC','ABCD',F(2),F(1,shift=-1))),
('Подготовить левый сдвиг','conveyor',L('DACB','ABCD',F(1),F(1,shift=-1))),
('Правый сдвиг','conveyor',L('BCAD','ABCD',F(2),F(2,shift=1))),
('Пять элементов','conveyor',L('EABDC','ABCDE',F(2,5),F(2,5,shift=-1))),
('Последний такт бомбы','bomb',L('BACD','ABCD',F(2,bomb=Bomb(0,'A',1,2)))),
('Взрыв помогает доставить','bomb',L('BACD','ABCD',F(3,bomb=Bomb(3,'A',1,2)))),
('Бомбу можно оставить','bomb',L('BACD','ABCD',Floor(4,(False,True,True),bomb=Bomb(3,'A',2,2)),F(1))),
('Обычная передышка','breather',L('BADC','ABCD',F(3),F(2))),
('Ключ открывает маршрут','key',L('BCAD','ABCD',Floor(2,(False,True,True),key=Sensor(1,'A',0)))),
('Ремонт после удара','repair',L('CABD','ABCD',F(1,repair=Sensor(0,'A',1)),F(1))),
('Ключ и обратный ход','key',L('ACBD','ABCD',Floor(5,(True,False,True),key=Sensor(0,'C',1)))),
('Бомба и ремонт вместе','bomb_repair',L('BACD','ABCD',F(2,bomb=Bomb(3,'A',1,2),repair=Sensor(0,'A',1)))),
('Хрупкое ребро','fragile',L('BACD','ABCD',F(2,fragile=(0,)))),
('Цена выше остатка','fragile_repair',L('BACD','ABCD',F(1,fragile=(0,),repair=Sensor(0,'A',1)))),
('Беречь верхнюю платформу','fragile',L('BADC','ABCD',F(2,fragile=(0,)),F(1))),
('Посадка после дорогого хода','fragile_conveyor',L('DACB','ABCD',F(2,fragile=(2,)),F(1,shift=-1))),
('Две пары за раз','linked',L('BADC','ABCD',F(1,linked=(0,2)))),
('Одинаковая первая пара','linked_duplicates',L('AABA','AAAB',F(1,linked=(0,2)))),
('Сначала разомкнуть путь','key_linked',L('BADC','ABCD',Floor(4,(False,True,True),linked=(0,2),key=Sensor(1,'D',0)))),
('Общая цена двух пар','fragile_linked',L('BADC','ABCD',F(3,linked=(0,2),fragile=(0,2)))),
('Фигуры обрамляют слово','mixed',L(('s:T','g:C','g:T','g:A','s:O'),('s:T','g:C','g:A','g:T','s:O'),F(1,5),F(1,5))),
('Две фигуры, слово, две фигуры','mixed7',L(('g:A','s:T','s:O','g:C','s:T','s:O','g:T'),('s:T','s:T','g:C','g:A','g:T','s:O','s:O'),F(3,7),F(3,7),F(3,7))),
('Смешанный ряд и ключ','mixed_key',L(('s:T','g:A','g:T','g:C','s:O'),('s:T','g:C','g:A','g:T','s:O'),Floor(2,(True,False,True,True),key=Sensor(2,'g:C',1)))),
('Восемь мест, два слова','mixed8',L(('g:C','s:T','g:A','g:T','s:O','g:D','g:G','g:O'),('s:T','g:C','g:A','g:T','s:O','g:D','g:O','g:G'),F(1,8),F(1,8))),
('Одноразовый мост','bridge',L('BACD','ABCD',F(1,bridge=0))),
('Не потратить мост зря','bridge',L('BCAD','ABCD',F(2,bridge=0))),
('Необязательная печать','stamp',L('ACBD','ABCD',F(5,stamp=Sensor(0,'C')))),
('Печать на посадке','stamp_conveyor',L('DABC','ABCD',F(2),F(1,shift=-1,stamp=Sensor(0,'A')))),
('Три согласованных этажа','combo',L('BCDEA','ABCDE',F(2,5),F(3,5,shift=-1,bomb=Bomb(0,'A',2,2)),F(1,5))),
('Ключ ремонтирует маршрут','key_repair',L('BCAD','ABCD',Floor(1,(False,True,True),key=Sensor(1,'A',0),repair=Sensor(1,'A',1)))),
('Связанные пары и взрыв','linked_bomb',L('BADC','ABCD',F(3,linked=(0,2),bomb=Bomb(3,'A',1,2)))),
('Передай целый смешанный ряд','mixed_bridge',L(('s:T','g:A','g:C','g:T','s:O'),('s:T','g:C','g:A','g:T','s:O'),F(2,5,bridge=1),F(2,5))),
]

# Symbol equality is logic; presentation is separate. Symbol namespaces avoid O == circle.
def remap(l,m):
    fs=[]
    for f in l.floors:
        kw={k:replace(d,symbol=m[d.symbol]) if (d:=getattr(f,k)) else None for k in ('key','repair','bomb','stamp')}
        fs.append(replace(f,**kw))
    return Level(tuple(m[s] for s in l.start),tuple(m[s] for s in l.target),tuple(fs))

def signature(row):
    ids={};out=[]
    for x in row:
        if x not in ids:ids[x]=len(ids)
        out.append(ids[x])
    return out

WORDS={
'en':{3:'CAT',4:'LAMP',5:'PLANT',6:'PLANET','ABAB':'MAMA','AAAB':None},
'ru':{3:'КОТ',4:'ЛУНА',5:'ВЕСНА',6:'ГЛОБУС','ABAB':'МАМА','AAAB':None},
'uk':{3:'КІТ',4:'МЛИН',5:'КНИГА',6:'ГЛОБУС','ABAB':'МАМА','AAAB':None}}

def build():
    data=[];reports=[];bindings=[];wordrecords={k:{} for k in WORDS}
    for i,(title,lesson,l) in enumerate(specs,1):
        lid=f'C{i:02d}';mixed=any(x.startswith('g:') for x in l.target)
        m={x:x if ':' in x else 's:'+x for x in set(l.target)}
        physical=remap(l,m)
        r=analyze(physical)
        if not r['solvable']:raise ValueError((lid,title,'UNSOLVABLE'))
        reports.append(dict(id=lid,title=title,lesson=lesson,**r))
        data.append(dict(id=lid,title=title,lesson=lesson,rules_version='2.0',**asdict(physical)))
        for locale,pool in WORDS.items():
            # Only bijective reskins; unusual repeat fixture stays shape-only.
            if mixed:
                word= {'en':'CAT','ru':'КОТ','uk':'КІТ'}[locale]
                word2={'en':'DOG','ru':'СЫР','uk':'СИР'}[locale]
                m={x:x for x in l.target}
                m.update({f'g:{c}':f'g:{locale}:{a}' for c,a in zip('CAT',word)})
                if 'g:D' in l.target:m.update({f'g:{c}':f'g:{locale}:{a}' for c,a in zip('DOG',word2)})
                mode='mixed';ws=[word]+([word2] if 'g:D' in l.target else [])
            else:
                sig=''.join(l.target)
                word=pool.get(sig) if len(set(l.target))<len(l.target) else pool.get(len(l.target))
                if not word:continue
                if signature(l.target)!=signature(word):raise ValueError('Word signature')
                m={s:f'g:{locale}:{g}' for s,g in zip(l.target,word)};mode='words';ws=[word]
            loc=remap(l,m);rr=analyze(loc)
            assert rr['solvable'] and rr['optimal_actions']==r['optimal_actions']
            bindings.append(dict(id=f'{lid}_{locale}',node_id=lid,locale=locale,mode=mode,word_ids=[f'{locale}_{w}' for w in ws],**asdict(loc)))
            for w in ws:
                wordrecords[locale][w]=dict(id=f'{locale}_{w}',locale=locale,text=w,tokens=list(unicodedata.normalize('NFC',w)),
                    equality_signature=signature(w),editorial_status='draft_native_review_required',
                    origin='original_short_editorial_list',license_review='required_before_release')
    for filename,obj in [('levels.json',data),('localized_levels.json',bindings)]:
        (ROOT/'Data'/filename).write_text(json.dumps(obj,ensure_ascii=False,indent=2))
    for locale,extra in json.loads((ROOT/'Data'/'word_seeds.json').read_text()).items():
        for w in extra:
            if w not in wordrecords[locale]:
                wordrecords[locale][w]=dict(id=f'{locale}_{w}',locale=locale,text=w,tokens=list(unicodedata.normalize('NFC',w)), equality_signature=signature(w),editorial_status='draft_native_review_required',origin='original_short_editorial_list',license_review='required_before_release')
    for locale,ws in wordrecords.items():
        (ROOT/'Data'/f'words_{locale}.json').write_text(json.dumps(list(ws.values()),ensure_ascii=False,indent=2))
    report=dict(levels=reports,total_states=sum(r['states'] for r in reports),total_transitions=sum(r['transitions'] for r in reports),
                localized_variants=len(bindings),word_counts={k:len(v) for k,v in wordrecords.items()},
                note='Exhaustive per-fixture graph; not all possible levels and not user testing.')
    (ROOT/'Reports'/'balance.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
    for r in reports:print(r['id'],r['lesson'],r['optimal_actions'],r['path'],r['states'])
    print('TOTAL',report['total_states'],report['total_transitions'],'LOCALIZED',len(bindings))

if __name__=='__main__':build()
