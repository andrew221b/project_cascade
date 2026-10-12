"""Checks published fixture data; does not perform native-speaker/editorial review."""
from pathlib import Path
import json,unicodedata
from cascade_model import from_dict,analyze
R=Path(__file__).resolve().parents[1]
alphabets={'en':set('ABCDEFGHIJKLMNOPQRSTUVWXYZ'),'ru':set('АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ'),'uk':set('АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯ')}
checks=0;words={}
for loc in alphabets:
 for w in json.loads((R/'Data'/f'words_{loc}.json').read_text()):
  assert w['id'] not in words
  assert w['locale']==loc and unicodedata.normalize('NFC',w['text'])==w['text']
  assert ''.join(w['tokens'])==w['text'] and set(w['text'])<=alphabets[loc]
  assert 3<=len(w['tokens'])<=8
  checks+=4;words[w['id']]=w
nodes=json.loads((R/'Data/levels.json').read_text());variants=json.loads((R/'Data/localized_levels.json').read_text())
assert len({x['id'] for x in nodes})==40
assert len({x['id'] for x in variants})==117
for v in variants:
 l=from_dict(v);l.validate();assert analyze(l)['solvable'];checks+=2
 for wid in v['word_ids']:assert wid in words;checks+=1
assert set(x['locale'] for x in variants)=={'en','ru','uk'}
report={'static_assertions':checks+3,'words':len(words),'primary_levels':len(nodes),'localized_variants':len(variants),
        'scope':'Restricted EN/RU/UK fixture alphabet, IDs, NFC, concrete solvability. Not font or native-language QA.'}
(R/'Reports/data_audit.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,ensure_ascii=False))
