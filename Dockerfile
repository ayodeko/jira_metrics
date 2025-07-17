# Multi-stage build for Jira Metrics Dashboard and API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS server-builder

# Set working directory for server
WORKDIR /app/server

# Copy server project files
COPY MetricsServer/*.csproj ./
RUN dotnet restore

# Copy server source code
COPY MetricsServer/ ./

# Build the server
RUN dotnet publish -c Release -o /app/server/publish

# Build stage for React dashboard
FROM node:18-alpine AS dashboard-builder

# Set working directory for dashboard
WORKDIR /app/dashboard

# Copy dashboard package files
COPY metric_dashboard/package*.json ./
RUN npm ci

# Copy dashboard source code
COPY metric_dashboard/ ./

# Build the dashboard
RUN npm run build

# Final stage - combine everything
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install nginx
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /app

# Copy server from builder
COPY --from=server-builder /app/server/publish ./server

# Copy dashboard from builder
COPY --from=dashboard-builder /app/dashboard/dist ./dashboard

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Copy startup script
COPY start.sh ./start.sh
RUN chmod +x ./start.sh

# Expose port 80 for nginx
EXPOSE 80

# Start both services
CMD ["bash", "./start.sh"] 