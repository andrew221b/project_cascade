#!/usr/bin/env python3
"""Apply the reviewed three-language story; retain receipt/route and StringTable IDs."""
from pathlib import Path
import json,re
ROOT=Path(__file__).resolve().parents[1]
CONTENT=ROOT/'CascadeUnity/Assets/_Cascade/Content'
PRESENTATION=ROOT/'CascadeUnity/Assets/_Cascade/Presentation'
SOURCE=ROOT/'Data/story_lantern_oct16.json'
LITERAL=re.compile(r'"((?:[^"\\]|\\.)*)"')
def literals(s):return [json.loads('"'+v+'"') for v in LITERAL.findall(s)]
def array(s,name):return literals(re.search(r'static readonly string\[\] '+name+r'=\{(.*?)\};',s,re.S).group(1))
def main():
    data=json.loads(SOURCE.read_text());sections=data['sections']
    # The first application records the old lookup key for each authored slot.
    if 'keys' not in data:
        story=(CONTENT/'StoryConversations.cs').read_text()
        comic=(CONTENT/'WorldPrologues.cs').read_text()
        keys={
            'comics':[json.loads('"'+v+'"') for v in re.findall(r'new StoryLine\("[^"\n]+","((?:[^"\\]|\\.)*)"',comic)],
            'captions':[json.loads('"'+v+'"') for v in re.findall(r'new WorldComicPage\("((?:[^"\\]|\\.)*)"',comic)],
            'openings':array(story,'OpeningLines'),'payoffs':array(story,'IslandLines'),
            'goals':array(story,'Goals'),'asides':array(story,'AsideLines'),
            'progress':array((CONTENT/'StoryConversations.IslandProgress.cs').read_text(),'IslandProgressLines'),
            'parallel_first':array((CONTENT/'StoryConversations.ParallelSkies.cs').read_text(),'ParallelFirst'),
            'parallel_return':array((CONTENT/'StoryConversations.ParallelSkies.cs').read_text(),'ParallelReturn'),
            'missions':array((PRESENTATION/'CascadeGame.PersonalMissions.cs').read_text(),'MissionInvitations')}
        banter=array(story,'BanterLines');keys['checkpoints']=[v for i,v in enumerate(banter) if i%4<2]
        data['keys']=keys
        SOURCE.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n')
    updates={};renames={}
    rename_file=ROOT/'Data/story_lantern_key_renames.json'
    previous=json.loads(rename_file.read_text()) if rename_file.exists() else data.get('previous_applied_keys',{})
    history=data.setdefault('key_history',{})
    for old,copy in [(k,r) for name,keys in data['keys'].items() for k,r in zip(keys,sections[name])]:
        aliases=history.setdefault(old,[])
        for alias in [old,data.get('previous_applied_keys',{}).get(old),previous.get(old)]:
            if alias and alias not in aliases:aliases.append(alias)
    SOURCE.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n')
    for section,keys in data['keys'].items():
        assert len(keys)==len(sections[section]),section
        for old,copy in zip(keys,sections[section]):
            assert len(copy)==3 and all(t.strip() and '\t' not in t and '\n' not in t for t in copy),(section,copy)
            assert not ('{0}' in old) or all(t.count('{0}')==1 for t in copy)
            assert old not in updates or updates[old]==copy,(old,section)
            updates[old]=copy
            if old!=copy[0]:renames[old]=copy[0]
    assert len(set(renames.values()))==len(renames),'New English story keys must remain distinct'
    table=ROOT/'Data/ui_localization.tsv';rows=[line.split('\t') for line in table.read_text().splitlines()]
    assert all(len(r)==4 for r in rows)
    existing={r[0] for r in rows[1:]}
    assert all(old in existing or new in existing or previous.get(old) in existing for old,new in renames.items())
    assert not {new for old,new in renames.items() if new in existing and old in existing},'Existing target key collision'
    by_new={copy[0]:copy for copy in updates.values()}
    revisions={previous[old]:copy[0] for old,copy in updates.items() if old in previous and previous[old]!=copy[0]}
    for row in rows[1:]:
        if row[0] in revisions:row[0]=revisions[row[0]]
    for row in rows[1:]:
        old=row[0]
        if old in updates:row[:]=[updates[old][0]]+updates[old]
        elif old in by_new:row[1:]=by_new[old]
        # Internal character/art identity remains Mika. Only the player-facing name changes.
        for c in range(1,4):
            row[c]=row[c].replace('Микой','Рин').replace('Мікою','Рін').replace('Mika','Rin').replace('Мики','Рин').replace('Мике','Рин').replace('Мику','Рин').replace('Мика','Рин').replace('Міки','Рін').replace('Міці','Рін').replace('Міку','Рін').replace('Міко','Рін').replace('Міка','Рін')
    for row in rows[1:]:
        if row[0]=='Mika':row[1:]=['Rin','Рин','Рін']
        if row[0]=='Lumi':row[1:]=['Lumi','Люми','Люмі']
    for key,copy in data.get('supporting_copy',{}).items():
        row=next((r for r in rows[1:] if r[0]==key),None)
        if row is None:rows.append([key]+copy)
        else:row[1:]=copy
    table.write_text('\n'.join('\t'.join(r) for r in rows)+'\n')
    # English code literals remain readable on cold-load before the locale tables resolve.
    touched=[]
    def replace_literal(match):
        try:value=json.loads(match.group())
        except ValueError:return match.group()  # C# verbatim regex, not an authored sentence.
        return json.dumps(renames.get(value,revisions.get(value)),ensure_ascii=False) if value in renames or value in revisions else match.group()
    for file in (ROOT/'CascadeUnity/Assets/_Cascade').rglob('*.cs'):
        source=file.read_text()
        result=LITERAL.sub(replace_literal,source)
        if result!=source:file.write_text(result);touched.append(str(file.relative_to(ROOT)))
    # Speakers and inline completion screens share the same authored source.
    comic=CONTENT/'WorldPrologues.cs'
    speakers=iter([v for w in data['comic_speakers'] for v in w])
    comic.write_text(re.sub(r'new StoryLine\(\"[^\"\n]+\",',lambda m:'new StoryLine('+json.dumps(next(speakers))+',',comic.read_text()))
    completion=PRESENTATION/'CascadeGame.WorldCompletion.cs'
    source=completion.read_text();start=source.index('        UiCopy WorldCompletionTitle(')
    functions=[]
    for method,section in [('WorldCompletionTitle','completion_titles'),('WorldCompletionBody','completion_bodies')]:
        text='        UiCopy '+method+'(int world)\n        {\n'
        for i,locale in enumerate(('en','ru','uk')):
            text+='            string[] '+locale+'={\n'+',\n'.join('                '+json.dumps(row[i].replace('\\n','\n'),ensure_ascii=False) for row in sections[section])+'};\n'
        text+='            return ReferenceCopy(en[world],ru[world],uk[world]);\n        }\n';functions.append(text)
    completion.write_text(source[:start]+''.join(functions)+'    }\n}\n')
    # Build-time key rename uses SharedTableData.RenameKey, preserving numeric table IDs.
    (ROOT/'Data/story_lantern_key_renames.json').write_text(json.dumps(renames,ensure_ascii=False,indent=2)+'\n')
    # Companion source exports must not retain stale duplicates which can shadow the canonical table.
    final={r[0]:r for r in rows[1:]}
    for name in ('star_story_localization.tsv','parallel_skies_localization.tsv','story_revision_oct09.tsv','story_polish_oct15.tsv'):
        file=ROOT/'Data'/name
        if not file.exists():continue
        companions=[line.split('\t') for line in file.read_text().splitlines()]
        for row in companions[1:]:
            key=revisions.get(row[0],renames.get(row[0],row[0]))
            if key in final:row[:]=final[key]
        file.write_text('\n'.join('\t'.join(r) for r in companions)+'\n')
    folder=ROOT/'Reports/LanternStoryOct16';folder.mkdir(parents=True,exist_ok=True)
    report={'authored_slots':sum(len(v) for v in sections.values()),'localized_values':3*sum(len(v) for v in sections.values()),'lookup_keys_renamed':len(renames),'source_files_updated':touched,'max_progress_english_chars':max(len(r[0]) for r in sections['progress'])}
    (folder/'source-application.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
    print(json.dumps(report,ensure_ascii=False,indent=2))
if __name__=='__main__':main()
