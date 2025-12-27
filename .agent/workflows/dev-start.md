---
description: Start the full development environment (Backend API + Frontend)
---

# Development Environment Startup

This workflow starts all services needed for local development.

## Prerequisites

- Docker and Docker Compose installed
- Node.js/Bun installed (for frontend)

---

## Option A: Full Stack with Docker (Recommended)

// turbo-all

1. Navigate to the backend directory:
   ```bash
   cd /Users/chanthawat/Developments/solar-cons/construction-dotnet-rest-api
   ```

2. Ensure `.env` file exists (create from template if needed):
   ```bash
   [ ! -f .env ] && cp .env.example .env
   ```

3. Start PostgreSQL and API with Docker:
   ```bash
   docker-compose up -d
   ```

4. Wait for services to be healthy (usually 30-60 seconds):
   ```bash
   sleep 10 && docker-compose ps
   ```

5. Verify API is running:
   ```bash
   curl -s http://localhost:5001/api/v1/projects/test | head -20
   ```

6. Navigate to frontend directory:
   ```bash
   cd /Users/chanthawat/Developments/solar-cons/construction-internal-app
   ```

7. Install frontend dependencies:
   ```bash
   bun install
   ```

8. Start frontend dev server:
   ```bash
   bun run dev
   ```

---

## Option B: Backend Only (Local .NET + Docker PostgreSQL)

1. Start only PostgreSQL:
   ```bash
   cd /Users/chanthawat/Developments/solar-cons/construction-dotnet-rest-api
   docker-compose up -d postgres
   ```

2. Run .NET API locally:
   ```bash
   dotnet run --urls "http://localhost:5001"
   ```

---

## Verification Checklist

| Service | URL | Expected |
|---------|-----|----------|
| API Test | http://localhost:5001/api/v1/projects/test | JSON with sample projects |
| Swagger Docs | http://localhost:5001/swagger | API documentation UI |
| Frontend | http://localhost:5173 | Login page |

---

## Default Login Credentials

- **Username**: `admin@example.com`
- **Password**: `Admin123!`

---

## Stopping Services

1. Stop Docker containers:
   ```bash
   cd /Users/chanthawat/Developments/solar-cons/construction-dotnet-rest-api
   docker-compose down
   ```

2. Stop with data cleanup (removes volumes):
   ```bash
   docker-compose down -v
   ```

---

## Troubleshooting

### Port 5001 already in use
```bash
lsof -i :5001 | grep LISTEN
kill -9 <PID>
```

### Port 5432 (PostgreSQL) already in use
```bash
lsof -i :5432 | grep LISTEN
# Stop local PostgreSQL or change docker-compose port
```

### Reset database
```bash
docker-compose down -v
docker-compose up -d
```

### View API logs
```bash
docker-compose logs -f api
```

### View database logs
```bash
docker-compose logs -f postgres
```
