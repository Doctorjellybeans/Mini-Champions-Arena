---
name: filesystem-reimport
description: |-
  Re-scan the Godot project's res:// filesystem and/or reimport specific files, then wait for the import to settle before returning. The Godot analog of Unity's AssetDatabase.Refresh. Two modes:
    - Pass 'files' (a list of res:// paths) to reimport exactly those files via EditorFileSystem.ReimportFiles — use this after editing a source asset's bytes outside the editor.
    - Omit 'files' (or pass an empty list) to trigger a full EditorFileSystem.Scan — use this after adding/removing files on disk so Godot picks up the change.
  The call blocks until scanning completes (bounded), so a subsequent resource-find/get-data sees the settled state. Returns a short status string.
---

# FileSystem / Reimport

Re-scan the Godot project's res:// filesystem and/or reimport specific files, then wait for the import to settle before returning. The Godot analog of Unity's AssetDatabase.Refresh. Two modes:
  - Pass 'files' (a list of res:// paths) to reimport exactly those files via EditorFileSystem.ReimportFiles — use this after editing a source asset's bytes outside the editor.
  - Omit 'files' (or pass an empty list) to trigger a full EditorFileSystem.Scan — use this after adding/removing files on disk so Godot picks up the change.
The call blocks until scanning completes (bounded), so a subsequent resource-find/get-data sees the settled state. Returns a short status string.

## How to Call

### HTTP API (Direct Tool Execution)

Execute this tool directly via the MCP Plugin HTTP API:

```bash
curl -X POST https://ai-game.dev/mcp/api/tools/filesystem-reimport \
  -H "Content-Type: application/json" \
  -d '{
  "files": "string_value"
}'
```

> For complex input (multi-line strings, code), save the JSON to a file and use `-d @args.json`.
>
> Or pipe via stdin:
> ```bash
> curl -X POST https://ai-game.dev/mcp/api/tools/filesystem-reimport -H "Content-Type: application/json" -d @- <<'EOF'
> {"param": "value"}
> EOF
> ```

#### With Authorization (if required)

```bash
curl -X POST https://ai-game.dev/mcp/api/tools/filesystem-reimport \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
  "files": "string_value"
}'
```

## Input

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `files` | `any` | No | Optional list of res:// file paths to reimport. When omitted/empty, a full filesystem scan is run instead. |

### Input JSON Schema

```json
{
  "type": "object",
  "properties": {
    "files": {
      "$ref": "#/$defs/System.Collections.Generic.List(System.String)",
      "description": "Optional list of res:// file paths to reimport. When omitted/empty, a full filesystem scan is run instead."
    }
  },
  "$defs": {
    "System.Collections.Generic.List(System.String)": {
      "type": "array",
      "items": {
        "type": "string"
      }
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
      "type": "string"
    }
  },
  "required": [
    "result"
  ]
}
```

