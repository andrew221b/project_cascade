#!/usr/bin/env python3
"""Accessible raster screen routes: small local chains, no crossing connections."""
from pathlib import Path
import json, math, argparse
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--captures', type=Path, help='Fresh native capture directory; no archived fallback')
args=parser.parse_args()
OUT = (args.captures.resolve() if args.captures else ROOT / 'Reports/ScreenRoadmap-Oct09')
# The existing navigation script owns the verified routes; do not recapture old UI.
scope = {'__file__': str(ROOT / 'Tools/build_navigation_png.py')}
source = (ROOT / 'Tools/build_navigation_png.py').read_text()
exec(source[:source.index('H = 330 + len(rows)*ROW')], scope)
sections, captures = scope['rows'], scope['sources']
fresh = args.captures is not None
if fresh:
    entries=json.loads((OUT/'screens.json').read_text())
    captures={Path(e['file']).stem:OUT/e['file'] for e in entries}
    captures['inline-bag']=captures['20a-inline-bag']
    # The newly captured previews follow the actual separate Look closer action.
    captures['11-shop-tools']=captures['11b-tool-preview']
    captures['12-shop-mika']=captures['12b-costume-preview']
    # These old windows have been removed; do not recreate them from old pictures.
    title,note,items,links=sections[-1]
    keep=[i for i in range(len(items)) if items[i][0] not in ('36-requests','40-word-language')]
    remap={old:new for new,old in enumerate(keep)}
    sections[-1]=(title,note,[items[i] for i in keep],
                  [(remap[a],remap[b],action,route,track,kind) for a,b,action,route,track,kind in links
                   if a in remap and b in remap])
    required={item[0] for row in sections for item in row[2]}
    missing=sorted(stem for stem in required if stem not in captures or not captures[stem].is_file())
    if missing:raise RuntimeError('Fresh captures missing: '+', '.join(missing))
# Replace the original map's cross-section footnotes with visible local routes.
sections[2][2].extend([('02-settings','Настройки',''),('01-home','Главное меню','')])
sections[2][3].extend([(2,8,'Настройки','middle',0,'tap'),(8,2,'Назад в паузу','bottom',0,'return'),
                       (2,9,'Главная','middle',0,'tap')])
sections[4][2].append(('19-supplies','Предметы / инструменты',''))
sections[4][3].extend([(1,8,'Предметы','middle',0,'tap'),(8,1,'Назад в мои вещи','bottom',0,'return')])
W, PW, PH, GAP, M = 4400, 2100, 1010, 60, 70
CW, CH, STEP = 340, 760, 520
BG, PAPER, INK, TEAL, AMBER, GREY = '#F1EDE3', '#FFFCF4', '#253340', '#156158', '#815019', '#536477'
FONTP = '/System/Library/Fonts/Supplemental/'
def face(size, bold=False):
    return ImageFont.truetype(FONTP + ('Arial Bold.ttf' if bold else 'Arial.ttf'), size)

# Keep every edge, but repeat screens to make each branch local and readable.
# Forward paths have at most four cards. A reciprocal edge uses a separate return lane.
all_panels=[]
for si, (title, note, items, links) in enumerate(sections):
    remaining=set(range(len(links))); panels=[]
    while remaining:
        candidates=[]
        def walk(ids, path):
            candidates.append((list(ids), list(path)))
            if len(path)==3:return
            for ei in sorted(remaining):
                a,b,_,_,_,kind=links[ei]
                if a==ids[-1] and b not in ids:
                    walk(ids+[b],path+[ei])
        for ei in sorted(remaining):
            a,b,*_=links[ei]
            walk([a,b],[ei])
        def score(candidate):
            ids,path=candidate
            reverse=[ei for ei in remaining if ei not in path and
                     any(links[ei][0]==ids[k+1] and links[ei][1]==ids[k] for k in range(len(ids)-1))]
            return (len(path)+len(reverse), sum(links[ei][3]=='middle' for ei in path),
                    -sum(links[ei][5]=='return' for ei in path), -min(path))
        ids,forward=max(candidates,key=score)
        backwards=[ei for ei in sorted(remaining) if ei not in forward and
                   any(links[ei][0]==ids[k+1] and links[ei][1]==ids[k] for k in range(len(ids)-1))]
        covered=forward+backwards
        panels.append({'ids':ids,'forward':forward,'backwards':backwards,'covered':covered})
        remaining.difference_update(covered)
    all_panels.append(panels)

