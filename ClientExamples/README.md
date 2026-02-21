# Universal API Gateway — Client Test Examples

This folder contains **client-side only** integration examples for the Universal API Gateway.

## 1) API analysis (from Swagger/controllers)

- **Main endpoint**: `POST /v1/ai/execute`
- **Base URL (local default)**: `https://localhost:7135/`
- **Request DTO**:

```json
{
  "providerKey": "string (required)",
  "payload": "string (required)"
}
```

- **Response DTO**:

```json
{
  "providerKey": "string",
  "result": "string"
}
```

- **Auth headers**: none required by default in current API setup.

## 2) Folder structure

```text
ClientExamples/
├── BasicHttpClientExample.cs
├── GatewayClient.cs
├── README.md
└── ConsoleTestClient/
    ├── ConsoleTestClient.csproj
    └── Program.cs
```

## 3) Files included

- `BasicHttpClientExample.cs`
  - Minimal `HttpClient` POST sample
  - JSON payload + response deserialization
  - non-success error handling
- `GatewayClient.cs`
  - reusable strongly-typed client
  - `SendTextAsync(...)`
  - shared request/response models
  - scenario helpers (Text→Text, Text→Image, Audio→Text, Any→Any)
- `ConsoleTestClient/Program.cs`
  - async `Main`
  - executes all scenarios
  - prints responses and catches gateway/client errors

## 4) Sample request/response JSON

### Sample request

```json
{
  "providerKey": "echo",
  "payload": "Hello from client"
}
```

### Sample response

```json
{
  "providerKey": "echo",
  "result": "Hello from client"
}
```

## 5) cURL examples

### Basic text request

```bash
curl -X POST "https://localhost:7135/v1/ai/execute" \
  -H "Content-Type: application/json" \
  -d '{"providerKey":"echo","payload":"Hello from curl"}'
```

### Text → image request

```bash
curl -X POST "https://localhost:7135/v1/ai/execute" \
  -H "Content-Type: application/json" \
  -d '{"providerKey":"replicate","payload":"{\"task\":\"text-to-image\",\"prompt\":\"Futuristic city\",\"size\":\"1024x1024\"}"}'
```

### Audio → text request (if provider enabled)

```bash
curl -X POST "https://localhost:7135/v1/ai/execute" \
  -H "Content-Type: application/json" \
  -d '{"providerKey":"assemblyai","payload":"{\"task\":\"audio-to-text\",\"audioBase64\":\"<base64-audio>\",\"language\":\"en\"}"}'
```

## 6) Frontend JavaScript `fetch()` example

```javascript
async function callGateway() {
  const response = await fetch("https://localhost:7135/v1/ai/execute", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      providerKey: "echo",
      payload: "Hello from fetch"
    })
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Gateway error: ${response.status} ${errorText}`);
  }

  const result = await response.json();
  console.log("Gateway result:", result);
  return result;
}
```

## 7) Usage instructions

1. Ensure API is running with HTTPS on `https://localhost:7135`.
2. Run the console sample:

```bash
dotnet run --project ClientExamples/ConsoleTestClient/ConsoleTestClient.csproj
```

3. Adjust provider keys (`echo`, `replicate`, `assemblyai`, `openrouter`) to match your configured providers.
4. For self-signed local certs, trust dev cert if needed:

```bash
dotnet dev-certs https --trust
```
