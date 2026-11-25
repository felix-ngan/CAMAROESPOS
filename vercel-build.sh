#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"

# Vercel minimal Linux containers often omit libicu; enable globalization invariant fallback when missing
if [ "$(uname -s)" = "Linux" ]; then
  if ! ldconfig -p 2>/dev/null | grep -q libicu; then
    export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
  fi
fi

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

# Emit headers + rewrites vercel.json inside dist/wwwroot for direct static CLI deploys
cat > dist/wwwroot/vercel.json <<'EOF'
{
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        { "key": "X-Frame-Options", "value": "DENY" },
        { "key": "X-Content-Type-Options", "value": "nosniff" },
        { "key": "Referrer-Policy", "value": "strict-origin-when-cross-origin" },
        { "key": "Permissions-Policy", "value": "camera=(), microphone=(), geolocation=()" },
        { "key": "Content-Security-Policy", "value": "default-src 'self'; script-src 'self' 'unsafe-inline' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'; base-uri 'self';" }
      ]
    },
    {
      "source": "/_framework/(.*)",
      "headers": [
        { "key": "Cache-Control", "value": "public, max-age=31536000, immutable" }
      ]
    }
  ],
  "rewrites": [
    {
      "source": "/favicon.ico",
      "destination": "/favicon.svg"
    },
    {
      "source": "/((?!_framework/|css/|js/|.*\\..*).*)",
      "destination": "/index.html"
    }
  ]
}
EOF

echo "Blazor WebAssembly PWA published cleanly to dist/wwwroot"
