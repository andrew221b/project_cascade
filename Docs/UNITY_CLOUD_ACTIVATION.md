# Unity Cloud: подключение CASCADE — 7 октября 2026

Облачная архитектура и файлы подготовлены в проекте. Активация сервисов,
публикация модуля и проверка настоящего облачного аккаунта **не выполнены**.
Игра сохраняет данные локально до подтверждённого развёртывания.

## Что проверено

| Параметр | Локальное значение |
| --- | --- |
| Unity Project ID | `a1b8a760-5a0c-4c2d-8ae7-54c1b411259a` |
| Локальное имя связанного проекта | `CascadeUnity` |
| Organization ID | `shapeinc25` |
| Планируемое окружение | `development` — существование в Cloud не подтверждено |
| Модуль | `CascadeAccount` |
| Закрытый ключ профиля | `cascade_account_v1`, Cloud Save **Protected** |
| UGS CLI | установлен, версия `1.9.0`; команды справки работают |

Пользователь назвал облачный проект **Cascade Project**. Его имя и Project ID
в Dashboard не удалось проверить. Нельзя считать локальное имя доказательством,
что выбран именно этот облачный проект. Не менять связку молча: сначала сравнить
ID проекта в Unity Cloud с ID выше. Если это другой проект, связать Unity Editor
с правильным проектом и согласованно обновить `CloudPlayerSettings.projectId`.

Проверки `ugs status`, `config get project-id`, `config get environment-name`
и `env list` завершены по лимиту 10 секунд каждая без результата; их дочерние
процессы остановлены. Поэтому наличие сохранённых полномочий CLI, доступ к
организации и список окружений неизвестны. Попытка открыть Chrome через CUA
зависла и была прервана пользователем; Dashboard не прочитан. Секреты не читались
и не выводились. Платёжные настройки и договоры не изменялись.

Установлены SDK Authentication `3.6.1`, Cloud Code `2.10.3`, Cloud Save `3.4.0`,
Mobile Notifications `2.4.3`; Unity IAP `5.4.4` сохранён. Клиентские переключатели
`enabledForPlayers`, `playerAccountsConfigured`, `purchaseVerificationConfigured`
в `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/CloudPlayerSettings.asset`
оставлены выключенными до соответствующей проверки.

## Что включить в Unity Dashboard

