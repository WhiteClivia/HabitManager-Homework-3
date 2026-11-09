Darba lielākā daļa tika izpildīta ar ChatGPT palīdzību (aizmirsu pieminēt, ka iepriekšējie divi mājasdarbi - arī).

# Ieradumu pārvaldnieks · 3. mājas darbs

Приложение .NET MAUI работает с REST API на ASP.NET Core. API при запуске загружает демонстрационные привычки и хранит изменения в памяти до перезапуска. Запросы работают от имени `demo-user-1`. У каждой привычки есть `UserId`, а поиск ограничен этим владельцем. Локальный JSON из задания № 2 не импортируется.

## Запуск на Windows

1. Установите .NET 10 SDK и рабочую нагрузку **.NET Multi-platform App UI development** в Visual Studio. Для запуска на Android также понадобится эмулятор с настроенным Android SDK.
2. Распакуйте ZIP и откройте `LU_HabitManager_mv25070.slnx`.
3. В терминале из папки решения запустите API и оставьте терминал открытым.

   ```powershell
   dotnet run --project HabitManager.Api
   ```

4. В Visual Studio выберите `HabitManager.Mobile` как запускаемый проект, выберите **Windows Machine** или Android Emulator и запустите отладку. Windows-клиент подключается к `http://localhost:5187/`. Стандартный Android Emulator использует `http://10.0.2.2:5187/`. Адрес задаётся в `HabitManager.Mobile/MauiProgram.cs`. [Документация Microsoft о локальном API](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-10.0)

Сборка решения из терминала на Windows после установки рабочей нагрузки:

```powershell
dotnet build LU_HabitManager_mv25070.slnx
```

## Демонстрация

1. Откройте **All active** и создайте привычку через **Add habit**. Для **Once** выберите дату. Для **Weekly** отметьте нужные дни в форме.
2. Откройте привычку, выберите запланированную дату и нажмите **Mark completed**. Проверьте историю и при необходимости отмените отметку через **Undo completion**.
3. Измените привычку через **Edit**. Сохранённые отметки остаются в истории при смене расписания.
4. Архивируйте привычку и найдите её в разделе **Archived**. Из деталей её можно вернуть в активный список.
5. В **Today** отображаются только активные привычки, запланированные на текущий день устройства. Счётчик использует тот же набор привычек.
6. Остановите API и нажмите **Refresh**. Приложение покажет ошибку подключения. После повторного запуска API нажмите **Refresh** ещё раз.

## REST API

| Метод | Маршрут | Назначение |
| --- | --- | --- |
| GET | `/api/habits` | Список активных и архивных привычек |
| GET | `/api/habits/{id}` | Детали привычки |
| POST | `/api/habits` | Создание, ответ `201 Created` с заголовком `Location` |
| PUT | `/api/habits/{id}` | Редактирование |
| PUT | `/api/habits/{id}/archive` | Архивирование |
| DELETE | `/api/habits/{id}/archive` | Возврат из архива |
| GET | `/api/habits/{id}/completions` | История выполнения |
| PUT | `/api/habits/{id}/completions/{date}` | Отметка выполнения за дату `yyyy-MM-dd` |
| DELETE | `/api/habits/{id}/completions/{date}` | Отмена отметки |

Ошибки возвращаются в формате Problem Details. `400` означает ошибку входных данных, `404` — отсутствующую привычку, `409` — нарушение доменного правила, например попытку отметить архивную привычку или незапланированную дату.

Пример для PowerShell:

```powershell
$base = 'http://localhost:5187/api/habits'
$date = Get-Date -Format 'yyyy-MM-dd'
$body = @{
    name = 'Read a chapter'
    description = 'One chapter today'
    schedule = @{ repeatType = 'Once'; onceDate = $date }
} | ConvertTo-Json -Depth 4

$habit = Invoke-RestMethod -Method Post -Uri $base -ContentType 'application/json' -Body $body
Invoke-RestMethod -Method Put -Uri "$base/$($habit.id)/completions/$date"
Invoke-RestMethod -Uri "$base/$($habit.id)/completions"
```

Расписание `Daily` состоит из `repeatType`. Для `Weekly` укажите `daysOfWeek`, например `["Monday", "Friday"]`. Для `Once` укажите `onceDate`. Ответ содержит структурированное расписание и записи выполнения.

## Проекты

- **Domain** содержит сущности, правила расписания, валидацию и восстановление привычки с сохранением идентификаторов и истории.
- **Contracts** содержит общие запросы и DTO.
- **Application** содержит сценарии работы с привычками и интерфейс `IHabitRepository`.
- **Infrastructure** содержит реализацию репозитория в памяти. На следующем этапе её можно заменить в регистрации зависимостей API.
- **Api** содержит HTTP-маршруты и преобразует ожидаемые ошибки в HTTP-ответы.
- **Mobile** содержит MAUI-экраны и HTTP-реализацию `IHabitClient`.
- **Console** содержит демонстрацию доменной модели из предыдущего этапа.
- **Web** — заготовка для веб-части.

Для этого этапа API использует локальный HTTP и демонстрационного пользователя. После перезапуска данные возвращаются к исходному набору.
