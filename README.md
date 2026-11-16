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

In a separate terminal, enable provisioning and select the Kubernetes context and namespace. The worker uses `cloudgameshare/steam-selkies:dev` by default. Build the image below and make it available to the GPU nodes. Set `Provisioning__Image` to a registry image when the nodes pull from a registry. Use `Provisioning__KubeConfigPath` if the context is in a separate kubeconfig. The worker needs permission to get, create, and delete pods in the configured namespace.

```powershell
$env:Provisioning__Enabled = 'true'
$env:Provisioning__KubeContext = '<local-gpu-cluster-context>'
$env:Provisioning__Namespace = '<gaming-namespace>'
$env:Provisioning__AllowUnconfinedSeccomp = 'true'
$env:Authentication__DevelopmentUserId = 'local-player'
dotnet run --project src/Services/GamingSessionService/CloudGameShare.GamingSession.Presentation.WebApi --launch-profile http
```

The optional development identity only applies in the Development environment. Configure `Authentication__Authority` and `Authentication__Audience` to use JWT bearer authentication. Override `ConnectionStrings__GamingSessions`, `MongoDBSettings__DatabaseName`, and `Catalog__GrpcAddress` for other environments.

Open Gaming Sessions Swagger at [localhost:5003/swagger](http://localhost:5003/swagger). Send `POST /api/gaming/sessions` with the ID of a published Steam game as `{"gameId":"<guid>"}`. Follow the `Location` header on the `202 Accepted` response to poll the session. Reuse the same optional GUID `Idempotency-Key` header when retrying a Play request.

The worker sends `GAMING_SESSION_ID`, `STEAM_APP_ID`, `GAME_RUNTIME`, and optional `PROTON_VERSION` to the runtime. Selkies listens on `8080`. The worker checks `GET /ready` on `8081` until both the game window and Selkies are up. The cluster must expose the GPU resource, which defaults to `nvidia.com/gpu`. Use `AllowUnconfinedSeccomp` for the local test. On cluster nodes with a Steam-compatible seccomp profile, set `Provisioning__SeccompLocalhostProfile` to its path relative to the node's seccomp directory and leave `AllowUnconfinedSeccomp` disabled.

Set `Provisioning__PlayerStorageGiB` to keep a Steam home for each player and game. Size it for the game installation, Steam, and Proton. Use `Provisioning__PlayerStorageClass` to select a CSI StorageClass that supports `ReadWriteOncePod` or omit it to use a compatible default. The worker needs permission to get and create PVCs. It reuses those claims across sessions and keeps them after pod cleanup. Without player storage, Steam's installation and settings last for the pod's lifetime.

Optionally set `Provisioning__GameLibraryClaim` to a prepopulated PVC in the same namespace with the `steamapps` layout shown below. The runtime mounts it read-only and imports the selected game into its writable Steam home. Steam can install the game directly when no seed is present.

For a first launch, port-forward the session pod's Selkies port while Steam is setting up. Convert the returned session ID to the pod name by removing the dashes.

```powershell
$pod = "game-$($sessionId -replace '-', '')"
kubectl --context $env:Provisioning__KubeContext -n $env:Provisioning__Namespace port-forward "pod/$pod" 18080:8080
```

Sign into [Selkies](https://localhost:18080) as `ubuntu`. The provisioner generates a password per pod. To use a known password, create a Secret in the same namespace with a `password` key and set `Provisioning__SelkiesPasswordSecret` to its name. For a pod with a generated password, an operator can retrieve it with `kubectl exec "$pod" -- printenv SELKIES_BASIC_AUTH_PASSWORD` in the same context and namespace.

## Build the Steam and Selkies runtime

From the repository root, build the reusable Linux x86-64 image. It uses the Selkies desktop display server and installs Steam through Debian.

```powershell
docker build --platform linux/amd64 -t cloudgameshare/steam-selkies:dev runtime/steam-selkies
```

For a local Stardew Valley test, the runtime can seed its installation from `D:\SteamLibrary\steamapps\appmanifest_413150.acf` and `D:\SteamLibrary\steamapps\common\Stardew Valley`. Keep the source mount read-only. The named home volume retains the imported game after Steam verifies or updates it, along with the Steam client and remembered login. `GAME_STATE_DIR` keeps any Proton prefix in that same volume.

```powershell
$password = Read-Host 'Choose a Selkies password' -MaskInput
docker run --rm --init --gpus 1 --shm-size 2g --security-opt seccomp=unconfined -p 127.0.0.1:18080:8080 -p 127.0.0.1:18081:8081 -e STEAM_APP_ID=413150 -e GAME_RUNTIME=linux-native -e GAME_STATE_DIR=/home/ubuntu/.game-state -e "SELKIES_BASIC_AUTH_PASSWORD=$password" --mount 'type=bind,source=D:\SteamLibrary,target=/steam-library,readonly' --mount 'type=volume,source=cloudgameshare-steam-home,target=/home/ubuntu' cloudgameshare/steam-selkies:dev
```

Docker Desktop's default seccomp profile blocks the Linux user namespaces Steam needs. The command uses `seccomp=unconfined` for this local container. For a cluster deployment, configure a seccomp profile that permits Steam's namespace operations.

Open [the local Selkies desktop](https://localhost:18080), sign into Steam with **Remember me**, and start the game from Steam if first-run setup interrupted the automatic launch. For a Windows title, choose Proton in Steam's Compatibility settings and use `GAME_RUNTIME=proton`. `PROTON_VERSION` records the requested version in the launcher log. Steam controls tool selection and may verify or download files to complete the installation.
