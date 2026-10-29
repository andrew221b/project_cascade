# Flying Ladders: действующие ссылки и статус миграции

Обновлено 27 октября 2026. Основной сайт: https://flyingladders.vickand.com/.

| Для чего | Адрес |
| --- | --- |
| Политика конфиденциальности | https://flyingladders.vickand.com/privacy/ |
| Условия использования | https://flyingladders.vickand.com/terms/ |
| Поддержка | https://flyingladders.vickand.com/support/ |
| Запросы данных | https://flyingladders.vickand.com/data-request/ |
| Доступность | https://flyingladders.vickand.com/support/#accessibility |
| Рекламный издатель | https://flyingladders.vickand.com/app-ads.txt |
| Почта поддержки и privacy | support@vickand.com |
| AdMob SSV: рабочий callback | https://vickand.com/api/flying-ladders/admob/ssv |
| AdMob ingress: проверка готовности | https://vickand.com/api/flying-ladders/health |
| Удаление аккаунта: рабочая форма | https://cascade-sky-gardens.active-seed-4233.chatgpt.site/delete-account |

## Подтверждено

- Сайт опубликован на Vercel; Cloudflare DNS и HTTPS работают. Контакты, canonical, sitemap и исходные игровые конфигурации обновлены. Флаги юридического утверждения/индексации сохранены в прежнем состоянии: документы пока черновики.
- App Store Connect: privacy/choices, support/marketing для EN/RU/UK и App Review email обновлены. Дополнительный Accessibility URL сохранён и подтверждён чтением API.
- TestFlight: Marketing URL, Privacy Policy URL и Feedback Email обновлены для EN/RU/UK; Beta Review email — support@vickand.com. Четыре записи подтверждены API. Сборки и статусы релизов не менялись.
- Google Play: сайт разработчика и email сохранены, API подтвердил значения; существующие треки неизменны. Privacy URL сохранён в черновике. Действующая ссылка удаления уже ведёт на AccountCloud и оставлена.
- AccountCloud опубликован как Sites version 11 из точной действующей версии. Только пять страниц с маркетинговыми ссылками и email изменены; сборка успешна. /privacy возвращает 308 на новый поддомен. Форма удаления показывает support@vickand.com. API, база, секреты, покупки и webhook URL сохранены.
- AdMob: для Android/iOS изменены privacy URL и подтверждён выбор обеих версий приложения. Новое consent-сообщение не сохранялось и не публиковалось. Сохранение обоих URL подтверждено после новой загрузки страницы.

## Внешние сервисы и оставшиеся действия

- Unity Player Accounts: Privacy/Terms уже сохранены на новом поддомене; имя, статус, client/project IDs и OAuth-настройки сохранены.
- Netlify: 301 уже опубликован (deploy 6ada32ff5cd7be500660d19a), сохранение пути и query подтверждено. Действий для редиректа не осталось.
- AdMob app-ads.txt: в кабинете пока видны только две версии ShapeMania. Для Flying Ladders статус обхода/подтверждения не появился; файл на новом домене готов, но это не подтверждение обхода Google. После публикации store listing и привязки приложения проверить повторно.
- Юридическая проверка документов, обычная подготовка релиза/деклараций, новый игровой билд для встроенных URL и проверка на телефоне. Задача миграции не является юридическим одобрением и не запускает выпуск игры.

## Игровой сервер перенесён в Unity

Прогресс, балансы, владение покупками, проверка чеков, рекламные intents/награды, подписки и удаление аккаунта теперь обрабатываются CascadeAccount в Unity Cloud Code; данные хранятся в Private Cloud Save. Старые игровые таблицы очищены без резервной копии; внешний шлюз больше не подключён к базе.

На https://cascade-sky-gardens.active-seed-4233.chatgpt.site остались публичные HTTPS-входы уведомлений Apple/Google/AdMob и тонкие relay для формы удаления/покупок локального режима. Они передают операции в Unity и не хранят игровые данные. Статический flyingladders.vickand.com не становится API-сервером от одной смены адреса. AdMob получил отдельный рабочий вход на vickand.com/api/flying-ladders/admob/ssv через Cloudflare; остальные callback URL сохранены. Внешний вид сайта не менялся.

Точная проверка переноса: Reports/UnityBackendMigrationOct27/verification.json. Рабочий health шлюза возвращает backend=UnityCloudCode, storage=UnityCloudSave, ready=true. Новый AdMob callback проверен Google и сохранён для iOS и Android; оба значения прочитаны после обновления страниц. Guard серверных наград включён в исходной конфигурации для следующей сборки. Реальная реклама и начисление на устройстве пока не проверены. См. Docs/ADMOB_CALLBACK_DOMAIN_OCT27_RU.md и Reports/AdmobCallbackVickandOct27.

Полный статус всех трёх продуктов: ../Reports/WebsiteMigrationVickandOct22/MIGRATION_VICKAND_2026-10-22.md. Исторические отчёты Oct21 сохранены как свидетельства предыдущего состояния.
