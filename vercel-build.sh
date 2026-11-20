#!/usr/bin/env bash
set -euo pipefail

# Ensure .NET 8 SDK is available (works both locally and inside Vercel Linux build containers)
if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-sdks 2>/dev/null | grep -q '^8\.0\.'; then
  echo "Installing .NET 8 SDK..."
  curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
fi

echo "Using dotnet: $(dotnet --version)"
rm -rf dist
dotnet publish CamaroesPOS.Web/CamaroesPOS.Web.csproj -c Release -o dist

# Emit rewrites-only vercel.json inside dist/wwwroot for direct static CLI deploys
cat > dist/wwwroot/vercel.json <<'EOF'
{
  "rewrites": [
    {
      "source": "/((?!_framework/|css/|js/|.*\\..*).*)",
      "destination": "/index.html"
    }
  ]
}
EOF

echo "Blazor WebAssembly published cleanly to dist/wwwroot"
