# CloudGameShare
This project demonstrates how a single physical GPU can be shared among multiple users by running GPU-accelerated games in isolated environments, enabling efficient resource utilization and scalable cloud gaming experiences.

## Run Gaming Sessions locally

Start a local MongoDB instance with a persistent volume. This example publishes MongoDB on loopback for local development.

```powershell
docker run --rm -p 127.0.0.1:27017:27017 -v cloudgameshare-sessions:/data/db mongo:8.0
```

Start Catalog with its HTTPS profile so the Session service can call it over HTTP/2. Trust the .NET development certificate if needed.

```powershell
dotnet dev-certs https --trust
dotnet run --project src/Services/CatalogService/CloudGameShare.Catalog.Presentation.WebApi --launch-profile https
```

In a separate terminal, select the Kubernetes context and namespace for provisioning and set the runtime image. Use `Provisioning__KubeConfigPath` if the context is in a separate kubeconfig. Inside Kubernetes the worker uses its service account, which needs permission to get, create, and delete pods in the configured namespace.

```powershell
$env:Provisioning__KubeContext = '<local-gpu-cluster-context>'
$env:Provisioning__Namespace = '<gaming-namespace>'
$env:Provisioning__Image = '<registry>/<game-runtime-image>:<tag>'
$env:Authentication__DevelopmentUserId = 'local-player'
dotnet run --project src/Services/GamingSessionService/CloudGameShare.GamingSession.Presentation.WebApi --launch-profile http
```

The optional development identity only applies in the Development environment. Configure `Authentication__Authority` and `Authentication__Audience` to use JWT bearer authentication. Override `ConnectionStrings__GamingSessions`, `MongoDBSettings__DatabaseName`, and `Catalog__GrpcAddress` for other environments.

Open Gaming Sessions Swagger at [localhost:5003/swagger](http://localhost:5003/swagger). Send `POST /api/gaming/sessions` with the ID of a published Steam game as `{"gameId":"<guid>"}`. Follow the `Location` header on the `202 Accepted` response to poll the session. Reuse the same optional GUID `Idempotency-Key` header when retrying a Play request.

The runtime image must start the game and streaming process from `GAMING_SESSION_ID`, `STEAM_APP_ID`, `GAME_RUNTIME`, and optional `PROTON_VERSION`. Its readiness endpoint defaults to `GET /ready` on port `8080` and should return success only when the game and streaming process are ready. The cluster must expose the configured GPU resource, which defaults to `nvidia.com/gpu`.
