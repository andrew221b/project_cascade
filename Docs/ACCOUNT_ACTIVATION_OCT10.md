# Account activation — 10 October 2026

## Design note

Keep the installed Unity Player Accounts flow: its hosted screen offers Google and Apple, then links the existing anonymous UGS identity. Do not add native OAuth SDKs, change product IDs, or force-link identities. Loading an existing account stays an explicit replacement choice; purchases remain server verified and idempotent.

`Configs/authentication.json` remains the source of public client configuration. Validate it and apply it before player builds, including disabling stale cloud settings when activation is off. Browser sign-in gets cancellation, a bounded wait, and guards against late callbacks. No secrets, access tokens, or receipts are added to this file or the save journal. Profile and journal schemas stay unchanged.

Deployment is an explicit operator command, never a side effect of opening Unity or compiling. The helper prepares and tests the current Cloud Code module; it must not mark a deployment verified automatically. Real provider login and cross-device purchases still require a configured service and physical-device verification.

The SDK requests `openid` and `offline_access`; CASCADE does not need the player's email address. Project-bound default redirect URIs replace any stale custom override when configuration is applied. No additional package is required.

## Что заполнить

Единственный новый публичный ID для Google/Apple входа — **Unity Player Accounts Client ID**. Google и Apple предлагает размещённый экран Unity Player Accounts. Не создавайте второй параллельный механизм входа через native Google/Apple Authentication providers и не кладите OAuth secrets в мобильное приложение.

Файл: [Configs/authentication.json](../Configs/authentication.json).

| Поле | Значение / действие |
| --- | --- |
| `unityProjectId` | Уже заполнен: `a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`. Это Cloud Project ID, а не bundle ID. |
| `unityPlayerAccountsClientId` | Вставить Client ID созданного приложения Unity Player Accounts. Сейчас пустой. |
| `unityEnvironmentName` | `production`; это окружение нужно создать/проверить в Unity Dashboard. Для проверки можно сначала использовать отдельное `development`. Данные окружений не объединяются автоматически. |
| `cloudCodeModuleName` | `CascadeAccount`; оставить как есть. |
| `cloudCodeDeployedAndVerified` | Пока `false`. Поставить `true` после успешного развёртывания и проверки API в выбранном окружении, затем выполнить проверку входа на тестовой сборке. Для публикации нужны все проверки ниже. |
| `purchaseVerificationConfigured` | Пока `false`. Поставить `true` после развёртывания и проверки сервера чеков; вход и сохранение можно проверить раньше. |

Изменения автоматически применяются перед сборкой. Чтобы применить сразу: **CASCADE → Release → Apply Apple and Google sign-in configuration**. Команда Check проверяет файл без изменения assets. Пустой Client ID с выключенными флагами — допустимое состояние; оно отключает облако и вход. Ошибки/опечатки конфигурации останавливают сборку, а не включают частично настроенный вход.

## Один раз в Unity Dashboard

1. Открыть именно указанный Cloud Project. В **Authentication → Identity Providers** добавить/включить **Unity Player Accounts**.
2. Создать клиент Player Accounts для CASCADE. Разрешить iOS, Android и Desktop/PC, если нужен вход из Unity Editor. Скопировать его Client ID в JSON.
3. Использовать стандартные callback URI SDK:
   - Editor/Desktop: `http://localhost/callback`;
   - iOS/Android: `unitydl://com.unityplayeraccounts.a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`.
   Схему приложения добавляет установленный SDK при сборке; вручную редактировать экспорт Xcode не нужно.
4. В нужном окружении подключить Cloud Save и развернуть текущий Cloud Code module. Защищённый ключ `cascade_account_v1` создаётся сервером при первом успешном сохранении; вручную заполнять данные игроков не нужно. Проверить права Cloud Code на Protected player data; клиент не должен напрямую писать этот ключ.
5. Для CLI создать Service Account с правом развёртывания Cloud Code в этом проекте. Его **Key ID** и **Secret Key** — только на компьютере развёртывания: `UGS_CLI_SERVICE_KEY_ID` и `UGS_CLI_SERVICE_SECRET_KEY`. Шаблон: [account-deployment.env.example](../Configs/account-deployment.env.example). Можно вместо env использовать `ugs login`; секреты в `authentication.json` не помещать. Локальный env необходимо загрузить в shell самостоятельно, helper его не читает и не выводит.

