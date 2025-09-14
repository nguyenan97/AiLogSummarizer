# AI Log Summarizer

Skeleton project with .NET 8 Web API backend and Angular frontend.

## Setup

### Backend
```bash
# create solution and project
dotnet new sln -n AiLogSummarizer
mkdir backend && cd backend
dotnet new webapi -n AiLogSummarizer.Api -f net8.0
cd ..
dotnet sln AiLogSummarizer.sln add backend/AiLogSummarizer.Api/AiLogSummarizer.Api.csproj
```
Set `OPENAI_API_KEY` and `SLACK_WEBHOOK` in `backend/AiLogSummarizer.Api/appsettings.json`.

Run:
```bash
cd backend/AiLogSummarizer.Api
dotnet run
```

### Frontend
```bash
npm install -g @angular/cli
ng new frontend --strict --standalone --routing=false --style=css
cd frontend
npm install
npm start
```
Configure `src/environments/environment.ts` with backend `apiBaseUrl`.

## Notes
- CORS is open for local development.
- Upload limit: 100MB.
