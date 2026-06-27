---
name: console-get-logs
description: |-
  Retrieve captured Godot-MCP editor log lines, newest-first. The Godot analog of Unity's 'console-get-logs'. NOTE: Godot's C# API exposes no global log hook, so this returns the plugin's own captured editor activity (not the entire Godot editor console).
  Inputs:
    - 'maxEntries' (default 100, min 1): caps the returned array (most-recent lines kept).
    - 'logTypeFilter' (default null = all): restrict to Log / Warning / Error.
    - 'includeStackTrace' (default false): include stack-trace strings.
    - 'lastMinutes' (default 0 = all): only lines captured in the last N minutes.
---

# Console / Get Logs

Retrieve captured Godot-MCP editor log lines, newest-first. The Godot analog of Unity's 'console-get-logs'. NOTE: Godot's C# API exposes no global log hook, so this returns the plugin's own captured editor activity (not the entire Godot editor console).
Inputs:
  - 'maxEntries' (default 100, min 1): caps the returned array (most-recent lines kept).
  - 'logTypeFilter' (default null = all): restrict to Log / Warning / Error.
  - 'includeStackTrace' (default false): include stack-trace strings.
  - 'lastMinutes' (default 0 = all): only lines captured in the last N minutes.

## How to Call

### HTTP API (Direct Tool Execution)

Execute this tool directly via the MCP Plugin HTTP API:

```bash
curl -X POST https://ai-game.dev/mcp/api/tools/console-get-logs \
  -H "Content-Type: application/json" \
  -d '{
  "maxEntries": 0,
  "logTypeFilter": "string_value",
  "includeStackTrace": false,
  "lastMinutes": 0
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use `-d @args.json`.
>
> Or pipe via stdin:
> ```bash
> curl -X POST https://ai-game.dev/mcp/api/tools/console-get-logs -H "Content-Type: application/json" -d @- <<'EOF'
> {"param": "value"}
> EOF
> ```

#### With Authorization (if required)

```bash
curl -X POST https://ai-game.dev/mcp/api/tools/console-get-logs \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
  "maxEntries": 0,
  "logTypeFilter": "string_value",
  "includeStackTrace": false,
  "lastMinutes": 0
}'
```

## Input

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `maxEntries` | `integer` | No | Maximum number of log entries to return. Minimum 1, default 100. |
| `logTypeFilter` | `any` | No | Filter by severity (Log / Warning / Error). Null means all severities. |
| `includeStackTrace` | `boolean` | No | Include stack traces in the output. Default false. |
| `lastMinutes` | `integer` | No | Return logs from the last N minutes. 0 returns all available logs. Default 0. |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "maxEntries": {
      "type": "integer",
      "description": "Maximum number of log entries to return. Minimum 1, default 100."
    },
    "logTypeFilter": {
      "$ref": "#/$defs/com.IvanMurzak.Godot.MCP.Data.GodotLogType",
      "description": "Filter by severity (Log / Warning / Error). Null means all severities."
    },
    "includeStackTrace": {
      "type": "boolean",
      "description": "Include stack traces in the output. Default false."
    },
    "lastMinutes": {
      "type": "integer",
      "description": "Return logs from the last N minutes. 0 returns all available logs. Default 0."
    }
  },
  "$defs": {
    "com.IvanMurzak.Godot.MCP.Data.GodotLogType": {
      "type": "string",
      "enum": [
        "Log",
        "Warning",
        "Error"
      ],
      "description": "Severity of a captured editor log line: Log, Warning, or Error."
    }
  }
}
```

## Output

### Output JSON Schema

```json
{
  "type": "object",
  "properties": {
    "result": {
      "$ref": "#/$defs/com.IvanMurzak.Godot.MCP.Data.LogEntry-1"
    }
  },
  "$defs": {
    "com.IvanMurzak.Godot.MCP.Data.LogEntry": {
      "type": "object",
      "properties": {
        "logType": {
          "type": "string",
          "enum": [
            "Log",
            "Warning",
            "Error"
          ],
          "description": "Severity of the log line (Log / Warning / Error)."
        },
        "message": {
          "type": "string",
          "description": "The log message text."
        },
        "timestamp": {
          "type": "string",
          "format": "date-time",
          "description": "UTC timestamp when the line was captured."
        },
        "stackTrace": {
          "type": "string",
          "description": "Optional stack trace associated with the line; null when none was captured."
        }
      },
      "required": [
        "logType",
        "timestamp"
      ],
      "description": "A captured Godot editor log line: severity, message, timestamp, and optional stack trace."
    },
    "com.IvanMurzak.Godot.MCP.Data.LogEntry-1": {
      "type": "array",
      "items": {
        "$ref": "#/$defs/com.IvanMurzak.Godot.MCP.Data.LogEntry",
        "description": "A captured Godot editor log line: severity, message, timestamp, and optional stack trace."
      }
    }
  },
  "required": [
    "result"
  ]
}
```

