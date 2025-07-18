# TicTacToe API

REST API для игры в крестики-нолики с поддержкой:

* Создания новой игры
* Выполнения ходов двух игроков
* Определения победы или ничьей
* Каждый третий ход с вероятностью 10% ставится знак противника
* Настраиваемого размера поля и длины линии для победы через переменные окружения

---

## Технологии

* .NET 9
* PostgreSQL
* Docker / Docker Compose
* Entity Framework Core
* Fluent Validation
* xUnit
* Swagger

---

## Запуск

1. Клонировать репозиторий и перейти в папку проекта

2. Создать и настроить `.env` файл с параметрами окружения:

```
BOARD_SIZE=3
WIN_LENGTH=3
ENEMY_MOVE_CHANCE=0.1
ConnectionStrings__Default=Host=db;Port=5432;Database=tictactoe;Username=postgres;Password=postgres
```

3. Запустить:

```bash
docker-compose up --build
```

`http://localhost:8080`

## Архитектурные решения

* **Многослойная архитектура** — разделение проекта на слои `Domain`, `Application`, `Infrastructure`, `API`, `Shared`
* **Persistence через PostgreSQL** — все игры и ходы сохраняются в базе, что обеспечивает устойчивость к сбоям и возможность восстановления состояния после рестартов
* **Идемпотентность** — при повторном поступлении одного и того же хода API возвращает 200 OK с тем же ETag, предотвращая дублирование ходов
* **Параметры игры настраиваются через переменные окружения** (`BOARD_SIZE`, `WIN_LENGTH`, `ENEMY_MOVE_CHANCE`)
* **Миграции EF Core** обеспечивают автоматическое создание и обновление структуры базы при запуске
* **Все некорректные запросы с JSON получают корректный ответ с HTTP 400 и описанием ошибки**

---

## API Основные эндпоинты

### POST `/games`

Создание новой игры. Параметры игры подтягиваются из окружения.

**Пример ответа:**

```json
{
  "id": 1,
  "currentTurnPlayerNumber": 1,
  "boardSize": 3,
  "winLength": 3,
  "status": "Игра",
  "winnerPlayerNumber": null,
  "createdAt": "2025-07-18T15:15:17.799983Z"
}
```

---

### POST `/games/{id}/moves`

Выполнить ход в игре с ID = `{id}`, X = `{x}`, Y = `{y}` 

```
curl -X 'POST' \
  'http://localhost:8080/api/moves/make-move/{id}?x={x}&y={y}' \
  -H 'accept: text/plain' \
  -H 'Idempotency-Key: key135413245asdf' \
  -d ''
```

---

### GET `/games/{id}`

Получить текущее состояние игры по ID.

---

### GET `/health`

Проверка работоспособности сервиса (возвращает 200 OK)

---

## Тесты

* Запуск тестов из TicTacToe.Tests:

```bash
dotnet test
```
* Также запуск в CI при пуше
---

## Контакты

* TG: [holo21k](https://t.me/holo21k)
* Email: [nneketaa@yandex.ru](mailto:nneketaa@yandex.ru)
