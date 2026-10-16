"""Apply authored dialogue edits to existing localization keys, retaining IDs and story receipts."""
from pathlib import Path
import json
import re

ROOT = Path(__file__).resolve().parents[1]
CONTENT = ROOT / 'CascadeUnity/Assets/_Cascade/Content'


def literals(source):
    return [json.loads('"'+s+'"') for s in re.findall(r'"((?:[^"\\]|\\.)*)"', source)]


def array(source, name):
    return literals(re.search(r'static readonly string\[\] '+name+r'=\{(.*?)\n        \};', source, re.S).group(1))


def main():
    edits = json.loads((ROOT / 'Data/story_polish_oct15.json').read_text())
    comic_source = (CONTENT / 'WorldPrologues.cs').read_text()
    comic_keys = [json.loads('"'+s+'"') for s in re.findall(r'new StoryLine\("[^"]+","((?:[^"\\]|\\.)*)"', comic_source)]
    assert len(comic_keys) == 72 and len(edits['comics']) == 9
    updates = {}
    for world, turns in enumerate(edits['comics']):
        assert len(turns) == 8
        for turn, copy in enumerate(turns):
            updates[comic_keys[world*8+turn]] = copy
    story_source = (CONTENT / 'StoryConversations.cs').read_text()
    for section, source_array, stride in [('checkpoint_first', 'BanterLines', 4), ('asides', 'AsideLines', 2)]:
        keys = array(story_source, source_array)
        assert len(edits[section]) == 9
        for world, turns in enumerate(edits[section]):
            assert len(turns) == 2
            for turn, copy in enumerate(turns):
                updates[keys[world*stride+turn]] = copy
    source_files={'IslandProgressLines':'StoryConversations.IslandProgress.cs'}
    for source_array, entries in edits.get('continuity', {}).items():
        source=(CONTENT/source_files[source_array]).read_text() if source_array in source_files else story_source
        keys=array(source,source_array)
        for index,copy in entries.items():
            updates[keys[int(index)]]=copy
    lines = (ROOT / 'Data/ui_localization.tsv').read_text().splitlines()
    rows = [line.split('\t') for line in lines]
    assert all(len(row) == 4 for row in rows)
    existing = {row[0] for row in rows[1:]}
    assert set(updates) <= existing
    for copy in updates.values():
        assert len(copy) == 3 and all(20 <= len(text) <= 210 and '\n' not in text and '\t' not in text for text in copy)
    for row in rows[1:]:
        if row[0] in updates:
            row[1:] = updates[row[0]]
    (ROOT / 'Data/ui_localization.tsv').write_text('\n'.join('\t'.join(row) for row in rows)+'\n')
    (ROOT / 'Data/story_polish_oct15.tsv').write_text('key\ten\tru\tuk\n'+'\n'.join('\t'.join([key]+value) for key, value in updates.items())+'\n')
    print(json.dumps(dict(edited_turns=len(updates), table_keys_preserved=len(existing),
                         max_characters={locale:max(len(v[i]) for v in updates.values()) for i, locale in enumerate(['en','ru','uk'])}), indent=2))


if __name__ == '__main__':
    main()
