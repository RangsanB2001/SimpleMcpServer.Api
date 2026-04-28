# ARCHITECTURE.md

## Overview

Advanced MCP-compatible server with multiple transports.

Supported transports:

* HTTP JSON-RPC
* WebSocket
* STDIO

---

## Architecture

```text id="n6xpb7"
Client / AI Agent
      ↓
Transport Layer
  ├── HTTP
  ├── WebSocket
  └── STDIO
      ↓
Tool Registry
      ↓
Tool Execution
      ↓
Response
```

---

## Core Components

### ToolRegistry

centralized tool registration and execution

---

### AuthService

simple API key validation

---

### WebSocketHandler

real-time bidirectional transport

---

### StdioServer

desktop / local MCP integration

---