# .NET Core Web API + PostgreSQL (data + logging) — Starter Template

TestKube 2-project (ตามโครงสร้างจริงที่ใช้): **TestKubeApi** + **TestKubeLibrary**

## โครงสร้าง

```
.
├── src/
│   ├── TestKubeApi/                    # Web + business/service/repo layer
│   │   ├── Controllers/TestKubeController.cs          # เรียก business logic + log ที่เดียว (LogRequest/LogSuccess/LogError)
│   │   ├── BusinessLogic/               # ITestKubeBusinessLogic/TestKubeBusinessLogic (ทำงานแล้ว throw เฉยๆ) + ErrorClassifier
│   │   ├── Services/                    # ITestKubeService/TestKubeService (เรียก API ภายนอก)
│   │   ├── Repositories/                # ITestKubeRepository/TestKubeRepository (query DB)
│   │   ├── Middleware/GlobalExceptionHandler.cs        # exception -> ProblemDetails (400/404/503/500)
│   │   ├── HealthChecks/PostgresHealthCheck.cs         # /health เช็คต่อ Postgres ได้จริงไหม
│   │   ├── Configuration/ApplicationServiceConfig.cs   # DI wiring รวมที่เดียว
│   │   ├── Configuration/StartupChecks.cs              # เช็คต่อ DB ได้จริงตอน startup ไม่ได้ก็ Environment.Exit(1)
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── TestKubeApi.csproj
│   │
│   └── TestKubeLibrary/                # Data access / Models / Logging / Exceptions
│       ├── Models/                      # TestKubeTest, LogRequestEntry, LogResponseEntry, Services/JsonTest
│       ├── Data/DapperDataAccess.cs     # generic raw SQL ผ่าน Dapper (DB-agnostic)
│       ├── Data/NpgsqlConnectionFactory.cs # สร้าง connection (จุดเดียวที่ผูกกับ Postgres)
│       ├── Interfaces/                  # IDataAccess, IDbConnectionFactory, ILogService
│       ├── Logging/PostgresLogService.cs # เขียน log ลงตาราง log_req / log_resp
│       ├── Logging/OracleLogService.cs  # ทางเลือกเดิม (ยังไม่ได้ใช้ - สลับได้ที่ DI)
│       ├── Exceptions/                  # AppException, NotFoundException, ValidationException, RepositoryException
│       └── TestKubeLibrary.csproj
│
├── db/                                   # DDL รันเรียงตามเลข (ตอนนี้ยังไม่ได้ใช้ เก็บไว้ใช้ตอนต่อ DB)
│   ├── 001_create_testkube_test.sql
│   └── 002_create_log_tables.sql
├── DotnetTestKube.sln
├── Dockerfile
├── rename-project.ps1                    # เปลี่ยนชื่อโปรเจค เช่น ./rename-project.ps1 TestKube
└── .vscode/
```

## แนวคิดของโครงสร้าง

Flow: **Controller → BusinessLogic → (Service | Repository)**

- **TestKubeController** — ที่เดียวที่ log: ทุก action ทำเหมือนกันเป๊ะ `LogRequestAsync` ก่อนเรียก business logic → `try { เรียก business logic, LogSuccessAsync, return Ok(...) } catch { LogErrorAsync แล้ว throw ต่อ }` ให้ handler จัดการ response
- **GlobalExceptionHandler** — ดักทุก exception ที่หลุดจาก pipeline แปลงเป็น **ProblemDetails** (RFC 7807) + map status:

  | exception | status |
  |---|---|
  | `ValidationException` | 400 |
  | `NotFoundException` | 404 |
  | `RepositoryException` (DB error) | 503 |
  | อื่นๆ | 500 |

  > controller ยัง log ผลลัพธ์ (success/error) ลง `log_resp` เหมือนเดิม — handler แค่จัดรูป HTTP response (คนละหน้าที่กัน)
