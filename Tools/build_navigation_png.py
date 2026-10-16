#!/usr/bin/env python3
"""One raster navigation map: genuine captures, named actions, explicit returns."""
from pathlib import Path
import json, math
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Reports/ScreenRoadmap-Oct09'
records = json.loads((OUT / 'manifest.json').read_text())
sources = {r['file'].removesuffix('.png'): ROOT / r['source'] for r in records if r['source']}
W, ROW, CW, CH = 5200, 1450, 310, 810
FONT = '/System/Library/Fonts/Supplemental/Arial.ttf'
BOLD = '/System/Library/Fonts/Supplemental/Arial Bold.ttf'
font = lambda n, bold=False: ImageFont.truetype(BOLD if bold else FONT, n)
INK, RETURN, EVENT = '#174b46', '#53758c', '#a66b20'
rows = []

def section(title, note, nodes, edges):
    rows.append((title, note, nodes, edges))

# Node: capture stem, caption, short context. Edges: source, destination, action,
# route (middle/top/bottom), track, kind (tap/return/event).
section('01  Начало игры', 'Играть открывает карту. Комикс и урок появляются только когда нужны.', [
 ('01-home','Главное меню',''), ('29-comic','Комикс мира','Первое знакомство с миром'),
 ('08-map','Карта','Тап на остров Мики'), ('10-island','Уровни острова','Тап на открытый уровень'),
 ('33-tutorial','Обучение механике','Первое знакомство с эффектом'), ('20-game','Игровой этап',''),
 ('24-checkpoint','Этап завершён','После 1-го / 2-го этапа'), ('25-victory','Уровень завершён','После 3-го этапа')], [
 (0,1,'Играть · первый вход','middle',0,'tap'), (1,2,'Далее / закончить комикс','middle',0,'tap'),
 (0,2,'Играть · история уже прочитана','top',0,'tap'), (2,3,'Тап по острову','middle',0,'tap'),
 (3,4,'Тап уровня · новый эффект','middle',0,'tap'), (4,5,'Начать / понятно','middle',0,'tap'),
 (3,5,'Тап уровня · эффект уже знаком','top',1,'tap'), (2,5,'Продолжить · текущий уровень','bottom',0,'tap'),
 (5,6,'Авто: цель этапа собрана','middle',0,'event'), (5,7,'Авто: завершён 3-й этап','top',2,'event'),
 (3,2,'Назад','bottom',1,'return')])

section('02  Три этапа → награда → продолжение', 'Следующий этап остаётся внутри уровня. После победы игрок выбирает следующий уровень или карту.', [
 ('24-checkpoint','Этап завершён','1-й или 2-й этап'), ('20-game','Следующий этап','То же игровое окно, новая задача'),
 ('25-victory','Победа и статистика',''), ('27-parcel','Посылка','Если заработана новая посылка'),
 ('28-parcel-open','Посылка открыта','Награда сохраняется'), ('08-map','Карта',''),
 ('26-failure','Проигрыш','Боковая ветка игры'), ('39-story-ending','Финал путешествия','После завершения всей истории')], [
 (0,1,'Ниже · перейти к этапу','middle',0,'tap'), (1,0,'Авто: завершён 1-й / 2-й этап','top',0,'event'),
 (1,2,'Авто: завершён 3-й этап','middle',0,'event'), (2,3,'Следующий уровень / карта · есть подарок','middle',0,'tap'),
 (3,4,'Открыть','middle',0,'tap'), (4,5,'Далее · выбрана карта','middle',0,'tap'),
 (2,5,'Карта · без новой посылки','top',1,'tap'), (2,1,'Следующий уровень · без подарка','bottom',0,'tap'),
 (4,1,'Далее · выбран следующий уровень','bottom',1,'tap'), (1,6,'Авто: проигрыш','bottom',2,'event'),
 (6,1,'Заново','bottom',3,'tap'), (6,5,'Карта','middle',0,'tap'),
 (5,7,'Авто: история полностью завершена','top',2,'event'), (7,5,'Наше путешествие','bottom',4,'tap')])

