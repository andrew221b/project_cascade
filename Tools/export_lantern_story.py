#!/usr/bin/env python3
"""Export the Russian script in reading order, with speakers and optional beats."""
from pathlib import Path
import json
R=Path(__file__).resolve().parents[1];d=json.loads((R/'Data/story_lantern_oct16.json').read_text());s=d['sections']
world_keys=['Wind Gardens', 'Rain Mill', 'Copper Orchard', 'Cloud Post', 'Glass Lagoon', 'Moss Archive', 'Snow Spire', 'Great Beacon', 'Star Observatory']
worlds=[d['supporting_copy'][key][1] for key in world_keys]
residents=['Bram','Pip','Rue','Sora','Neri','Toma','Lumi','Gran','Aster']
names=dict(zip(['Mika']+residents,['Рин','Вальтер','Бен','Роза','Нора','Клара','Оскар','Люми','Бабушка Ада','Феликс']))
lines=['# Высший Фонарь: полный русский сценарий','', 'Спойлеры. Редактура причинно-следственных связей и диалогов от 21 октября 2026. Реплики перечислены в порядке появления; дополнительные задания и смена вида игры не содержат обязательных улик.','']
def say(who,text):lines.extend(['**'+names[who]+':** '+text,''])
def exchange(section,index,world):
 for turn,who in enumerate([residents[world],'Mika']):say(who,s[section][index+turn][1])
for w,world in enumerate(worlds):
 lines+=['## '+str(w+1)+'. '+world,'','### Вступительный комикс','']
 for p in range(4):
  lines+=['#### '+str(p+1)+'. '+s['captions'][w*4+p][1],'']
  for t in range(2):say(d['comic_speakers'][w][p*2+t],s['comics'][w*8+p*2+t][1])
 for island in range(5):
  index=(w*5+island)*2
  lines+=['### Остров '+str(island+1),'','**Цель:** '+s['goals'][w*5+island][1],'','Начало:',''];exchange('openings',index,w)
  if island==0:lines+=['Контрольный разговор между этапами второго уровня:',''];exchange('checkpoints',w*2,w)
  if island==3:lines+=['Дополнительный разговор после начала острова:',''];exchange('asides',w*2,w)
  lines+=['Развитие на шестом уровне:',''];exchange('progress',index,w)
  lines+=['Результат острова:',''];exchange('payoffs',index,w)
 lines+=['### Переход после мира','','**'+s['completion_titles'][w][1]+'**','',s['completion_bodies'][w][1].replace('\\n','  \n'),'','### Персональное задание (дополнительное)','']
 say(residents[w],s['missions'][w][1].replace('{0}','[Цель текущей механики.]'))
lines+=['## Смена вида игры и возвращение','']
for mode,name in enumerate(['Фигуры','Слова','Микс']):
 lines+=['### '+name,'','Первый переход:','']
 for t,who in enumerate(['Mika','Bram','Mika']):
  if t==1:lines+=['**Текущий житель мира:** '+s['parallel_first'][mode*3+t][1],'']
  else:say(who,s['parallel_first'][mode*3+t][1])
 lines+=['Возвращение:',''];say('Mika',s['parallel_return'][mode*2][1]);lines+=['**Текущий житель мира:** '+s['parallel_return'][mode*2+1][1],'']
(R/'Docs/LANTERN_STORY_FULL_RU_OCT16.md').write_text('\n'.join(lines))
print('Russian reading script exported:',len(lines),'lines')