1. Открыть [Unity Cloud](https://cloud.unity.com/), выбрать **Cascade Project**
   и проверить Project ID. Создать или выбрать отдельное окружение `development`.
2. Подключить **Authentication**, **Cloud Save** и **Cloud Code** в этом проекте.
   Проверить права публикации текущего пользователя либо существующего
   Service Account. CLI использует Service Account; его ключи не входят в игру
   и не должны попадать в репозиторий или переписку.
3. В Authentication → Identity Providers добавить **Unity Player Accounts**
   для PC (проверка в Editor), Android и iOS. В Editor открыть Services →
   Unity Player Accounts → Configure, установить выданный Client ID и проверить
   зарегистрированные redirect URI. Стандартные URI: `http://localhost/callback`
   для PC и `unitydl://com.unityplayeraccounts.<unity-project-id>` для телефона.
   Сам анонимный вход не обеспечивает восстановление после переустановки;
   сохранение нужно привязать к аккаунту игрока.
4. Опубликовать модуль `CascadeAccount` в `development` после успешной сборки
   и тестов. Модуль хранит профиль в Protected Player Data и реестр владельцев
   оплаченных транзакций в Private Game Data. Вручную заполнять профили и
   выдавать купленные предметы не требуется.
5. Проверить настоящее создание аккаунта, два одновременных устройства,
   повтор запроса после потери ответа, автономную игру и восстановление входа.
   Отдельно проверить первое создание профиля и владельца оплаченной транзакции:
   модуль использует экспериментальный synthetic guard для отсутствующего ключа.
   Документированного create-if-absent у Cloud Save нет; официальная инструкция
   предлагает опускать `writeLock` при создании. Если guard отклоняется, нужен
   документированный атомарный сервис первичного создания. Нельзя включать клиент
   до проверки обоих сценариев; безусловная запись не является безопасной заменой.
6. Только после успешной облачной проверки включить `enabledForPlayers`.
   `playerAccountsConfigured` включить после реального входа и восстановления
   привязанного аккаунта. Повторить проверку в отдельном `production` перед
   выпуском; проект и окружение никогда не должны подменяться автоматически.

Protected Player Data уже запрещает прямую запись игроком; все записи профиля
идут через Cloud Code. Дополнительный Access Control полезен при наличии других
сервисов, но не заменяет Protected-класс и проверки модуля. Для этой реализации
не нужны Economy, Leaderboards, Analytics, Remote Config или серверные push.
Накопления и оплаченные права имеют одного владельца — агрегат `CascadeAccount`;
создавать второй независимый кошелёк в Economy не следует.

## Публикация через уже установленный CLI

После проверки Project ID, полномочий и сборки .NET 9 выполнить из корня пакета:

```sh
export DOTNET_BUNDLE_EXTRACT_BASE_DIR=/private/tmp/cascade-ugs-runtime
ugs status
ugs env list --project-id <VERIFIED_PROJECT_ID>
ugs deploy CascadeUnity/CascadeAccountCCM/CascadeAccount.sln \
  --project-id <VERIFIED_PROJECT_ID> --environment-name development --dry-run
ugs deploy CascadeUnity/CascadeAccountCCM/CascadeAccount.sln \
  --project-id <VERIFIED_PROJECT_ID> --environment-name development
ugs cloud-code modules list \
  --project-id <VERIFIED_PROJECT_ID> --environment-name development
```

`<VERIFIED_PROJECT_ID>` заменить ID, проверенным в Dashboard. Не применять
`--reconcile`: этот флаг удаляет облачные ресурсы, отсутствующие в публикации.
CLI может собрать `.sln` с сохранённым publish profile и опубликовать модуль.
`CascadeUnity/Assets/_Cascade/Cloud/CascadeAccount.ccmr` — ссылка на ту же solution
для Deployment Window; для публикации через Editor потребуются пакеты
Services Deployment/Tooling. Через CLI они не требуются.

## Настройка настоящих покупок

Подключить Apple/Google каталоги с существующими ID и приложение
`com.vickand.cascade`. Проверить sandbox-покупки и восстановление No ads.
Само включение Unity Cloud не заменяет App Store Connect и Play Console.

Cloud Code требует HTTPS-проверяющий сервис с привязкой доказательства покупки
к Player ID, Project ID, Environment ID, Product ID и Transaction ID. Успешный
ответ должен содержать `Valid`, `PlayerBindingVerified`, `CatalogVerified` и
совпадающие идентификаторы, платформу и Bundle ID. `Backend/ReceiptVerifier` уже обновлён для авторизованных вызовов из Cloud Code.
Настроить на сервере `SERVICE_BEARER_TOKEN`, `UGS_PROJECT_ID` и `UGS_ENVIRONMENT_ID`,
а также Apple/Google ключи и сертификаты по его README. Первый подтверждённый
владелец чека закрепляется в Cloud Save; магазинная привязка appAccountToken/
obfuscatedAccountId и обработчик возвратов требуют отдельной настройки.
До проверки реальных транзакций оплаченные облачные выдачи закрыты.

В Unity Secret Manager задать `CASCADE_RECEIPT_VERIFIER_URL` и
`CASCADE_RECEIPT_VERIFIER_TOKEN`, разрешить доступ **Cloud Code**. URL и токен
не вводить в клиентские ассеты. Ключи Apple/Google хранить на проверяющем сервисе.
После проверки реальной покупки, повторной доставки, потери ответа, восстановления
и попытки присвоить одну транзакцию двум аккаунтам включить
`purchaseVerificationConfigured`. Возвраты и отзыв оплаченных прав требуют
отдельного обработчика магазинных уведомлений перед production.

Пока не подключена AdMob SSV, настоящие rewarded-предложения при включённом
Cloud Save скрыты: клиентская награда иначе была бы отклонена облаком.
Тестовые видео продолжают работать с временным тестовым кошельком.
Для включения настоящих наград нужен серверный обработчик подписанных AdMob
событий с идемпотентной выдачей в этот же аккаунт; обычный SDK callback этого
не заменяет. Автоматические объявления не меняют кошелёк и сохраняют свою политику.

Локальные напоминания настроены отдельно: разрешение ОС и переключатель
в настройках относятся к текущему устройству. Для них не нужны Unity Push,
APNs/FCM или отдельный сервер.

## Первичные источники

- [Cloud Code: публикация через CLI](https://docs.unity.com/en-us/cloud-code/modules/how-to-guides/write-modules/cli)
- [Cloud Save: Protected Player Data](https://docs.unity.com/en-us/cloud-save/concepts/player-data)
- [Cloud Save: конкурентные записи](https://docs.unity.com/en-us/cloud-save/concepts/write-locks)
- [Unity Player Accounts: provider, Client ID и URI](https://docs.unity.com/en-us/authentication/unity-player-accounts)
- [Secret Manager для Cloud Code](https://docs.unity.com/en-us/services/secret-manager/tutorials/integrations/cloud-code/modules)
- [Access Control: приоритет конкретных правил](https://docs.unity.com/en-us/services/access-control)