section('03  Действия внутри уровня', 'Пауза открывает отдельное меню. Игровая сумка раскрывается над кнопками и повторным тапом сворачивается.', [
 ('20-game','Игра',''), ('41-target','Увеличенная цель','Наложение поверх игры'),
 ('23-pause','Пауза',''), ('19-supplies','Предметы / инструменты','Каталог из паузы'),
 ('11a-shop-shelf','Магазин','Из каталога предметов'), ('inline-bag','Сумка над кнопками','Новое окно: снимка ещё нет'),
 ('21-game-words','Игра: слова','Вариант поля, не отдельное меню'), ('22-game-mixed','Игра: микс','Вариант поля, не отдельное меню')], [
 (0,1,'Тап по цели','middle',0,'tap'), (1,0,'Закрыть цель','top',0,'return'),
 (0,2,'Кнопка паузы II','top',1,'tap'), (2,0,'Продолжить / заново','bottom',0,'return'),
 (2,3,'Мои инструменты','middle',0,'tap'), (3,0,'Выбрать → использовать','bottom',1,'tap'),
 (3,4,'Магазин','middle',0,'tap'), (4,3,'Назад','top',2,'return'),
 (0,5,'Тап по сумке','bottom',2,'tap'), (5,0,'Тап предмета / повторный тап сумки','bottom',3,'tap')])

section('04  Миры, диалоги и личные миссии', 'Режим меняется на карте; сам уровень открывается тапом в списке острова. Личная миссия появляется как диалог.', [
 ('08-map','Карта','Фигуры / слова / микс — выбор режима'), ('30-dialogue','Разговор с героем','Событие истории, без кнопки «Поговорить»'),
 ('mission-offer','Предложение миссии','Новое окно: снимка ещё нет'), ('20-game','Игра / личная миссия','Для миссии: свои 3 этапа'),
 ('10-island','Список уровней','Режим уже выбран на карте'), ('21-game-words','Уровень со словами',''),
 ('22-game-mixed','Смешанный уровень',''), ('09-map-night','Другой открытый мир','Тот же экран карты')], [
 (0,1,'Авто: сюжетное событие','middle',0,'event'), (1,0,'Далее / закрыть разговор','top',0,'return'),
 (0,2,'Авто: время предложения миссии','top',1,'event'), (2,0,'Decline / отказаться','bottom',0,'tap'),
 (2,3,'Accept / принять','middle',0,'tap'), (3,0,'Карта · завершить / покинуть миссию','bottom',1,'return'),
 (0,4,'Тап по острову','bottom',2,'tap'), (4,5,'Тап уровня · выбран режим «Слова»','middle',0,'tap'),
 (4,6,'Тап уровня · выбран «Микс»','top',2,'tap'), (4,3,'Тап уровня · выбран режим «Фигуры»','bottom',3,'tap'),
 (0,7,'Стрелка мира / свайп','top',3,'tap'), (7,0,'Обратная стрелка / свайп','bottom',4,'return')])

section('05  Мои вещи, журнал и коллекция', 'Журнал в текущем коде находится внутри «Моих вещей». Трофей выбирается тапом; отдельное нажатие открывает его просмотр.', [
 ('01-home','Главное меню',''), ('17-bag','Мои вещи',''), ('34-journal','Журнал / кабинет',''),
 ('37-records','Мои пути / рекорды',''), ('12a-trophy-shelf','Каталог трофеев','Общая сетка магазина / коллекции'),
 ('18-trophies','Просмотр трофея',''), ('27-parcel','Посылка','Если есть неоткрытая'),
 ('20-game','Переигрывание уровня','Из записи в рекордах')], [
 (0,1,'Мои вещи','middle',0,'tap'), (1,0,'Назад','top',0,'return'),
 (1,2,'Мой журнал','middle',0,'tap'), (2,1,'Назад','bottom',0,'return'),
 (2,3,'Мои пути','middle',0,'tap'), (3,2,'Назад','top',1,'return'),
 (1,4,'Трофеи','bottom',1,'tap'), (2,4,'Трофеи · нет посылки','top',2,'tap'),
 (4,5,'Выбрать тапом → посмотреть','middle',0,'tap'), (5,4,'Назад из просмотра','bottom',2,'return'),
 (2,6,'Открыть посылку','bottom',3,'tap'), (6,2,'Открыть → далее','bottom',4,'return'),
 (3,7,'Пройти уровень','top',3,'tap')])

