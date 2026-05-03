# ARCHITECTURE.md

## Overview

MCP-compatible tool provider exposed over HTTP using JSON-RPC 2.0.
Layered using Clean Architecture (Domain / Application / Infrastructure / Api).

---

## Layout

```text
SimpleMcpServer.sln
└── src/
    ├── SimpleMcpServer.Domain/           (entities, no dependencies)
    ├── SimpleMcpServer.Application/      (ports, use cases, JSON-RPC contracts)
    ├── SimpleMcpServer.Infrastructure/   (port implementations: data providers)
    └── SimpleMcpServer.Api/              (controller, middleware, hosting)
```

Dependency direction (inward only):

```text
Api ──► Application ──► Domain
Api ──► Infrastructure ──► Application ──► Domain
```

---

## Request flow

```text
HTTP POST /api/rpc
      ↓
McpRpcController            (Api)
      ↓
IRpcDispatcher              (Application port)
      ↓
RpcDispatcher               (Application use case)
      ↓
ToolRegistry                (Application service)
      ↓
IDataTool implementation    (Application use case)
      ↓
IDataProvider<T>            (Application port)
      ↓
FundamentalProvider         (Infrastructure adapter)
      ↓
Response (JsonRpcResponse)
```

Errors thrown anywhere downstream are translated into a JSON-RPC error envelope by `JsonRpcExceptionMiddleware`.

---

## Components

### McpRpcController (Api)
Thin pass-through. Deserializes `JsonRpcRequest` and calls `IRpcDispatcher`.

### JsonRpcExceptionMiddleware (Api)
Catches `JsonRpcException`, `JsonException`, and unhandled exceptions and writes a spec-compliant `{ jsonrpc, id, error: { code, message, data? } }` envelope with HTTP 200.

### RpcDispatcher (Application)
Routes JSON-RPC methods (`tools/list`, `tools/call`) and validates `params`.

### ToolRegistry (Application)
Lookup of registered `IDataTool` instances by name.

### IDataTool (Application port)
Contract for any tool the server exposes. Tools throw `JsonRpcException` with `InvalidParams` on bad arguments.

### IDataProvider&lt;T&gt; (Application port)
Generic key-based data access. Implemented per entity in Infrastructure.

---

## Adding a new tool

1. Create a class in `Application/Tools/` implementing `IDataTool`.
2. Register it in `Application/DependencyInjection.cs` (`AddSingleton<IDataTool, YourTool>()`).
3. Done — `ToolRegistry` picks it up via `IEnumerable<IDataTool>` injection.