TOP, HEAD = 430, 170
H=TOP+sum(HEAD+math.ceil(len(p)/2)*(PH+GAP)+80 for p in all_panels)+110
canvas=Image.new('RGB',(W,H),BG);draw=ImageDraw.Draw(canvas)
checks=[];card_bounds=[];label_bounds=[];painted=[];thumbs={}

def lines(text, f, width):
    result=[];line=''
    for word in text.split():
        trial=(line+' '+word).strip()
        if line and draw.textlength(trial,font=f)>width:result.append(line);line=word
        else:line=trial
    if line:result.append(line)
    return result

def textblock(text, x, y, width, size, bold=False, color=INK, leading=None):
    f=face(size,bold);leading=leading or round(size*1.4)
    ls=lines(text,f,width)
    for i,line in enumerate(ls):draw.text((x,y+i*leading),line,font=f,fill=color)
    return y+len(ls)*leading

def arrow(start,end,color,reverse=False):
    draw.line([start,end],fill=color,width=7)
    angle=math.atan2(end[1]-start[1],end[0]-start[0])
    draw.polygon([end,(end[0]-22*math.cos(angle-.5),end[1]-22*math.sin(angle-.5)),
                  (end[0]-22*math.cos(angle+.5),end[1]-22*math.sin(angle+.5))],fill=color)

short={
 'Далее / закончить комикс':'Далее', 'Начать / понятно':'Начать',
 'Авто: цель этапа собрана':'Само: цель собрана', 'Авто: завершён 3-й этап':'Само: 3 этапа готовы',
 'Авто: завершён 1-й / 2-й этап':'Само: этап готов', 'Авто: проигрыш':'Само: проигрыш',
 'Ниже · перейти к этапу':'Ниже', 'Тап по острову':'Нажать остров',
 'Выбрать тапом → посмотреть':'Выбрать, затем Посмотреть', 'Выбрать → посмотреть':'Выбрать, затем Посмотреть',
 'Кнопка паузы II':'Пауза', 'Тап по цели':'Нажать цель', 'Тап по сумке':'Нажать сумку',
 'Тап предмета / повторный тап сумки':'Выбрать предмет / закрыть сумку',
 'Decline / отказаться':'Отказаться', 'Accept / принять':'Принять',
 'Авто: время предложения миссии':'Само: новая миссия',
 'Авто: сюжетное событие':'Само: разговор',
 'Наше путешествие':'Наше путешествие',
 'Далее · выбрана карта':'Далее: карта',
 'Далее · выбран следующий уровень':'Далее: следующий уровень',
 'Карта · завершить / покинуть миссию':'Карта: выйти из миссии',
 'Назад · вход из меню':'Назад в меню',
 'Назад · вход из паузы':'Назад в паузу',
 'Закрыть · вход после уровня':'Закрыть: карта',
 'Закрыть · вход из магазина':'Закрыть: магазин',
}
def action_text(action,kind):
    result=short.get(action,action.replace('Авто:', 'Само:').replace('Тап уровня', 'Выбрать уровень').replace('Тап по', 'Нажать'))
    if kind=='return' and not result.lower().startswith(('назад','закрыть','продолжить','далее','обратная')):
        result='Возврат: '+result
    return result

