# TestApi — тестовое задание (.NET 10)

REST API: разбор HTML (AngleSharp), поиск email, AES-256-ECB decrypt, запись элементов в PostgreSQL (Dapper).

## Запуск

```bash
docker compose up --build
```

| Сервис | URL |
|--------|-----|
| API / Swagger | http://localhost:8090/api/swagger |
| pgAdmin | http://localhost:8080 (`admin@admin.com` / `admin`) |
| PostgreSQL | `localhost:5432`, БД `testdb`, user/password `postgres` |

Остановка:

```bash
docker compose down
```

Данные БД сохраняются в volume `pgdata` (не теряются при перезапуске). Полный сброс БД: `docker compose down -v`.

## API

- **POST** `/api/process`
- Body: JSON (`selector`, `attribute`, `url_b64`, `encrypted_text_bytes_b64`, `key_bytes_b64`, `page_b64`)
- Примеры входа: `json_payload_1.txt`, `json_payload_2.txt`
- Примеры ответа: `json_result_1.txt`, `json_result_2.txt`

## Структура

```
├── compose.yml          # api + postgres:18 + pgadmin
├── Dockerfile           # SDK, restore + run при старте
├── TestApi/             # исходники приложения
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
├── pgadmin/             # servers.json, pgpass
├── json_result_1.txt
└── json_result_2.txt
```

## Локальный запуск без Docker

```bash
cd TestApi
dotnet run
```

Swagger: порт смотри в консоли (часто `http://localhost:5075/api/swagger`).  
Для записи в БД нужен PostgreSQL и строка подключения в `appsettings.json`.
