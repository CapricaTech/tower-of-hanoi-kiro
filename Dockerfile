# Tower of Hanoi - container image.
#
# The .NET Framework 4.8 is Windows-only, so on macOS/Linux the game runs on
# Mono. This image is a Linux + Mono image; Windows containers are intentionally
# not used because they cannot run on macOS.
#
# Multi-stage build: the first stage compiles the game with Mono's C# compiler
# (mcs) targeting the 4.8 profile; the runtime stage carries only the Mono
# runtime and the produced executable.

# ---- Build stage ---------------------------------------------------------
FROM mono:6.12 AS build
WORKDIR /src

# Copy sources needed to compile.
COPY src/ ./src/

# Compile all C# sources into a single net48 executable.
RUN mkdir -p /app \
    && mcs -sdk:4.8 -target:exe -optimize+ -define:TRACE \
        -langversion:7.2 \
        -out:/app/TowerOfHanoi.exe \
        $(find src/TowerOfHanoi -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*')

# ---- Runtime stage -------------------------------------------------------
FROM mono:6.12 AS runtime
WORKDIR /app

COPY --from=build /app/TowerOfHanoi.exe ./TowerOfHanoi.exe

# The game is an interactive terminal application; run it with `docker run -it`.
ENTRYPOINT ["mono", "TowerOfHanoi.exe"]
