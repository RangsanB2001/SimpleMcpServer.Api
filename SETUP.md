# SETUP — SimpleMcpServer.Api

คู่มือการติดตั้งและใช้งานโปรเจกต์ **SimpleMcpServer.Api** ซึ่งเป็น MCP Server
(Model Context Protocol) สำหรับให้บริการข้อมูลตลาดทุนไทย (SET/mai) โดยดึงข้อมูลจาก
ฐานข้อมูล PSIMS (MySQL)

> ข้อมูลสถาปัตยกรรมเชิงลึกดูเพิ่มเติมได้ที่ [ARCHITECTURE.md](ARCHITECTURE.md)

---

## 1. ภาพรวม

- เป็น MCP-compatible tool provider ที่ใช้ official **MCP .NET SDK**
  (`ModelContextProtocol.AspNetCore`)
- เปิดให้เรียกใช้ tools ผ่าน 2 ช่องทาง
  - **HTTP Streamable MCP** ที่ path `/mcp`
  - **stdio transport** เมื่อสั่งรันด้วย flag `--stdio`
- โครงสร้างแบบ **Clean Architecture** แบ่งเป็น Domain / Application / Infrastructure / Api
- ข้อมูลจริงดึงจากฐานข้อมูล **PSIMS (MySQL)** ผ่าน Dapper + MySqlConnector

---

## 2. สิ่งที่ต้องมี (Prerequisites)

| รายการ | เวอร์ชัน / หมายเหตุ |
|--------|----------------------|
| .NET SDK | **8.0** (TargetFramework = `net8.0`) |
| การเชื่อมต่อฐานข้อมูล | MySQL/PSIMS (port `3306`, database `psims`) — host กำหนดผ่าน config |
| Docker (ทางเลือก) | สำหรับรันแบบ container |
| MCP Client (ทางเลือก) | เช่น Claude Desktop, MCP Inspector สำหรับเรียกใช้ tools |

ตรวจสอบเวอร์ชัน .NET:

```powershell
dotnet --version
```

---

## 3. โครงสร้างโปรเจกต์

```text
SimpleMcpServer.sln
├── src/
│   ├── SimpleMcpServer.Domain/           # entities, constants (ไม่มี dependency)
│   ├── SimpleMcpServer.Application/       # tool classes, provider ports (interfaces)
│   ├── SimpleMcpServer.Infrastructure/    # provider implementations (PSIMS/MySQL, Dapper)
│   └── SimpleMcpServer.Api/               # hosting + MCP transport setup (entry point)
└── tests/
    └── SimpleMcpServer.Application.Tests/  # xUnit tests
```

ทิศทาง dependency:

```text
Api ──► Application ──► Domain
Api ──► Infrastructure ──► Application ──► Domain
```

---

## 4. การตั้งค่า (Configuration)

### 4.1 Connection String

โปรเจกต์อ่าน connection string ชื่อ `DefaultConnection` จาก
[appsettings.json](src/SimpleMcpServer.Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=<DB_HOST>;Port=3306;Database=psims;User Id=<DB_USER>;Password=<DB_PASSWORD>;SslMode=Preferred;ConnectionTimeout=10;DefaultCommandTimeout=30;ConvertZeroDateTime=True;"
  }
}
```

> ⚠️ **ข้อควรระวังด้านความปลอดภัย:**
> - ค่าด้านบนใช้ placeholder (`<DB_HOST>`, `<DB_USER>`, `<DB_PASSWORD>`) โดยตั้งใจ —
>   **อย่าใส่ host/username/password ตัวจริงลงในเอกสารนี้หรือไฟล์ใดที่ถูก commit เข้า git**
> - ปัจจุบันไฟล์ [appsettings.json](src/SimpleMcpServer.Api/appsettings.json) มี credential
>   ตัวจริงฝังอยู่และถูก track ใน git อยู่ ควรย้ายออกไปใช้ **User Secrets** หรือ
>   **Environment Variables** แทน (ดูหัวข้อ 4.2) แล้วลบค่าออกจาก `appsettings.json`

### 4.2 ทางเลือกที่ปลอดภัยกว่าสำหรับ credential

โปรเจกต์เปิดใช้ User Secrets แล้ว (มี `UserSecretsId` ใน `.csproj`):

```powershell
# กำหนด connection string ผ่าน User Secrets (ไม่ถูก commit เข้า git)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=psims;User Id=...;Password=...;" `
  --project src/SimpleMcpServer.Api
