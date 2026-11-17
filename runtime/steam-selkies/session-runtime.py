import os
import re
import shutil
import signal
import subprocess
import tempfile
import time
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path


def prepare_steam_client(installation):
    copying = installation.parent / ".cloudgameshare-steam-copy"
    if (installation / "steam.sh").is_file() and not copying.exists():
        return

    print("Copying the Steam client from the runtime image.")
    installation.mkdir(parents=True, exist_ok=True)
    copying.touch()
    subprocess.run(
        ("cp", "-a", "--reflink=auto", "/opt/cloudgameshare/steam-client/.", installation),
        check=True,
    )
    copying.unlink()


def prepare_library(steamapps, app_id, runtime):
    steamapps.mkdir(parents=True, exist_ok=True)

    if runtime == "proton":
        state = Path(os.environ.get("GAME_STATE_DIR", "/game-state"))
        prefix = state / "compatdata" / app_id
        prefix.mkdir(parents=True, exist_ok=True)
        compatdata = steamapps / "compatdata"
        compatdata.mkdir(exist_ok=True)
        destination = compatdata / app_id
        if not destination.exists() and not destination.is_symlink():
            destination.symlink_to(prefix, target_is_directory=True)

    source = Path("/steam-library/steamapps")
    manifest_name = f"appmanifest_{app_id}.acf"
    manifest = source / manifest_name
    local_manifest = steamapps / manifest_name
    if not manifest.is_file():
        return

    match = re.search(r'^\s*"installdir"\s+"([^"\\/]+)"', manifest.read_text(encoding="utf-8-sig"), re.MULTILINE)
    if not match or match[1] in (".", ".."):
        raise ValueError(f"Invalid installation directory in {manifest_name}.")

    common = steamapps / "common"
    common.mkdir(exist_ok=True)
    destination = common / match[1]
    game_dir = source / "common" / match[1]
    if destination.is_symlink():
        if destination.resolve() != game_dir.resolve():
            raise ValueError(f"Unexpected Steam library link for {manifest_name}.")
    elif local_manifest.exists():
        return

    if not destination.exists() or destination.is_symlink():
        if not game_dir.is_dir():
            raise ValueError(f"Game directory is missing for {manifest_name}.")

        print(f"Preparing a writable Steam installation for App ID {app_id}.")
        with tempfile.TemporaryDirectory(prefix=f".import-{app_id}-", dir=common) as work:
            staged = Path(work) / "game"
            shutil.copytree(game_dir, staged, symlinks=True)
            staged.chmod(staged.stat().st_mode | 0o200)
            for path in staged.rglob("*"):
                if not path.is_symlink():
                    path.chmod(path.stat().st_mode | 0o200)

            if destination.is_symlink():
                destination.unlink()
            staged.rename(destination)

    if not local_manifest.exists():
        with tempfile.TemporaryDirectory(prefix=f".manifest-{app_id}-", dir=steamapps) as work:
            staged = Path(work) / manifest_namel
            shutil.copyfile(manifest, staged)
            staged.rename(local_manifest)


def command_succeeds(*args):
    try:
        return subprocess.run(args, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=2).returncode == 0
    except subprocess.TimeoutExpired:
        return False


def game_window_exists(app_id):
    try:
        result = subprocess.run(("wmctrl", "-lp"), capture_output=True, text=True, timeout=2)
    except subprocess.TimeoutExpired:
        return False

    markers = {f"SteamAppId={app_id}".encode(), f"SteamGameId={app_id}".encode()}
    for window in result.stdout.splitlines():
        columns = window.split(maxsplit=4)
        if len(columns) < 3 or not columns[2].isdigit():
            continue

        try:
            process_env = Path(f"/proc/{columns[2]}/environ").read_bytes().split(b"\0")
        except OSError:
            continue
        if markers.intersection(process_env):
            return True
    return False


def selkies_responds():
    port = os.environ.get("SELKIES_PORT", "8080")
    prefix = os.environ.get("SELKIES_SUBFOLDER", "").strip("/")
    path = f"/{prefix}/api/health" if prefix else "/api/health"
    scheme = "http" if os.environ.get("SELKIES_ENABLE_HTTPS", "true").lower() == "false" else "https"
    return command_succeeds("curl", "--insecure", "--fail", "--silent", "--max-time", "1",
                            f"{scheme}://127.0.0.1:{port}{path}")


def main():
    app_id = os.environ.get("STEAM_APP_ID", "")

    runtime = os.environ.get("GAME_RUNTIME", "")

    if not re.fullmatch(r"[0-9]{1,10}", app_id) or not 0 < int(app_id) <= 0xFFFFFFFF:
        raise ValueError("STEAM_APP_ID must be a positive 32-bit Steam App ID.")
    
    app_id = str(int(app_id))

    if runtime not in ("linux-native", "proton"):
        raise ValueError("GAME_RUNTIME must be linux-native or proton.")

    installation = Path.home() / ".steam/debian-installation"
    prepare_steam_client(installation)
    prepare_library(installation / "steamapps", app_id, runtime)
    
    if runtime == "proton" and (version := os.environ.get("PROTON_VERSION")):
        print(f"Requested Proton version: {version}. Select it in the Steam client for this game.")

    class ReadinessHandler(BaseHTTPRequestHandler):
        def do_GET(self):
            if self.path != "/ready":
                self.send_error(404)
                return
            ready = game_window_exists(app_id) and selkies_responds()
            self.send_response(200 if ready else 503)
            self.end_headers()

        def log_message(self, format, *args):
            pass

    child = None
    running = True

    def stop(_signal, _frame):
        nonlocal running
        running = False

    signal.signal(signal.SIGTERM, stop)
    signal.signal(signal.SIGINT, stop)
    with HTTPServer(("0.0.0.0", 8081), ReadinessHandler) as server:
        server.timeout = 0.5
        next_display_check = 0.0
        launched = False
        try:
            while running:
                if not launched and time.monotonic() >= next_display_check:
                    if command_succeeds("wmctrl", "-m"):
                        child = subprocess.Popen(("/usr/games/steam", "-applaunch", app_id))
                        launched = True
                    next_display_check = time.monotonic() + 1
                if child is not None and child.poll() is not None:
                    child = None
                server.handle_request()
        finally:
            if child is not None and child.poll() is None:
                child.terminate()


if __name__ == "__main__":
    main()