- **TestKubeBusinessLogic** — ตัวคุม flow เฉยๆ: แต่ละเมธอด เรียก API นอกผ่าน `_service` หรือ query DB ผ่าน `_repository`, ปั้นข้อมูล, return หรือ throw — **ไม่ log เอง** (controller เป็นคนคุม logging ทั้งหมด)
- **TestKubeRepository** — query DB (`testkube_test`) ผ่าน `IDataAccess` เขียน SQL ตรง ไม่มี try/catch (ปล่อยให้ business logic คุม)
- **TestKubeService** — เรียก API ภายนอกผ่าน typed `HttpClient`
- **TestKubeLibrary/Data/DapperDataAccess.cs** — data access กลาง (DB-agnostic) ยิง raw SQL + parameters ผ่าน **Dapper**:
  - `LoadData<T>(sql, param)` — query คืน list
  - `SaveData(sql, param)` — execute (INSERT/UPDATE/DELETE) คืนจำนวนแถวที่กระทบ
  - `SaveDataTransaction(commands)` — หลายคำสั่งใน transaction เดียว (สำเร็จหมดหรือ rollback หมด)
- **IDbConnectionFactory / NpgsqlConnectionFactory** — จุดเดียวที่ผูกกับ database จริง (`new NpgsqlConnection`) โค้ดชั้นบนคุยกับ `DbConnection`/Dapper ล้วนๆ
- **TestKubeApi/HealthChecks/PostgresHealthCheck.cs** — เปิด connection จริงเช็ค `/health` (`AddCheck<PostgresHealthCheck>("postgresql")` ใน `Program.cs`)
- **TestKubeApi/Configuration/StartupChecks.cs** — ตอน `app.Build()` เสร็จ เปิด connection จริงเช็คก่อนรับ request ถ้าต่อ DB ไม่ได้ → พิมพ์ error แดงแล้ว `Environment.Exit(1)` ทันที (กันแอป "ดูรันได้" ทั้งที่ DB พัง เพราะ `NpgsqlConnectionFactory.CreateConnection()` แค่ `new` object ไม่ได้เปิดจริง)
- **TestKubeLibrary/Logging/PostgresLogService.cs** — เขียน log ลงตาราง `log_req` (ก่อนเรียก business logic ผ่าน `LogRequestAsync`) และ `log_resp` (หลัง business logic return ทั้ง success/error ผ่าน `LogSuccessAsync`/`LogErrorAsync`) ผ่าน `IDataAccess` เดียวกับข้อมูลธุรกิจ ถ้าเขียนไม่สำเร็จจะ fallback ไป `ILogger` แทน ไม่ throw ต่อ (กัน logging พังแล้วลากทั้งแอปพัง)
- **TestKubeApi/BusinessLogic/ErrorClassifier.cs** — ใน `catch (Exception ex)` ของ business logic จะ classify error เป็น 5 ประเภทแล้วเก็บลงคอลัมน์ `category`:

  | category | เมื่อไหร่ |
  |---|---|
  | `DB_CONNECTION` | ต่อ database ไม่ได้ (Npgsql connection) |
  | `DB_QUERY` | SQL error จาก server เช่น table ไม่มี / constraint (`PostgresException`) |
  | `DATA_FORMAT` | format/parse ผิด (`FormatException`, `JsonException`, `InvalidCastException`) |
  | `VALIDATION` | ข้อมูลไม่ผ่าน validate (`ValidationException`) |
  | `UNKNOWN` | error อื่นๆ ที่ไม่เข้าพวกข้างบน |