textblock('Карта приложения',M,45,W-2*M,72,True)
textblock('Читай слева направо. На стрелке — что нажать.',M,145,W-2*M,40)
textblock('Каждый блок — один маршрут. Один экран может встречаться снова.',M,210,W-2*M,34,color=GREY)
textblock('«Само» — переход без нажатия. Возврат показан под карточками.',M,265,W-2*M,34,color=GREY)
textblock('Снимки: 9 октября 2026. Текущая версия Unity. Отдельный тестовый профиль.' if fresh else 'Снимки: 8 октября. Связи: код от 9 октября. Два новых окна пока без снимков.',M,335,W-2*M,28,color=GREY)

base=TOP
for si, ((title,note,items,links),panels) in enumerate(zip(sections,all_panels)):
    title=title[4:]
    draw.rectangle((M,base+5,M+12,base+98),fill=TEAL)
    textblock(str(si+1).zfill(2)+'  '+title,M+40,base,W-2*M-40,51,True)
    # Only one brief orientation sentence; the routes carry the rest.
    notes=[
      'От меню до первого уровня.', 'Три этапа. Потом награда и выбор пути.',
      'Что можно открыть, пока играешь.', 'Выбор мира, режима и личной миссии.',
      'Где вещи, трофеи, посылки и рекорды.', 'Выбрать предмет. Посмотреть. Купить или надеть.',
      'Открыть раздел. Изменить. Вернуться.', 'Помощь и восстановление сохранения.',
      'Рекламные переходы. Отдельно — экраны без кнопки входа.'
    ]
    textblock(notes[si],M+40,base+83,W-2*M-40,31,color=GREY)
    py=base+HEAD
    for pi,panel in enumerate(panels):
        px=M+(pi%2)*(PW+GAP);y=py+(pi//2)*(PH+GAP)
        draw.rounded_rectangle((px,y,px+PW,y+PH),24,fill=PAPER,outline='#C6CBC6',width=2)
        textblock(f'{si+1}.{pi+1}  Маршрут',px+30,y+20,PW-60,29,True,color=GREY)
        ids=panel['ids'];startx=px+(PW-((len(ids)-1)*STEP+CW))//2
        boxes=[]
        for ci,ni in enumerate(ids):
            stem,caption,context=items[ni];x=startx+ci*STEP;cy=y+83
            box=(x,cy,x+CW,cy+CH);boxes.append(box);card_bounds.append(box)
            draw.rounded_rectangle(box,14,fill='#FFFFFF',outline='#B9C3BE',width=2)
            # Large step numeral, left aligned title and predictable hierarchy.
            draw.text((x+16,cy+12),str(ci+1),font=face(29,True),fill=TEAL)
            endtitle=textblock(caption,x+16,cy+55,CW-32,32,True,leading=41)
            if endtitle>cy+178:checks.append('Title overflows: '+caption)
            sy=cy+183;sh=CH-205;path=captures.get(stem)
            if path:
                if stem not in thumbs:
                    pic=Image.open(path).convert('RGB');pic.thumbnail((CW-16,sh),Image.Resampling.LANCZOS);thumbs[stem]=pic
                pic=thumbs[stem];canvas.paste(pic,(x+(CW-pic.width)//2,sy+(sh-pic.height)//2))
            else:
                draw.rectangle((x+8,sy,x+CW-8,sy+sh),fill='#F1E7D4')
                textblock('Кадра пока нет',x+25,sy+90,CW-50,33,True)
                textblock('Окно есть в коде. Свежий снимок ещё не снят.',x+25,sy+220,CW-50,28,color=GREY)
        for ci,ei in enumerate(panel['forward']):
            a,b,action,_,_,kind=links[ei];left,right=boxes[ci],boxes[ci+1]
            color=AMBER if kind=='event' else TEAL
            x=left[2]+13;labelw=STEP-CW-26;yy=y+290
            caption=action_text(action,kind)
            textend=textblock(caption,x,yy,labelw,28,True,color=color,leading=38)
            label_bounds.append((x,yy,x+labelw,textend))
            arrow_y=max(y+550,textend+38)
            if arrow_y>y+790:checks.append('Action too long: '+action)
            arrow((left[2]+16,arrow_y),(right[0]-16,arrow_y),color)
            painted.append((si,ei))
        for ei in panel['backwards']:
            a,b,action,_,_,kind=links[ei];ia,ib=ids.index(a),ids.index(b)
            ax=boxes[ia][0]+CW/2;bx=boxes[ib][0]+CW/2;ay=y+978
            color=GREY
            # A separate return lane touches the bottom of its two cards only.
            draw.line([(ax,boxes[ia][3]+8),(ax,ay),(bx,ay),(bx,boxes[ib][3]+8)],fill=color,width=5)
            arrow((bx,ay),(bx,boxes[ib][3]+8),color)
            return_end=textblock(action_text(action,kind),bx+20,y+865,max(100,ax-bx-40),26,color=color,leading=35)
            if return_end>ay-6:checks.append('Return label meets its arrow: '+action)
            painted.append((si,ei))
        # Isolated archival screens still appear as cards, with an explicit no-entry status below.
    used={ni for p in panels for ni in p['ids']}
    orphan=[ni for ni in range(len(items)) if ni not in used]
    base=py+math.ceil(len(panels)/2)*(PH+GAP)+80
    if orphan:
        # Reserved separately in the final archival strip below, avoiding a false arrow.
        pass

# Add the four obsolete isolated windows, still required for a complete design inventory.
archive=sections[-1]
orphans=[archive[2][ni] for ni in range(len(archive[2]))
         if not any(edge[0]==ni or edge[1]==ni for edge in archive[3])]
if orphans:
    old=canvas;extra=1000;canvas=Image.new('RGB',(W,H+extra),BG);canvas.paste(old,(0,0));draw=ImageDraw.Draw(canvas)
    y=H-70
    textblock('Экраны без кнопки входа в текущем меню' if fresh else 'Архив — нет кнопки входа в текущем меню',M,y,W-2*M,40,True,color=GREY)
    for i,(stem,caption,context) in enumerate(orphans):
        x=M+i*1040;cy=y+90
        draw.rounded_rectangle((x,cy,x+970,cy+760),16,fill=PAPER,outline='#C6CBC6',width=2)
        textblock(caption,x+25,cy+20,920,32,True)
        pic=Image.open(captures[stem]).convert('RGB');pic.thumbnail((300,590),Image.Resampling.LANCZOS)
        canvas.paste(pic,(x+25,cy+115))
        textblock('Нет входа',x+355,cy+140,580,31,True,color=GREY)
        textblock(context,x+355,cy+220,580,29,color=GREY)
    H+=extra

expected={(si,ei) for si,row in enumerate(sections) for ei in range(len(row[3]))}
if set(painted)!=expected or len(painted)!=len(expected):checks.append('Lost or repeated transitions')
for label in label_bounds:
    for box in card_bounds:
        if max(label[0],box[0])<min(label[2],box[2]) and max(label[1],box[1])<min(label[3],box[3]):
            checks.append('Action overlaps a screen')
if checks:raise RuntimeError(checks)
output=OUT/'cascade-navigation-clear.png';canvas.save(output,optimize=True)
preview=canvas.copy();preview.thumbnail((1400,3000));preview.save('/private/tmp/cascade-clear-overview.jpg',quality=92)
# Detail crop used for the bounded visual check; no extra user deliverable.
canvas.crop((0,TOP,W,TOP+HEAD+PH+60)).resize((2200,(HEAD+PH+60)//2)).save('/private/tmp/cascade-clear-detail.jpg',quality=94)
(OUT/'navigation-clear-check.json').write_text(json.dumps({'width':W,'height':H,'routes':sum(map(len,all_panels)),
    'transitions':len(painted),'screensWithoutEntry':len(orphans),'errors':checks,'captures':'fresh 2026-10-09' if fresh else 'archived 2026-10-08'},indent=2))
(OUT/'navigation-clear-sources.json').write_text(json.dumps({stem:str(path.relative_to(ROOT)) for stem,path in captures.items()},ensure_ascii=False,indent=2))
print(f'{output}: {W} x {H}; {sum(map(len,all_panels))} routes; {len(painted)} transitions; no overlapping action labels')