Официальная инструкция: [Unity Player Accounts](https://docs.unity.com/en-us/authentication/unity-player-accounts), [UGS CLI deployment](https://docs.unity.com/en-us/services-cli/2.0.0/manual/general/base-commands/deploy).

## Подготовка и развёртывание

```sh
python3 Tools/prepare_accounts.py --check
python3 Tools/prepare_accounts.py --prepare
python3 Tools/prepare_accounts.py --dry-run
python3 Tools/prepare_accounts.py --deploy
```

- `--check`: локальная проверка JSON, без сети и облачных изменений.
- `--prepare`: проверки настоящего клиента на управляемых SDK-заглушках, проверки настоящих серверных endpoints, локальная публикация .NET module в `Reports/AccountActivation-Oct10/module`.
- `--dry-run`: предварительный просмотр UGS deployment; нужны действующие CLI credentials и Client ID, удалённых изменений нет.
- `--deploy`: повторная подготовка и развёртывание только `CascadeAccount.ccmr`, без `--reconcile` и удаления других облачных ресурсов. Флаг готовности автоматически не меняется.

Нужны Python 3, .NET 9 SDK и UGS CLI с поддержкой C# modules. Если исполняемые файлы не находятся на PATH, задать `CASCADE_DOTNET` и `CASCADE_UGS`. Helper не устанавливает/обновляет пакеты и не собирает iPhone player.

## Покупки

Непотребляемый товар: `cascade.no_automatic_ads`. Потребляемые: `cascade.coins_240`, `cascade.coins_480`. IDs одинаковы в уже подготовленном каталоге клиента и сервера; зарегистрировать их в App Store Connect / Google Play Console, цены задавать в сторах.

Для [Backend/ReceiptVerifier](../Backend/ReceiptVerifier) отдельно задать серверные Apple/Google credentials по его `.env.example`. В Unity Secret Manager настроить `CASCADE_RECEIPT_VERIFIER_URL` и `CASCADE_RECEIPT_VERIFIER_TOKEN`, разрешить их получение модулю Cloud Code. Это не Client ID входа. Приватный Apple `.p8`, Google service-account JSON и токены не входят в Unity assets или публичный JSON.

Гостевой вход сначала создаёт отдельную UGS identity. Успешная привязка сохраняет эту же identity, прогресс и серверные entitlement records. Если аккаунт уже занят, игрок явно выбирает загрузку его сохранения или сохранение текущего. Исходное локальное сохранение архивируется перед заменой, а гостевой SDK profile сохраняется для восстановления после неудачной загрузки. Два чужих кошелька не склеиваются. Нативное восстановление покупок не выдаёт повторно уже потраченные consumables; оставшийся баланс переносит облачное сохранение.

Для переноса между платформами использовать один и тот же Player Account. Разные Google/Apple identities не следует считать одним игровым аккаунтом только из-за похожего email.

## Проверки перед публикацией

1. Новый гость → Google; другой новый гость → Apple. Отмена браузера, закрытие окна, истечение ожидания, повтор входа. Прогресс гостя сохраняется, кнопки снова доступны.
2. Перезапуск приложения после входа: SDK восстанавливает сессию, сохранение продолжает работать.
3. Второе устройство → существующий аккаунт → явное Load account. Все три режима, инвентарь и непотребляемое entitlement совпадают; отмена выбора не меняет гостевой save.
4. Офлайн игра → возвращение сети; два устройства с конкурентными сохранениями. Никаких повторных наград/дублированных покупок.
5. Store sandbox/TestFlight/Google internal testing: оплата, отказ, pending, прерывание приложения, восстановление. Секреты сервера настроены, чек привязан к верной identity, повтор доставки не добавляет монеты снова.
6. Заполнить данные издателя и опубликовать Privacy/Terms. **Полное удаление game account/data остаётся отдельным релизным блокером:** текущий Unity account portal управляет общей Unity identity и сам по себе не удаляет все данные CASCADE. Нужны проверенный маршрут удаления из игры и внешняя страница запроса для Google Play. Не считать браузерную ссылку готовым удалением игровых данных. См. [ACCOUNT_STORE_ASSETS_OCT09.md](ACCOUNT_STORE_ASSETS_OCT09.md).

**Что проверено сейчас:** локальные конфиги и настоящая логика клиента на SDK transport doubles; серверные проверки и локальная публикация описаны в отчёте текущего запуска. **Что пока невозможно подтвердить:** реальный OAuth, развёртывание в вашем окружении, перенос между физическими устройствами и реальные покупки — Client ID и серверные credentials ещё не заполнены.

Текущий результат: **43** проверки клиента/конфига, **80** серверных boundary checks; Release module опубликован локально. Настоящий Unity Editor завершил проверку с кодом 0: runtime/editor C# скомпилирован, конфиг применён, контракт установленного SDK проверен, EN/RU/UK таблицы обновлены. UI/OAuth на телефоне в этой задаче не проверялись. NuGet сообщил `NU1900`: онлайн-проверка уязвимостей зависимостей недоступна в среде выполнения; это не подтверждение отсутствия уязвимостей. Пакеты Unity не менялись. Артефакты: `Reports/AccountActivation-Oct10`.