- **TestKubeLibrary/Exceptions/** เก็บ exception type ของแอปเอง — error จาก database ถูกห่อเป็น `RepositoryException` ใน `DapperDataAccess`

ทั้งข้อมูลธุรกิจ (`testkube_test`) และ log (`log_req` / `log_resp`) เก็บที่ **Postgres ตัวเดียวกัน** (`appdb`) — testkube จึงไม่ต้องรัน Oracle แยก (ยังเก็บ `OracleLogService` ไว้เป็นทางเลือก สลับได้ที่ `ApplicationServiceConfig`)

## สลับ database (เน้น PG แต่รองรับตัวอื่น)

DB ผูกกับที่เดียว = `IDbConnectionFactory` ถ้าจะย้ายไป SQL Server / MySQL:

1. เพิ่ม package provider (เช่น `Microsoft.Data.SqlClient`)
2. สร้าง factory ใหม่ เช่น `SqlConnectionFactory : IDbConnectionFactory` คืน `new SqlConnection(...)`
3. สลับบรรทัดเดียวใน `ApplicationServiceConfig`: `AddScoped<IDbConnectionFactory, SqlConnectionFactory>()`

ชั้น `DapperDataAccess` / repository / business logic **ไม่ต้องแก้** เพราะคุยผ่าน `DbConnection` + Dapper

> ⚠️ ข้อจำกัด: abstraction นี้ครอบแค่ **connection/execution** — ตัว **SQL ในrepository ยังเป็น dialect ของ Postgres** (เช่น `RETURNING`, `SERIAL`) ถ้าย้าย DB ต้องปรับ SQL ใน repository ให้ตรง dialect ปลายทางเอง (SQL Server ใช้ `OUTPUT`, `IDENTITY`)

## เริ่มต้นใช้งาน

### ⚠️ ตอนนี้ปิด DB อยู่ (`Database:Enabled = false`)

ตอนนี้ DB ถูกปิดไว้ (ไม่ได้ลบ) เพราะจะให้ Kubernetes จัดการ DB cluster ทีหลัง
- ใน `appsettings.json` ตั้ง `"Database": { "Enabled": false }` ไว้ ถ้าจะเปิดกลับให้ใช้ env var `Database__Enabled=true` คู่กับ `ConnectionStrings__Default`
- ตอนปิด DB: ไม่เช็ค DB ตอน start, `/health` ไม่เช็ค Postgres, log ออก stdout ผ่าน `ConsoleLogService` (ดูได้ด้วย `kubectl logs`), endpoint ที่ใช้ DB ตอบ 503, ส่วน `/api/testkube/get-data` ยังใช้ได้ปกติ
- ไม่มี docker-compose แล้ว ตัวแอปจะ deploy เข้า Kubernetes จาก image ที่ build ด้วย `Dockerfile` ส่วน DB ให้ cluster จัดการทีหลัง

### รันแบบไม่มี DB (ค่าเริ่มต้น)

```bash
cd src/TestKubeApi
dotnet run
```

### (ภายหลัง) รัน Postgres เอง (Docker แบบ standalone) + `dotnet run` — ต้องตั้ง `Database__Enabled=true`

```bash
docker run -d --name pg-testkube \
  -e POSTGRES_DB=appdb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -v pgdata:/var/lib/postgresql/data \
  -v "$(pwd)/db":/docker-entrypoint-initdb.d:ro \
  postgres:16-alpine
```

หยุด/เปิดใหม่ตามใจ (data เก็บใน volume `pgdata`):

```bash
docker stop pg-testkube     # หยุด คืนแรม
docker start pg-testkube    # เปิดใหม่ (ชื่อ + data เดิม)
docker rm -f pg-testkube    # ลบ container (data ยังอยู่)
docker volume rm pgdata      # ลบ data ล้างสะอาด
```

เชื่อมผ่าน client (เช่น DBeaver): `localhost:5432` / db `appdb` / user `postgres` / pass `postgres`

รัน API:

```bash
cd src/TestKubeApi
dotnet run
```

`dotnet run` เฉยๆ พอ — `Properties/launchSettings.json` ตั้ง port `5080` + `ASPNETCORE_ENVIRONMENT=Development` + เปิด Swagger ให้อัตโนมัติ:
- API: `http://localhost:5080`
- Swagger: `http://localhost:5080/swagger`

API ต่อ Postgres ที่ `localhost:5432` ให้เองตาม `appsettings.json` หรือเปิด VS Code แล้วกด F5 ก็ได้ — ถ้าต่อไม่ได้ตอน startup แอปจะพิมพ์ error สีแดงแล้ว exit ทันที (ดู `Configuration/StartupChecks.cs`)

**secret จริง** (password, connection string ที่มี credential) ใส่ใน `src/TestKubeApi/appsettings.Local.json` (gitignored, `Program.cs` โหลดทับ `appsettings.json` อัตโนมัติถ้ามีไฟล์นี้) — ห้าม commit ค่าจริงลง `appsettings.*.json`

## สร้างตาราง (Postgres)

ไม่ได้ใช้ EF migration แล้ว — สร้าง schema เองด้วย SQL (รันผ่าน DBeaver หรือ `psql`, หรือ mount `./db` เข้า `docker-entrypoint-initdb.d` ของ container Postgres) มี 3 ตาราง: `testkube_test` (ข้อมูลธุรกิจ), `log_req` (1 แถวต่อ request ที่เข้ามา บันทึกก่อนเรียก business logic), `log_resp` (1 แถวต่อผลลัพธ์ ทั้ง success/error บันทึกก่อน business logic return)

```sql
-- ข้อมูลธุรกิจ
CREATE TABLE IF NOT EXISTS testkube_test (
    id          SERIAL PRIMARY KEY,
    title       TEXT        NOT NULL,
    is_done     BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 1 แถวต่อ request ที่เข้ามา controller (ก่อนเรียก business logic)
CREATE TABLE IF NOT EXISTS log_req (
    id           BIGSERIAL PRIMARY KEY,
    request      TEXT,
    username     VARCHAR(100),
    created_by   VARCHAR(100) NOT NULL,   -- ชื่อแอป จาก appsettings "AppName"
    created_date TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- 1 แถวต่อผลลัพธ์ (success/error) บันทึกก่อน business logic return
-- category = ประเภท error จาก ErrorClassifier (null เมื่อ success)
CREATE TABLE IF NOT EXISTS log_resp (
    id               BIGSERIAL PRIMARY KEY,
    request_id       BIGINT REFERENCES log_req (id),
    log_level        VARCHAR(20)  NOT NULL,   -- INFO / ERROR
    category         VARCHAR(40),
    source           VARCHAR(200) NOT NULL,   -- เช่น TestKubeBusinessLogic.CreateAsync
    message          TEXT,
    exception_detail TEXT,                    -- ex.ToString() เต็มๆ (null เมื่อ success)
    created_at       TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS idx_log_req_created_date ON log_req (created_date);
CREATE INDEX IF NOT EXISTS idx_log_resp_request_id  ON log_resp (request_id);
CREATE INDEX IF NOT EXISTS idx_log_resp_created_at  ON log_resp (created_at);
CREATE INDEX IF NOT EXISTS idx_log_resp_category    ON log_resp (category);
```

> คอลัมน์เป็น snake_case (`is_done`, `created_at`) — `DapperDataAccess` ตั้ง `Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true` ให้ map เข้ากับ property PascalCase (`IsDone`, `CreatedAt`) อัตโนมัติ

DDL อยู่ใน `db/001_create_testkube_test.sql` และ `db/002_create_log_tables.sql` แล้ว รันผ่าน container ที่ให้ไว้ก็ได้:

```bash
docker exec -i pg-testkube psql -U postgres -d appdb < db/001_create_testkube_test.sql
docker exec -i pg-testkube psql -U postgres -d appdb < db/002_create_log_tables.sql
```

## นำไปใช้กับโปรเจคใหม่

1. คัดลอกทั้งโฟลเดอร์ แล้วรัน `./rename-project.ps1 <ชื่อใหม่>` เปลี่ยน namespace/ชื่อ project ให้อัตโนมัติ (`.csproj`, `.sln`, `using` ทุกไฟล์, `Dockerfile`, `launch.json`) เพิ่ม `-All` ถ้าจะเปลี่ยนชื่อคลาสตัวอย่าง/ตาราง DB ด้วย
2. เปลี่ยน `TestKubeTest` / `TestKubeController` / `TestKubeBusinessLogic` / `TestKubeRepository` ตัวอย่างให้เป็น entity/flow ของจริง ตาม pattern เดิม (controller → business logic → repository/service)
3. แก้ connection string `Default` ใน `appsettings.Development.json` (dev) และตั้ง env var ตอน prod
4. เปลี่ยน `AppName` ใน `appsettings.json` เป็นชื่อแอปจริง (ใช้เป็น `created_by` ใน `log_req`)
5. **อย่า commit password จริงขึ้น git** — ใช้ `appsettings.Local.json` (gitignored, dev), environment variable, หรือ secret manager (prod)

## Deploy

```bash
docker build -t myapp-api:latest .
docker run -p 8080:8080 \
  -e ServiceAPI__ApiUrl="https://<prod-external-api>" \
  myapp-api:latest
```

ตอนจะต่อ DB เพิ่ม env `Database__Enabled=true` และ `ConnectionStrings__Default="Host=...;Password=..."` (ใน Kubernetes ให้เก็บ connection string ใน `Secret`)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop
- VS Code + extensions ใน `.vscode/extensions.json`