```

หรือผ่าน Environment Variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=psims;..."
```

### 4.3 Environment

profile `http`/`https` ตั้ง `ASPNETCORE_ENVIRONMENT=Development` ไว้แล้วใน
[launchSettings.json](src/SimpleMcpServer.Api/Properties/launchSettings.json)

---

## 5. Build & Run

### 5.1 Restore + Build

```powershell
dotnet restore SimpleMcpServer.sln
dotnet build SimpleMcpServer.sln
```

### 5.2 รันแบบ HTTP (โหมดหลัก)

```powershell
dotnet run --project src/SimpleMcpServer.Api
```

- ค่าเริ่มต้น HTTP: `http://localhost:5063`
- HTTPS (profile `https`): `https://localhost:7293`
- MCP endpoint: `POST http://localhost:5063/mcp`

เลือก profile เฉพาะ:

```powershell
dotnet run --project src/SimpleMcpServer.Api --launch-profile https
```

### 5.3 รันแบบ stdio (สำหรับเชื่อมต่อ MCP client โดยตรง)

```powershell
dotnet run --project src/SimpleMcpServer.Api -- --stdio
```

โหมด stdio จะปิด console logging และสื่อสารผ่าน stdin/stdout ตามมาตรฐาน MCP

---

## 6. การทดสอบ / เรียกใช้ MCP Endpoint

HTTP transport ต้องส่ง header ดังนี้เสมอ:

```text
Accept: application/json, text/event-stream
Content-Type: application/json
```

### 6.1 ใช้ไฟล์ .http (VS / VS Code REST Client)

มีตัวอย่างพร้อมใช้ที่ [SimpleMcpServer.Api.http](src/SimpleMcpServer.Api/SimpleMcpServer.Api.http)

### 6.2 ตัวอย่างด้วย curl (PowerShell)

**ขอรายการ tools ทั้งหมด:**

```powershell
curl -Method POST http://localhost:5063/mcp `
  -Headers @{ "Accept" = "application/json, text/event-stream"; "Content-Type" = "application/json" } `
  -Body '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

**เรียก tool `fundamental_lookup` (PTT ไตรมาสล่าสุด):**

```powershell
curl -Method POST http://localhost:5063/mcp `
  -Headers @{ "Accept" = "application/json, text/event-stream"; "Content-Type" = "application/json" } `
  -Body '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"fundamental_lookup","arguments":{"symbol":"PTT"}}}'
```

**เรียกข้อมูลราคา (date range):**

```powershell
curl -Method POST http://localhost:5063/mcp `
  -Headers @{ "Accept" = "application/json, text/event-stream"; "Content-Type" = "application/json" } `
  -Body '{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"daily_prices","arguments":{"symbol":"PTT","fromDate":"2024-01-01","toDate":"2024-01-31"}}}'
```

---

## 7. การเชื่อมต่อกับ MCP Client (Claude Desktop)

