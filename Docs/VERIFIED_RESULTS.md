# Приложение A. Фактическая проверка текущей версии

**39 unit-тестов reference-модели прошли.** Файл отчёта находится в `Reports/unit_tests.txt`. Это Python, не Unity EditMode/PlayMode.

Перебраны полные достижимые графы 40 первичных конфигураций: **4228 состояний**, **16136 принятых переходов**. Все решаемы. Ещё проверено **117 конкретных языковых вариантов** — по 39 для EN/RU/UK. Первые два числа относятся только к 40 primary-уровням и не включают повторный обход локализованных копий.

Черновой словарь после сохранения прежних RU/UK записей и добавления EN: en: 40, ru: 35, uk: 36. Native-редактура и реальные шрифты не проверены.

| ID | Мотив | Минимум команд | Один кратчайший путь (индексы с нуля; D=Drop) | Состояния |
|---|---|---:|---|---:|
| C01 | Первый обмен | 1 | `0` | 4 |
| C02 | Два обмена | 2 | `1 0` | 8 |
| C03 | Сохраняем весь ряд | 2 | `0 2` | 14 |
| C04 | Собрать ещё не доставить | 2 | `0 D` | 13 |
| C05 | Повторяющиеся символы | 3 | `0 2 D` | 16 |
| C06 | Разные связи | 3 | `0 D 2` | 8 |
| C07 | Спуститься раньше | 2 | `D 0` | 27 |
| C08 | Средняя пара внизу | 2 | `D 1` | 8 |
| C09 | Посадочный сдвиг | 2 | `D D` | 34 |
| C10 | Подготовить левый сдвиг | 2 | `2 D` | 17 |
| C11 | Правый сдвиг | 3 | `2 D D` | 57 |
| C12 | Пять элементов | 3 | `3 D D` | 141 |
| C13 | Последний такт бомбы | 2 | `0 D` | 9 |
| C14 | Взрыв помогает доставить | 1 | `0` | 5 |
| C15 | Бомбу можно оставить | 2 | `D 0` | 41 |
| C16 | Обычная передышка | 3 | `D 0 2` | 68 |
| C17 | Ключ открывает маршрут | 2 | `1 0` | 9 |
| C18 | Ремонт после удара | 3 | `0 1 D` | 20 |
| C19 | Ключ и обратный ход | 4 | `0 0 1 D` | 44 |
| C20 | Бомба и ремонт вместе | 1 | `0` | 5 |
| C21 | Хрупкое ребро | 1 | `0` | 11 |
| C22 | Цена выше остатка | 1 | `0` | 5 |
| C23 | Беречь верхнюю платформу | 2 | `0 2` | 25 |
| C24 | Посадка после дорогого хода | 2 | `2 D` | 31 |
| C25 | Две пары за раз | 1 | `0` | 4 |
| C26 | Одинаковая первая пара | 1 | `0` | 4 |
| C27 | Сначала разомкнуть путь | 4 | `1 1 0 D` | 14 |
| C28 | Общая цена двух пар | 1 | `0` | 7 |
| C29 | Фигуры обрамляют слово | 2 | `2 D` | 20 |
| C30 | Две фигуры, слово, две фигуры | 9 | `0 2 1 3 2 1 5 4 D` | 2898 |
| C31 | Смешанный ряд и ключ | 2 | `2 1` | 13 |
| C32 | Восемь мест, два слова | 2 | `0 6` | 44 |
| C33 | Одноразовый мост | 1 | `0` | 5 |
| C34 | Не потратить мост зря | 2 | `1 0` | 13 |
| C35 | Необязательная печать | 2 | `1 D` | 77 |
| C36 | Печать на посадке | 2 | `D D` | 36 |
| C37 | Три согласованных этажа | 6 | `0 3 1 0 3 2` | 365 |
| C38 | Ключ ремонтирует маршрут | 2 | `1 0` | 7 |
| C39 | Связанные пары и взрыв | 1 | `0` | 4 |
| C40 | Передай целый смешанный ряд | 3 | `1 D D` | 97 |

Дополнительный contract-test C35 подтвердил 2 команды для обычной цели и 4 для цели с печатью. Недостижимая optional-цель проверяется и не публикуется.

### Найденные и исправленные недочёты

Учебный C27 сначала открывал ключ прямо на входе; C31 позволял пройти без нужного ребра. Оба заменены на задачи, где открытие участвует в маршруте. Состояния бомб теперь различаются, linked+fragile имеет единую цену. Смешанные 7/8 элементов и английский не остались только словами в описании: у них есть данные и выполненный поиск.

