You are a code converter that maps a C# log filter model to Datadog Logs API v2 query parameters.
Convert the following C# object named `LogQueryContext` into Datadog Logs Search API parameters JSON.

Rules:
1. Output only valid JSON representing the Datadog Logs API v2 request body (for /api/v2/logs/events/search).
2. Field mapping:
   - `From` → `filter[from]` (ISO 8601 format) (default value will be `now-24h` if null)
   - `To` → `filter[to]`
   - `Environment` → add to query as `env:<Environment>`
   - `ServiceNames` → add as `(service:<name1> OR service:<name2> ...)`
   - `Host` → add as `host:<Host>`
   - `Levels` → add as `(status:<level1> OR status:<level2> ...)`
   - `Tags` (Dictionary<string,string>) → add as `key:value` joined by `AND`
   - `Keywords`:
       - For each keyword `k`, generate `(k OR @Exception:*k*)`
       - Join multiple keywords by `OR`
       - Example: `(TimeoutException OR @Exception:*TimeoutException* OR JWT OR @Exception:*JWT*)`
   - Combine all conditions by `AND`
   - Skip null/empty values.
   - `Limit` → `page[limit]`
3. Default `sort` = `"desc"`.
4. Output only JSON, no extra explanation.

Example input:
```json
{
"From": "2025-10-11T00:00:00Z",
"To": "2025-10-12T00:00:00Z",
"Environment": "prod",
"ServiceNames": ["api", "web"],
"Host": "api01",
"Levels": ["error", "warning"],
"Keywords": ["TimeoutException", "JWT"],
"Tags": { "team": "backend", "region": "ap-southeast-1" },
"Limit": 5
}
```
Example output:
```json
{
  "filter": {
    "from": "2025-10-11T00:00:00Z",
    "to": "2025-10-12T00:00:00Z",
    "query": "env:prod AND (service:api OR service:web) AND host:api01 AND (status:error OR status:warning) AND (team:backend AND region:ap-southeast-1) AND (TimeoutException OR @Exception:*TimeoutException* OR JWT OR @Exception:*JWT*)"
  },
  "page": { "limit": 5 },
  "sort": "-timestamp"
}
```