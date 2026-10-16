# Что осталось настроить для входа

Экран и обработчики используют существующие **Unity Authentication + Unity Player Accounts + Cloud Code/Cloud Save**. Система аккаунтов и проект не менялись. Пароли, Apple keys и Google secrets в Unity-приложение не добавляются.

Сейчас в `Configs/authentication.json` Client ID пустой, `cloudCodeDeployedAndVerified=false` и `purchaseVerificationConfigured=false`. Поэтому настоящий вход/облако пока выключены. Гостевой режим сохраняется на устройстве; нажатие Apple/Google в неактивированной сборке показывает понятное сообщение, а не имитирует вход.

## 1. Включить провайдера

В Unity Dashboard открыть Cloud Project **`a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`**. В Authentication → Identity Providers добавить Unity Player Accounts, создать клиент CASCADE для iOS/Android и PC, если нужен вход из Editor. Его **Client ID** вставить в `unityPlayerAccountsClientId` в `Configs/authentication.json`.

Используются стандартные callback URI:

- Desktop/Editor: `http://localhost/callback`.
- iOS/Android: `unitydl://com.unityplayeraccounts.a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`.

Обе игровые кнопки открывают защищённую браузерную страницу Unity Player Accounts, где выбирается Apple или Google. Именно так устроена установленная интеграция; SDK 3.6.1 не предоставляет параметра прямого выбора провайдера. Подпись в ожидании подсказывает выбранный игроком вариант. Это не отдельные native Apple/Google SDK. [Официальная настройка и поведение Player Accounts](https://docs.unity.com/en-us/authentication/unity-player-accounts).

## 2. Развернуть сохранения

Проверить окружение **`production`**, Cloud Save и модуль **`CascadeAccount`**. Разворачивается существующий `CascadeUnity/Assets/_Cascade/Cloud/CascadeAccount.ccmr`. Сервер работает с защищённым ключом **`cascade_account_v1`**; его создаёт модуль при первом сохранении. Выдать Cloud Code доступ к Protected player data; прямую запись игроком этого ключа не разрешать.

Подготовка из корня проекта:

```sh
python3 Tools/prepare_accounts.py --check
python3 Tools/prepare_accounts.py --prepare
python3 Tools/prepare_accounts.py --dry-run
python3 Tools/prepare_accounts.py --deploy
```

`--check` и `--prepare` локальны. Последние две команды требуют действительного Client ID и вашего доступа UGS; `--deploy` меняет выбранное облачное окружение. Потребуются установленный .NET 9 SDK и UGS CLI; пути можно задать через `CASCADE_DOTNET` / `CASCADE_UGS`. CLI credentials держать локально по `Configs/account-deployment.env.example`, не в `authentication.json` и не в Unity assets.

После успешного развёртывания и проверки ReadAccount/SyncAccount в этом окружении включить **`cloudCodeDeployedAndVerified=true`**. Применить JSON через **CASCADE → Release → Apply Apple and Google sign-in configuration**; перед player-сборкой это также делается автоматически. `purchaseVerificationConfigured` можно оставить `false`, пока отдельно не настроен и проверен сервер чеков: этот флаг не нужен для проверки входа и обычного сохранения.

## 3. Проверить с реальными аккаунтами

После активации: новый гость → Apple, другой гость → Google; отмена браузера и повтор; выход и перезапуск; повторный вход; второе устройство с тем же Player Account; выбор существующего сохранения и отказ от замены локального. Успешная привязка сохраняет исходную игровую identity. Сохранение другого аккаунта загружается только по явному выбору; кошельки не объединяются.

Выход останавливает облачную работу и сохраняет локальный прогресс. Неотправленный запрос и прежняя сессия остаются восстанавливаемыми, но игра не возобновляет их без явного входа. Выход не отвязывает и не удаляет аккаунт. Пока игрок явно вышел, проверка новых покупок от прежней identity блокируется до входа.

Настоящий OAuth и перенос между физическими устройствами нельзя подтвердить пустым Client ID или тестовым SDK. Эти проверки остаются за вами после настройки сервиса. Существующий отдельный релизный пункт полного удаления игровых данных остаётся открытым; подробности — `Docs/ACCOUNT_ACTIVATION_OCT10.md`. Эта задача не заменяет его браузерной кнопкой выхода.
