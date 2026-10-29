from pathlib import Path
from collections import Counter
from urllib.request import urlopen
import json,re,hashlib,shutil,zipfile,os,argparse
parser=argparse.ArgumentParser();parser.add_argument('--input-dir',type=Path,required=True);parser.add_argument('--replace-unreleased-v1',action='store_true');args=parser.parse_args()
root=Path(__file__).resolve().parents[1];raw=args.input_dir;licenses=root/'Data/Licenses/StageWords';licenses.mkdir(parents=True,exist_ok=True)
# Keep the complete, permissive SCOWL notices; the adjacent generic en/license.txt is not this data licence.
(raw/'en-notices.txt').write_bytes(urlopen('https://raw.githubusercontent.com/LibreOffice/dictionaries/master/en/README_en_US.txt',timeout=40).read())
(raw/'mpl-1.1.txt').write_bytes(urlopen('https://www.mozilla.org/media/MPL/1.1/index.txt',timeout=40).read())
allows={
'en':'ago air and any are awe aye bar bay bow bye can day dew dry dye ear eat ego elf elk end era eve eye far fat few fit fix fly fog for fox fry fun fur gas gay gem get god got gum gut guy gym ham has hat hay hen her hey hid him hip his hit hog hot how hub hue hug hum hut ice icy ilk imp ink inn ion ire irk its ivy jam jar jaw jay jet jig job jog jot joy jug jut key kid kit lad lag lap law lay led leg let lid lie lip lit log lot low lug lyc mad man map mat may men met mid mix mob mom mop mow mud mug mum nap nay net new nil nip nod nor not now nub nun nut oak oar oat odd ode off oft oil old one opt orb ore our out owl own pad pal pan pat paw pay pea peg pen per pet pew pie pig pin pit ply pod pop pot pro pry pub pug pun pup purr put rag ram ran rap rat raw ray red rib rid rig rim rip rob rod roe rot row rub rue rug run rye sad sag sap sat saw say sea see set sew shy sin sip sir sit six ski sky sly sob sod son sop sot sow spa spy sty sub sue sum sun sup tab tag tan tap tar tax tea tee ten the thy tic tie tin tip toad toe tog ton too top tot tow toy try tub tug tun two use van vat vet vex via vie vow wad wag wan war was wax way web wed wee wet who why wig win wit woe wok won woo wot wow wry yah yak yam yap yaw yea yen yes yet yew yin yip yob yod yoke you yum zap zed zee zen zig zip zit zoo'.split(),
'ru':'без вам вас ваш век вес вид виз вот все вся где год гул дар два две для дно дух дым его ему ещё ешь жаль жар жест жук зал зев зов зоб зуд имя йод как кол ком кот кош край кто куб кум кун кур лад лаз лай лак лев лёд лет лещ лик лис лоб лог лом луг лук луч май мал мак мат мах мёд мел мех миг мир мог мол мой мох муж мыть нам нас наш наш неб нег неё нет них нос обо оба обе овёс ого одр она они орёл ось пар пас пах пел пёс пир пик пил пыл под пол пот про пух пуд путь раз рак рай рад раз ров род рок ром рос рот рог руб рой рык ряд сад сам сан сев сел сен сок сон сор сто суд сук суп сыр сыт таз так тал там тан тек тем тех тик тир тис тих ток том тон топ тор тот три тут туф тыл уже увы ужа узы ума умён ура усы уха ухо уют фаз фея фиг фок фол фон хор ход цап цех чан час чай чьи чья чью чьё чей чем чиж чип чин чих чту что щит щуп эко эль эму эра это эта эти эту юг юла юнга ярд яма'.split(),
'uk':'аби або акт акр але ані ану аул аут аїр бай бак бал бар бас беж без бей бек бик бич біг бій бік бір біс боб бог бод бон бор бот бік бук був буц вал вам вас ваш вби вез век вив вид вил вин вир вис вия віз вік він вісь віч вов вон вуж вуз вус гай гак гал гам гап газ гей ген гер гик гид гич гін гіп гір гість глу гну гук гул гус гуж дай дар дах два дві дея див дим дім дід дно дні доб дог док дол дот дух дуж дуб дув дул дяк дяр дьог еге ель ера есе ехо єси жар жах жва жди жук жим жир жить жін жуй жуп жмут зал зам зан зев зел зим зір зла зло змій зна зоб зой зум зуд зич зят їда їжа їмо їм іва іде іди іже іти йди йод йог йор ква кед кей кекс кив кий кил ким кіт ків кіл кіз кір кін кіп кол ком кон кор кош куб кум кун кур кут куш кущ лак лад лаз лай лан лап лев лед лис лів лік літ ліс лід ліз лоб лов лом лот луг луд лук луч люд лют льон мак май мал мам ман мат мах мед мер мет мех меч миг мир мис мить міст між міг мій мін міх мох мол мор мот мул муж мук нам нас наш ней нир низ ніс ніч ніж нім ніт нуд нут нюх ньо оба обо ого ода око олі она оса ось ота ото оті оце оцт оці оця пад пай пан пар пас пат паф пах паю пек пен пес пив пий пік піл пін пір піс піт піч плі пнi под поз пом поп пор пот про пру пря пуд пук пул пут пух пхе пяв пів пік пір рай рак ран рап рат рев рив риг риз рис рід рій рік рім рів ріс ріг рів річ риш род рок ром рот руд руж рух ряс рят сад сак сам сан сап сев сел сем сен сип сич сів сік сіл сін сір сіт сім скі сми сну соб сов сом сон сор сот соя сто сту суб суд сук сум суп суф суш сяд сюр таз так там тан тва тек тем тер тес тех тил тин тип тир тис тиш тік тім тін тіп тіт тіс тов ток тол том тон тор тож той топ тро три тру тух туз тут уби ува узи ужа узи умі ура уси уста уха ухи ухо уяв фар фас фах фен фес фет фік філ фін фіт фіч фол фон фук фур хай хам хан хат хек хід хиб хиж хлип хіт хід хлор хм хну хід хом хор хот хук цап цат цеб цех цим цин ців ціп цір цить чад чай чан чар час чат чва чей чеп чий чим чин чип чир чия чиє чок чом чує чую чух шар шан шат шах шей шив шин шип шир шик шия шкі шов шок шуб шум шур шух щем щеп щит щік щіп щой щук щум ют юла юнт юр юрк юрт юх ярд яра яре ярі ярм ярч яса ятв'.split()}
# A curated exception is accepted only if present in the downloaded dictionary.
blocks={
'en':['fuck','shit','cunt','dick','cock','piss','turd','fart','porn','sex','rape','slut','whore','nigg','nazi','suicid','murder','death','dead','kill','drug','heroin','cocaine','penis','vagina','erotic','fetish','abuse','molest','bastard','ass','boob','damn'],
'ru':['хуй','хуе','хуё','хер','пизд','жоп','бля','еба','ебл','ебн','ёба','сук','секс','порн','смерт','труп','убий','убить','наркот','суиц','нациз','эрот','дебил','идиот','гнил'],
'uk':['хуй','хує','хуе','хер','пизд','жоп','срак','бля','їба','єба','йоб','сук','секс','порн','смерт','труп','убив','вбив','наркот','суїц','нациз','ерот','дебіл','ідіот','гнил']}
alphabets={'en':r'[a-z]+','ru':r'[а-яё]+','uk':r'[а-щьюяєіїґ]+'}
warm={
'en':'CAT DOG SUN CUP MAP FOX OWL COW BAT HEN PIG ANT KEY SKY SEA TEA HAT BAG BOX CAR BUS PEN INK TOY JAM NUT YAK PIE OAK ELM RAM APE CAP TAP RED BOW STAR BIRD FROG FISH DUCK WIND LEAF SNOW RAIN MILK CAKE GIFT BLUE GOLD WAVE MINT LION BEAR HARE PLAY CLOUD PLANT BREAD SMILE GRAIN MOUSE LIGHT CROWN HORSE PEARL FRUIT MUSIC TIGER OCEAN HONEY MAGIC DAISY FAIRY HEART WORLD FLOWER GARDEN BRIGHT PLANET CASTLE FOREST ORANGE MEADOW STREAM SILVER FAMILY FRIEND'.split(),
'ru':'КОТ ЛЕС ДОМ СЫР НОС СОК ЛУГ ЛУК ЖУК ЛЁД ЧАЙ МЁД РОГ МЯЧ СОН СУП КИТ БЫК ДАР ВЕС МИР ШАР РОТ ЗАЛ МАЙ КУБ ТАЗ ПАР ДУБ ЛЕВ ЛИС ЛУНА ЛИСА РОЗА РУКА НОГА СТОЛ СТУЛ РЕКА ГОРА СОВА СЛОН ТИГР КРАН ПОЛЕ МОРЕ НЕБО ЛИСТ РЫБА ЗИМА ЛЕТО МАЯК ОКЕАН ЛИМОН ВЕСНА РУЧЕЙ ПЕСОК МЕЧТА ЗВУКИ ВЕТКА ПТИЦА ЗЕБРА ЛОДКА БЕРЁЗА ЦВЕТОК ЛИСТОК ДОБРЫЙ УЛЫБКА ВОЗДУХ ДРУЖБА ПРАЗДНИК'.split(),
'uk':'КІТ ЛІС ДІМ СИР НІС СІК ЛУГ ЛУК ЖУК ЛІД ЧАЙ МЕД РІГ СОН СУП КИТ БИК ДАР МИР РОТ ЗАЛ МАЙ КУБ ДУБ ЛЕВ ЛИС РІК РІЧ РУКА НОГА СТІЛ РІКА ГОРА СОВА СЛОН ТИГР КРАН ПОЛЕ МОРЕ НЕБО ЛИСТ РИБА ЗИМА ЛІТО САЛО ЖИТО ШПАК КЛЕН БАНТ КІНО МРІЯ ОКЕАН ЛИМОН ВЕСНА ЗЕБРА ЧОВЕН СОНЦЕ ЗВУКИ ПТАХИ ЛИСТОК ДРУЖБА САДОК ВІТЕР КАЗКА МУЗИКА ГАЙ ПАР ЛАН МАК ГАК ДАХ'.split()}
allwords={};sources=[];preferred={}
external={}
for loc in ['en','ru']:
 external[loc]={w.strip().lower() for w in (raw/('blocked-'+loc+'.txt')).read_text().splitlines() if len(w.strip())>=3 and w.strip().isalpha()}
