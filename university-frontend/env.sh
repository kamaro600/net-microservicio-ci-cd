#!/bin/sh

# Debug: Print environment variables
echo "=== DEBUG: Environment Variables ==="
echo "API_URL: ${API_URL}"
echo "AUTH_URL: ${AUTH_URL}"
echo "===================================="

# Ensure assets directory exists
mkdir -p /usr/share/nginx/html/assets

# Create env-config.js with environment variables
cat <<EOF > /usr/share/nginx/html/assets/env-config.js
window.__env = {
  apiUrl: '${API_URL:-http://localhost:5000/api}',
  authUrl: '${AUTH_URL:-http://localhost:5063/api}'
};
EOF

echo "Environment configuration created:"
cat /usr/share/nginx/html/assets/env-config.js