section('06  Магазин: выбор → просмотр → действие', 'Тап на предмет выделяет его. «Посмотреть» открывает подробности. Покупка и надевание сохраняют этот экран.', [
 ('01-home','Главное меню',''), ('11a-shop-shelf','Инструменты: каталог','Тап меняет выбранный предмет'),
 ('11-shop-tools','Инструмент: просмотр','Купить → остаётся здесь'), ('12a-trophy-shelf','Трофеи: каталог','Мика / фон / рамки'),
 ('12-shop-mika','Костюм: просмотр','Купить / надеть → остаётся здесь'), ('13-shop-background','Фон и живность','Вкладка трофеев'),
 ('14-shop-frames','Рамки','Вкладка трофеев'), ('15-coins','Пакеты монет','Покупка → остаётся здесь')], [
 (0,1,'Магазин','middle',0,'tap'), (1,0,'Назад · вход из меню','bottom',0,'return'),
 (1,2,'Выбрать → посмотреть','middle',0,'tap'), (2,1,'Назад из просмотра','top',0,'return'),
 (1,3,'Вкладка «Трофеи»','bottom',1,'tap'), (3,4,'Выбрать → посмотреть','middle',0,'tap'),
 (4,3,'Назад из просмотра','top',1,'return'), (3,5,'Вкладка «Фон»','top',2,'tap'),
 (3,6,'Вкладка «Рамки»','bottom',2,'tap'), (5,3,'Вкладка «Мика»','bottom',3,'tap'),
 (6,3,'Вкладка «Мика»','top',3,'tap'), (1,7,'Вкладка «Монеты»','bottom',4,'tap'),
 (7,1,'Назад / «Помощь»','top',4,'return')])

section('07  Настройки', 'Разделы открываются из списка. Назад из раздела возвращает в список настроек; затем — к экрану, откуда вошли.', [
 ('01-home','Главное меню','Также вход из паузы'), ('02-settings','Список настроек',''),
 ('03-settings-0','Звук и движение','Переключатели меняют это окно'), ('04-settings-1','Удобство и помощь','Переключатели меняют это окно'),
 ('05-settings-2','Языки','Тап языка применяет выбор здесь'), ('06-settings-3','Напоминания',''),
 ('07-settings-4','Облачное сохранение',''), ('16-no-ads','Отключение рекламы','Покупка / восстановить → здесь')], [
 (0,1,'Настройки','middle',0,'tap'), (1,0,'Назад','bottom',0,'return'),
 (1,2,'Звук и движение','middle',0,'tap'), (1,3,'Удобство и помощь','top',0,'tap'),
 (1,4,'Языки','top',1,'tap'), (1,5,'Напоминания','top',2,'tap'),
 (1,6,'Облачное сохранение','top',3,'tap'), (1,7,'Без рекламы','top',4,'tap'),
 (2,1,'Назад','bottom',1,'return'), (3,1,'Назад','bottom',2,'return'),
 (4,1,'Назад','bottom',3,'return'), (5,1,'Назад','bottom',4,'return'),
 (6,1,'Назад','top',5,'return'), (7,1,'Назад','top',6,'return')])

section('08  Помощь и служебные окна', 'Обучение — наложение над игрой. Реклама возвращает к исходному экрану. Ошибки сохранения имеют отдельное окно восстановления.', [
 ('23-pause','Пауза',''), ('31-guide','Как играть',''), ('32-effect-guide','Эффекты и инструменты','Стрелки листают примеры здесь'),
 ('33-tutorial','Урок эффекта','Наложение над текущей игрой'), ('20-game','Игра',''),
 ('43-save-recovery','Восстановление сохранения','Появляется только при проблеме'),
 ('01-home','Главное меню','')], [
 (0,1,'Как играть','middle',0,'tap'), (1,0,'Назад','top',0,'return'),
 (1,2,'Инструменты и эффекты','middle',0,'tap'), (2,0,'Назад · вход из паузы','bottom',0,'return'),
 (4,3,'Авто: незнакомый эффект / предмет','middle',0,'event'), (3,4,'Начать / понятно','bottom',1,'tap'),
 (5,6,'Продолжить / начать заново','middle',0,'tap')])