blocks['ru']+=['шлюх','пидор','педер','пенис','анус','хуес','хуёв','наци','дроч','манд','мудак','залуп','гандон']
blocks['uk']+=['шлюх','шльонд','підор','педера','пеніс','анус','хує','хуї','хуй','наци','дроч','мудак','залуп','гандон']
for loc in ['en','ru','uk']:
 data=(raw/(loc+'.dic')).read_bytes()
 try:txt=data.decode('utf8')
 except UnicodeDecodeError:txt=data.decode('cp1251')
 aff=(raw/(loc+'.aff')).read_bytes()
 try:aff=aff.decode('utf8')
 except UnicodeDecodeError:aff=aff.decode('cp1251')
 forbidden=set()
 for directive in aff.splitlines():
  parts=directive.split()
  if len(parts)>1 and parts[0] in ['NEEDAFFIX','ONLYINCOMPOUND','FORBIDDENWORD','NOSUGGEST']:forbidden.add(parts[1])
 selected=set()
 for line in txt.splitlines()[1:]:
  if not line.strip():continue
  word=line.split('/')[0].split()[0]
  if not re.fullmatch(alphabets[loc],word) or not 3<=len(word)<=8 or len(set(word))!=len(word):continue
  # Bare abbreviation entries are excluded; ordinary bare function words are allowlisted.
  if '/' not in line and word not in allows[loc]:continue
  if any(stem in word for stem in blocks[loc]):continue
  if any(bad in word for bad in external.get(loc,())):continue
  flags=line.split('/',1)[1].split()[0] if '/' in line else ''
  if any(flag in flags for flag in forbidden):continue
  if loc=='en' and '/' in line and '!' in line.split('/',1)[1]:continue
  selected.add(word.upper())
 allwords[loc]=sorted(selected)
 preferred[loc]=sorted({w for w in warm[loc] if w in selected})
 print(loc,len(selected),dict(sorted(Counter(map(len,selected)).items())))
 assert sum(len(w)==3 for w in selected)>=402,(loc,'insufficient three-letter deck')
 sources.append({'locale':loc,'url':'https://raw.githubusercontent.com/LibreOffice/dictionaries/master/'+({'en':'en/en_US.dic','ru':'ru_RU/ru_RU.dic','uk':'uk_UA/uk_UA.dic'}[loc]),'sha256':hashlib.sha256(data).hexdigest(),'affix_sha256':hashlib.sha256((raw/(loc+'.aff')).read_bytes()).hexdigest(),'licence':{'en':'SCOWL permissive notices','ru':'Lebedev permissive redistribution notice','uk':'MPL-1.1'}[loc]})
