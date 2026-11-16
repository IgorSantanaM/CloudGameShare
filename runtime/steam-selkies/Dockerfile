# syntax=docker/dockerfile:1
FROM ghcr.io/selkies-project/selkies/desktop:2.1.0-debiantrixie@sha256:dd26d133435a051250f888f8d528438974696cc59047872f9cd71975fd97bb1e

USER 0

RUN test "$(dpkg --print-architecture)" = amd64 \
    && dpkg --add-architecture i386 \
    && sed -i '/^Components:.*\bcontrib\b/!s/^Components:.*/& contrib/' \
        /etc/apt/sources.list.d/debian.sources \
    && apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        ca-certificates curl python3 steam-installer wmctrl \
    && rm -rf /var/lib/apt/lists/* \
    && install -d -o 1000 -g 1000 /game-state /steam-library /etc/service/cloudgameshare

RUN install -d -m 0755 /usr/local/lib/cloudgameshare

COPY --chmod=644 session-runtime.py \
    /usr/local/lib/cloudgameshare/session-runtime.py

COPY --chown=1000:1000 --chmod=755 run \
    /etc/service/cloudgameshare/run

ENV SELKIES_WAYLAND=false \
    NVIDIA_DRIVER_CAPABILITIES=compute,utility,graphics,video,display,compat32 \
    SELKIES_COMMAND_ENABLED=false

EXPOSE 8080 8081

USER 1000:1000
