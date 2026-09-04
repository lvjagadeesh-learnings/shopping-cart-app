# Containerization

Multi-stage builds keep runtime images small and avoid shipping build toolchains (SDKs, node_modules dev deps) to production.

## Frontend (React/SPA → nginx)

See [Dockerfile.frontend](../templates/Dockerfile.frontend). Key points:
- Stage 1 builds the static assets (`npm ci && npm run build`).
- Stage 2 copies only the build output into a slim `nginx:alpine` image.
- Add an `nginx.conf` that proxies `/api/*` to the backend service name (ECS Service Connect/Cloud Map DNS) if the frontend container talks to the backend directly, or let the ALB handle path routing instead (preferred — keeps the frontend container simple).
- Run nginx as a non-root user; expose port 8080 instead of 80 if enforcing non-root.

## Backend (.NET API)

See [Dockerfile.backend](../templates/Dockerfile.backend). Key points:
- Stage 1 uses the `mcr.microsoft.com/dotnet/sdk` image to `restore` and `publish` in Release mode.
- Stage 2 uses the matching `mcr.microsoft.com/dotnet/aspnet` runtime image (much smaller, no compiler).
- Set `ASPNETCORE_URLS=http://+:8080` (avoid binding to port 80 as a non-root container user) and expose 8080.
- Don't copy `appsettings.Development.json` or `.env` files into the image — inject config via ECS task definition environment/secrets instead.

## Validation Before Infrastructure Work

```bash
docker build -t app-frontend -f Dockerfile.frontend .
docker build -t app-backend -f Dockerfile.backend .
docker run --rm -p 8080:8080 app-backend   # confirm health endpoint responds
docker run --rm -p 3000:8080 app-frontend  # confirm static assets load
```

Only proceed to CDK provisioning once both images build and run cleanly.
