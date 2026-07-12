#!/usr/bin/env bash
# Runs the automated test suite (unit + integration) against the in-memory DB.
# No live server required — integration tests self-host via WebApplicationFactory.
set -euo pipefail

DOTNET="${DOTNET:-/usr/local/share/dotnet/dotnet}"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$ROOT"
exec "$DOTNET" test "$@"