ตัวอย่าง config สำหรับ Claude Desktop โดยใช้ stdio transport
(`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "efin-set": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "D:\\efin.finance\\SimpleMcpServer.Api\\src\\SimpleMcpServer.Api",
        "--",
        "--stdio"
      ]
    }
  }
}
```

> สำหรับ production แนะนำ build/publish เป็น DLL แล้วชี้ `command` ไปที่
> `dotnet` + path ของ `SimpleMcpServer.Api.dll` เพื่อให้เริ่มทำงานเร็วขึ้น

---

## 8. การรัน Tests

โปรเจกต์ test ใช้ **xUnit**:

```powershell
dotnet test SimpleMcpServer.sln
```

รันเฉพาะ test project:

```powershell
dotnet test tests/SimpleMcpServer.Application.Tests
```

---

## 9. Docker (ทางเลือก)

มี Dockerfile และ docker-compose ให้แล้ว
([Dockerfile](src/SimpleMcpServer.Api/Dockerfile), [docker-compose.yml](docker-compose.yml))

```powershell
# build + run ผ่าน docker compose
docker compose up --build
```

ภายใน container เปิด port `8080` (HTTP) และ `8081` (HTTPS)

> หมายเหตุ: connection string ชี้ไปที่ฐานข้อมูลภายในเครือข่าย (internal LAN)
> หาก container เข้าถึง network นั้นไม่ได้ ต้องปรับ connection string ผ่าน
> environment variable ตามหัวข้อ 4.2

---

## 10. รายการ MCP Tools

### 10.1 Tools ที่ใช้งานได้จริง (PSIMS-backed)

| Tool name | หน้าที่ |
|-----------|---------|
| `fundamental_lookup` | ข้อมูลพื้นฐาน (P/E, P/BV, ROE, ROA, EPS, market cap ฯลฯ) |
| `financial_statements` | งบการเงิน (ระบุ fiscal/quarter/finStateType) |
| `daily_prices` | ราคาปิดรายวัน / ช่วงวันที่ |
| `daily_stats` | สถิติรายวัน |
| `dividends` | ประวัติเงินปันผล |
| `par_changes` | ประวัติการเปลี่ยนแปลงพาร์ |
| `company_profile` | ข้อมูลบริษัท / ประเภทธุรกิจ |
| `security_master` | ข้อมูลหลักทรัพย์ (master data) |
| `foreign_room` | สัดส่วนการถือครองของต่างชาติ |
| `nvdr_holdings` | การถือครอง NVDR |
| `short_position` | สถานะ short |
| `trading_signs` / `trading_sign_reasons` | เครื่องหมายซื้อขาย + เหตุผล |
| `free_float` | สัดส่วน free float |
| `investor_breakdown` | สัดส่วนนักลงทุนแต่ละประเภท |

### 10.2 SRS Phase 1 Aliases (PSIMS-backed)

| Tool name | หน้าที่ |
|-----------|---------|
| `efin.set.stock.search` | ค้นหาหุ้นด้วย symbol/ชื่อ/keyword/market |
| `efin.set.stock.profile.get` | profile หุ้น + security master |
| `efin.market.price.latest.get` | ราคาล่าสุด (EOD) |
| `efin.market.price.history.get` | ราคาย้อนหลัง (default period = 6M) |
| `efin.market.price.ohlcv.get` | OHLCV (รองรับ timeframe `1d`) |

### 10.3 Tools ที่ยังไม่พร้อมใช้ (Phase 1)

tools กลุ่มข่าวและเอกสารยังไม่ได้ตั้งค่า source — จะคืน
`status = "source_not_configured"` และ **จะไม่สร้างข้อมูลปลอม**:

`efin.set.news.search`, `efin.set.news.by_symbol.get`, `efin.news.search`,
`efin.news.by_symbol.get`, `efin.news.summary.generate`, `efin.document.search`,
`efin.document.qa.answer`

---

## 11. การเพิ่ม Tool ใหม่

1. สร้างคลาส `[McpServerToolType]` ในโฟลเดอร์ `src/SimpleMcpServer.Application/Tools/`
2. เพิ่ม method ที่มี attribute `[McpServerTool(Name = "...")]`
3. ถ้าต้องเข้าถึงข้อมูล: นิยาม interface (port) ใน Application แล้ว implement ใน Infrastructure
4. register provider ใน [Infrastructure/DependencyInjection.cs](src/SimpleMcpServer.Infrastructure/DependencyInjection.cs)
5. เขียน test ใน `tests/SimpleMcpServer.Application.Tests/`

> tools ถูกค้นพบอัตโนมัติด้วย `WithToolsFromAssembly(...)` ใน
> [Program.cs](src/SimpleMcpServer.Api/Program.cs) — ไม่ต้อง register tool เอง
> แต่ provider ที่ tool ใช้ต้องถูก register ใน DI
