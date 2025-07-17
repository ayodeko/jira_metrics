#!/bin/bash

# Function to handle shutdown gracefully
cleanup() {
    echo "Shutting down services..."
    kill $NGINX_PID $DOTNET_PID 2>/dev/null
    wait $NGINX_PID $DOTNET_PID 2>/dev/null
    exit 0
}

# Set up signal handlers
trap cleanup SIGTERM SIGINT

# Start .NET server in background
echo "Starting .NET API server..."
cd /app/server
dotnet MetricsServer.dll --urls "http://localhost:5024" &
DOTNET_PID=$!

# Wait a moment for the server to start
sleep 5

# Start nginx
echo "Starting nginx..."
nginx -g "daemon off;" &
NGINX_PID=$!

# Wait for both processes
wait $NGINX_PID $DOTNET_PID