pack={'revision':'stage-words-v1','sources':sources,'words':allwords,'preferred':preferred,'filter_sources':[{'locale':loc,'url':'https://raw.githubusercontent.com/LDNOOBW/List-of-Dirty-Naughty-Obscene-and-Otherwise-Bad-Words/master/'+loc,'sha256':hashlib.sha256((raw/('blocked-'+loc+'.txt')).read_bytes()).hexdigest(),'licence':'CC-BY-4.0'} for loc in ['en','ru']]}
encoded=json.dumps(pack,ensure_ascii=False,separators=(',',':'))+'\n';file=root/'Data/stage_words_v1.json';
if file.exists() and file.read_text()!=encoded and not args.replace_unreleased_v1:raise SystemExit('v1 is immutable after release; create a new version, or explicitly replace the unreleased pack.')
file.write_text(encoded)
for src,dest in [('en-notices.txt','SCOWL-notices.txt'),('ru-license.txt','Russian-notice.txt'),('uk-license.txt','Ukrainian-notice.txt'),('mpl-1.1.txt','MPL-1.1.txt'),('blocked-license.txt','Shutterstock-filter-CC-BY-4.0.txt')]:shutil.copy2(raw/src,licenses/dest)
(licenses/'Shutterstock-filter-NOTICE.txt').write_text('Filtering input only: List of Dirty, Naughty, Obscene, and Otherwise Bad Words, Copyright 2012–2020 Shutterstock, Inc., CC BY 4.0. https://github.com/LDNOOBW/List-of-Dirty-Naughty-Obscene-and-Otherwise-Bad-Words . English/Russian lists used to exclude entries; not displayed as game words. Additional Ukrainian exclusions are authored here.\n')
(licenses/'MODIFICATIONS.txt').write_text('Flying Ladders stage-words-v1, 2026-10-29. Modified subset of the identified spelling dictionaries: lowercase alphabetic entries, 3–8 distinct letters, excluded abbreviations, explicit function-word exceptions, family vocabulary exclusions, uppercase NFC display. No definitions or affix engines included. Original sources and exact input SHA-256 hashes are in stage_words_v1.json. These word-data notices and source rights are separate from the game code. Ukrainian derived data remain available under MPL-1.1. Source form is provided in StageWordPoolSource.zip distributed with the game. The pool is a spelling-list selection; manual editorial review remains distinct from spelling validity.\n')
# Plain source copy + notices available in the distributed app, without changing the UI.
resources=root/'CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent';shutil.copy2(file,resources/file.name)
stream=root/'CascadeUnity/Assets/StreamingAssets';stream.mkdir(exist_ok=True)
with zipfile.ZipFile(stream/'StageWordPoolSource.zip','w',zipfile.ZIP_DEFLATED) as z:
 z.write(file,file.name)
 z.write(Path(__file__),'build_stage_word_pool.py')
 for f in licenses.iterdir():z.write(f,'Licences/'+f.name)
print('JSON bytes',len(encoded.encode()),'source archive bytes',(stream/'StageWordPoolSource.zip').stat().st_size)