section('09  Экраны без входа и рекламные переходы', 'Первые четыре карточки не имеют кнопки входа в текущих меню. Справа — два случая показа рекламы и возврат к месту входа.', [
 ('35-workshop','Мастерская','Нет кнопки входа в текущих меню'), ('38-contracts','Пути с печатями','Нет кнопки входа в текущих меню'),
 ('36-requests','Прежнее меню просьб','Заменено предложением на карте'), ('40-word-language','Подтверждение языка слов','Текущий выбор языка применяется сразу'),
 ('25-victory','Уровень завершён','Реклама возможна при выходе в карту'),
 ('42-test-ad','Рекламное окно','Архивный пример: тестовый вид SDK'),
 ('08-map','Карта',''), ('11a-shop-shelf','Магазин','Добровольное видео за монеты')], [
 (4,5,'Карта · наступил интервал рекламы','middle',0,'tap'), (5,6,'Закрыть · вход после уровня','middle',0,'return'),
 (7,5,'Видео за монеты · доступно','top',0,'tap'), (5,7,'Закрыть · вход из магазина','bottom',0,'return')])

H = 330 + len(rows)*ROW
image = Image.new('RGB',(W,H),'#eff1ed')
d = ImageDraw.Draw(image)
nodes, edges, labels = [], [], []

def wrap(text, face, width):
    result, line = [], ''
    for word in text.split():
        trial=(line+' '+word).strip()
        if line and d.textlength(trial,font=face)>width:
            result.append(line); line=word
        else: line=trial
    return result+[line] if line else result

def pill(text, x, y, color, width=285):
    lines=wrap(text,font(20,True),width-24)
    tw=max([d.textlength(s,font=font(20,True)) for s in lines] or [0])+24
    hh=24*len(lines)+16
    box=(int(x-tw/2),int(y-hh/2),int(x+tw/2),int(y+hh/2))
    if min(box)<0 or box[2]>W or box[3]>H: raise ValueError(('label outside canvas',text,box))
    d.rounded_rectangle(box,10,fill='#fbfcf8',outline=color,width=2)
    for i,s in enumerate(lines):
        d.text((x,y-hh/2+8+i*24),s,font=font(20,True),anchor='mt',fill=color)
    labels.append((text,box))

def arrow(points,color):
    d.line(points,fill=color,width=5,joint='curve')
    a,b=points[-2:]; angle=math.atan2(b[1]-a[1],b[0]-a[0])
    d.polygon([b,(b[0]-18*math.cos(angle-.45),b[1]-18*math.sin(angle-.45)),
                  (b[0]-18*math.cos(angle+.45),b[1]-18*math.sin(angle+.45))],fill=color)
    a=points[0];d.ellipse((a[0]-5,a[1]-5,a[0]+5,a[1]+5),fill=color)

d.rectangle((0,0,W,265),fill='#163e38')
d.text((70,35),'CASCADE · КАРТА ПЕРЕХОДОВ',font=font(52,True),fill='#fff5d8')
d.text((70,112),'Карточка = экран. На стрелке — нажатие или событие, которое открывает следующий экран.',font=font(28),fill='#e4eade')
d.text((70,163),'Снимки: архив 8 октября. Связи: текущий код. Повтор карточки означает тот же экран в другой ветке.',font=font(25),fill='#c8d5c9')
for x,color,text in [(70,INK,'Нажатие'),(450,RETURN,'Возврат'),(820,EVENT,'Автоматическое событие')]:
    d.line((x,223,x+60,223),fill=color if color!=INK else '#b5d6c4',width=6)
    d.text((x+76,206),text,font=font(24),fill='#fff5d8')