### Повторить

```sh
python Tools/test_cascade.py
python Tools/build_content.py
```

`build_content.py` создаёт исходные Data/levels, localized_levels и word packs из авторского списка внутри скрипта и Data/word_seeds.json. Он перезаписывает эти конкретные generated-файлы. После перехода на Unity editor источником становятся его данные, а генератор фиксируется как regression fixture; не запускать его поверх нового контента без сравнения.

# Приложение B. Все 40 точных определений

Это reference-формат. `s:*` — фигуры из палитры; `g:*` — буквы в Mixed-шаблоне. Цена, счётчики и индексы соответствуют C03–C08. В импортёре добавляются стабильные имена ресурсов и TargetSegments; правила не меняются.

```json
[
  {"id":"C01","lesson":"swap","start":["s:B","s:A","s:C"],"target":["s:A","s:B","s:C"],"floors":[{"hp":1,"edges":[true,true]}]},
  {"id":"C02","lesson":"swap","start":["s:B","s:C","s:A"],"target":["s:A","s:B","s:C"],"floors":[{"hp":2,"edges":[true,true]}]},
  {"id":"C03","lesson":"collapse","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[true,true,true]},{"hp":1,"edges":[true,true,true]}]},
  {"id":"C04","lesson":"drop","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true]}]},
  {"id":"C05","lesson":"duplicates","start":["s:B","s:A","s:B","s:A"],"target":["s:A","s:B","s:A","s:B"],"floors":[{"hp":1,"edges":[true,true,true]},{"hp":2,"edges":[true,true,true]}]},
  {"id":"C06","lesson":"locked","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,false,false]},{"hp":1,"edges":[false,false,true]}]},
  {"id":"C07","lesson":"drop","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":4,"edges":[false,true,true]},{"hp":1,"edges":[true,false,false]}]},
  {"id":"C08","lesson":"locked","start":["s:A","s:C","s:B","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[false,false,true]},{"hp":1,"edges":[false,true,false]}]},
  {"id":"C09","lesson":"conveyor","start":["s:D","s:A","s:B","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true]},{"hp":1,"edges":[true,true,true],"shift":-1}]},
  {"id":"C10","lesson":"conveyor","start":["s:D","s:A","s:C","s:B"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[true,true,true]},{"hp":1,"edges":[true,true,true],"shift":-1}]},
  {"id":"C11","lesson":"conveyor","start":["s:B","s:C","s:A","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true]},{"hp":2,"edges":[true,true,true],"shift":1}]},
  {"id":"C12","lesson":"conveyor","start":["s:E","s:A","s:B","s:D","s:C"],"target":["s:A","s:B","s:C","s:D","s:E"],"floors":[{"hp":2,"edges":[true,true,true,true]},{"hp":2,"edges":[true,true,true,true],"shift":-1}]},
  {"id":"C13","lesson":"bomb","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true],"bomb":{"slot":0,"symbol":"s:A","ticks":1,"damage":2}}]},
  {"id":"C14","lesson":"bomb","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":3,"edges":[true,true,true],"bomb":{"slot":3,"symbol":"s:A","ticks":1,"damage":2}}]},
  {"id":"C15","lesson":"bomb","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":4,"edges":[false,true,true],"bomb":{"slot":3,"symbol":"s:A","ticks":2,"damage":2}},{"hp":1,"edges":[true,true,true]}]},
  {"id":"C16","lesson":"breather","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":3,"edges":[true,true,true]},{"hp":2,"edges":[true,true,true]}]},
  {"id":"C17","lesson":"key","start":["s:B","s:C","s:A","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[false,true,true],"key":{"slot":1,"symbol":"s:A","value":0}}]},
  {"id":"C18","lesson":"repair","start":["s:C","s:A","s:B","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[true,true,true],"repair":{"slot":0,"symbol":"s:A","value":1}},{"hp":1,"edges":[true,true,true]}]},
  {"id":"C19","lesson":"key","start":["s:A","s:C","s:B","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":5,"edges":[true,false,true],"key":{"slot":0,"symbol":"s:C","value":1}}]},
  {"id":"C20","lesson":"bomb_repair","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true],"repair":{"slot":0,"symbol":"s:A","value":1},"bomb":{"slot":3,"symbol":"s:A","ticks":1,"damage":2}}]},
  {"id":"C21","lesson":"fragile","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true],"fragile":[0]}]},
  {"id":"C22","lesson":"fragile_repair","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[true,true,true],"fragile":[0],"repair":{"slot":0,"symbol":"s:A","value":1}}]},
  {"id":"C23","lesson":"fragile","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true],"fragile":[0]},{"hp":1,"edges":[true,true,true]}]},
  {"id":"C24","lesson":"fragile_conveyor","start":["s:D","s:A","s:C","s:B"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true],"fragile":[2]},{"hp":1,"edges":[true,true,true],"shift":-1}]},
  {"id":"C25","lesson":"linked","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[true,true,true],"linked":[0,2]}]},
  {"id":"C26","lesson":"linked_duplicates","start":["s:A","s:A","s:B","s:A"],"target":["s:A","s:A","s:A","s:B"],"floors":[{"hp":1,"edges":[true,true,true],"linked":[0,2]}]},
  {"id":"C27","lesson":"key_linked","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":4,"edges":[false,true,true],"linked":[0,2],"key":{"slot":1,"symbol":"s:D","value":0}}]},
  {"id":"C28","lesson":"fragile_linked","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":3,"edges":[true,true,true],"fragile":[0,2],"linked":[0,2]}]},
  {"id":"C29","lesson":"mixed","start":["s:T","g:C","g:T","g:A","s:O"],"target":["s:T","g:C","g:A","g:T","s:O"],"floors":[{"hp":1,"edges":[true,true,true,true]},{"hp":1,"edges":[true,true,true,true]}]},
  {"id":"C30","lesson":"mixed7","start":["g:A","s:T","s:O","g:C","s:T","s:O","g:T"],"target":["s:T","s:T","g:C","g:A","g:T","s:O","s:O"],"floors":[{"hp":3,"edges":[true,true,true,true,true,true]},{"hp":3,"edges":[true,true,true,true,true,true]},{"hp":3,"edges":[true,true,true,true,true,true]}]},
  {"id":"C31","lesson":"mixed_key","start":["s:T","g:A","g:T","g:C","s:O"],"target":["s:T","g:C","g:A","g:T","s:O"],"floors":[{"hp":2,"edges":[true,false,true,true],"key":{"slot":2,"symbol":"g:C","value":1}}]},
  {"id":"C32","lesson":"mixed8","start":["g:C","s:T","g:A","g:T","s:O","g:D","g:G","g:O"],"target":["s:T","g:C","g:A","g:T","s:O","g:D","g:O","g:G"],"floors":[{"hp":1,"edges":[true,true,true,true,true,true,true]},{"hp":1,"edges":[true,true,true,true,true,true,true]}]},
  {"id":"C33","lesson":"bridge","start":["s:B","s:A","s:C","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[true,true,true]}]},
  {"id":"C34","lesson":"bridge","start":["s:B","s:C","s:A","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true]}]},
  {"id":"C35","lesson":"stamp","start":["s:A","s:C","s:B","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":5,"edges":[true,true,true],"stamp":{"slot":0,"symbol":"s:C","value":1}}]},
  {"id":"C36","lesson":"stamp_conveyor","start":["s:D","s:A","s:B","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":2,"edges":[true,true,true]},{"hp":1,"edges":[true,true,true],"shift":-1,"stamp":{"slot":0,"symbol":"s:A","value":1}}]},
  {"id":"C37","lesson":"combo","start":["s:B","s:C","s:D","s:E","s:A"],"target":["s:A","s:B","s:C","s:D","s:E"],"floors":[{"hp":2,"edges":[true,true,true,true]},{"hp":3,"edges":[true,true,true,true],"shift":-1,"bomb":{"slot":0,"symbol":"s:A","ticks":2,"damage":2}},{"hp":1,"edges":[true,true,true,true]}]},
  {"id":"C38","lesson":"key_repair","start":["s:B","s:C","s:A","s:D"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":1,"edges":[false,true,true],"key":{"slot":1,"symbol":"s:A","value":0},"repair":{"slot":1,"symbol":"s:A","value":1}}]},
  {"id":"C39","lesson":"linked_bomb","start":["s:B","s:A","s:D","s:C"],"target":["s:A","s:B","s:C","s:D"],"floors":[{"hp":3,"edges":[true,true,true],"linked":[0,2],"bomb":{"slot":3,"symbol":"s:A","ticks":1,"damage":2}}]},
  {"id":"C40","lesson":"mixed_bridge","start":["s:T","g:A","g:C","g:T","s:O"],"target":["s:T","g:C","g:A","g:T","s:O"],"floors":[{"hp":2,"edges":[true,true,true,true],"bridge":1},{"hp":2,"edges":[true,true,true,true]}]}
]
```