for ri,(title,note,items,connections) in enumerate(rows):
    base=300+ri*ROW
    d.rounded_rectangle((35,base,W-35,base+ROW-20),20,fill='#f8f9f4',outline='#c7d1c5',width=2)
    d.text((70,base+18),title,font=font(35,True),fill=INK)
    d.text((70,base+70),note,font=font(22),fill='#526659')
    boxes=[(85+i*635,base+290,85+i*635+CW,base+290+CH) for i in range(len(items))]
    pending_labels=[]
    for ai,bi,action,route,track,kind in connections:
        a,b=boxes[ai],boxes[bi];color=RETURN if kind=='return' else EVENT if kind=='event' else INK
        if route=='middle':
            right=bi>ai; y=base+650
            start=(a[2] if right else a[0],y);end=(b[0] if right else b[2],y)
            points=[start,end];lx=(start[0]+end[0])/2;ly=y-45
        else:
            top=route=='top'; y=base+(154+track*22 if top else 1140+track*40)
            # Shared bus joins are outside the photographed UI.
            start=(a[0]+CW/2,a[1] if top else a[3]);end=(b[0]+CW/2,b[1] if top else b[3])
            points=[start,(start[0],y),(end[0],y),end]
            lx=(start[0]+end[0])/2;ly=y-23 if top else y+21
        if action in ('Тап по сумке','Тап уровня · выбран режим «Фигуры»'):lx-=340
        arrow(points,color);pending_labels.append((action,lx,ly,color))
        edges.append({'from':f'{ri+1:02d}.{ai+1:02d}','to':f'{ri+1:02d}.{bi+1:02d}','action':action,'kind':kind})
    # Labels below and above arrows are placed before cards, so they cannot cover screenshots.
    for p in pending_labels:pill(*p)
    for ni,((stem,caption,context),box) in enumerate(zip(items,boxes)):
        x,y,x2,y2=box;identity=f'{ri+1:02d}.{ni+1:02d}'
        source=sources.get(stem);missing=source is None
        d.rounded_rectangle((x+4,y+7,x2+4,y2+7),14,fill='#d9dfd4')
        d.rounded_rectangle(box,14,fill='#ffffff',outline='#b9c8bd' if not missing else '#b68e5b',width=2)
        d.text((x+12,y+9),identity,font=font(18,True),fill='#6f836f')
        lines=wrap(caption,font(23,True),CW-22)
        for j,s in enumerate(lines[:2]):d.text((x+12,y+32+j*26),s,font=font(23,True),fill=INK)
        sy=y+92;sh=CH-172
        if source:
            pic=Image.open(source).convert('RGB');pic.thumbnail((CW-12,sh),Image.Resampling.LANCZOS)
            image.paste(pic,(x+(CW-pic.width)//2,sy+(sh-pic.height)//2))
        else:
            d.rounded_rectangle((x+8,sy,x2-8,sy+sh),12,fill='#f6eddf',outline='#b68e5b',width=2)
            text='СНИМКА ПОКА НЕТ\n\nОкно реализовано\nв коде; свежий кадр\nещё не снят.'
            d.multiline_text((x+CW/2,sy+sh/2),text,font=font(23),anchor='mm',align='center',fill='#856340',spacing=12)
        if context:
            for j,s in enumerate(wrap(context,font(17),CW-22)[:3]):
                d.text((x+11,y2-70+j*20),s,font=font(17),fill='#657565')
        nodes.append({'id':identity,'title':caption,'source':str(source.relative_to(ROOT)) if source else None,'bounds':box})
    # Quiet footnotes spell out remaining origin-dependent returns without inventing routes.
    foot={2:'Пауза: «Настройки» → 07.02; «Главная» → 01.01. В словах и миксе действуют те же кнопки.',
          4:'Из «Моих вещей»: «Предметы» → 03.04. «Назад» из коллекции возвращает к месту входа.',
          6:'«Назад» из «Напоминаний», «Облака» и «Без рекламы» → список настроек. Из списка — в меню или паузу.',
          7:'Реклама: из магазина — добровольное видео → магазин; между играми — закрыть → карта. Урок: закрыть → та же игра.'}.get(ri)
    if foot:d.text((70,base+ROW-56),foot,font=font(21),fill='#6c7a70')

path=OUT/'cascade-navigation.png'
image.save(path,optimize=True)
(OUT/'navigation-map.json').write_text(json.dumps({'width':W,'height':H,'captures':'archived 2026-10-08','nodes':nodes,'transitions':edges},ensure_ascii=False,indent=2))
preview=image.copy();preview.thumbnail((1300,3200));preview.save('/private/tmp/cascade-navigation-preview.jpg',quality=90)
overlaps=[]
for i,(text,a) in enumerate(labels):
    for other,b in labels[i+1:]:
        if max(a[0],b[0])<min(a[2],b[2]) and max(a[1],b[1])<min(a[3],b[3]):
            overlaps.append([text,other])
    for node in nodes:
        b=node['bounds']
        if max(a[0],b[0])<min(a[2],b[2]) and max(a[1],b[1])<min(a[3],b[3]):
            overlaps.append([text,'card '+node['id']])
(OUT/'navigation-layout-check.json').write_text(json.dumps({'overlaps':overlaps,'nodes':len(nodes),'edges':len(edges)},ensure_ascii=False,indent=2))
if overlaps:print('Layout overlaps: '+json.dumps(overlaps,ensure_ascii=False))
print(f'{path}: {W}×{H}, {len(nodes)} cards, {len(edges)} labelled transitions, {sum(n["source"] is None for n in nodes)} missing captures; {path.stat().st_size//1048576} MiB')
