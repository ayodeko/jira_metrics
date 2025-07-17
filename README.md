# Jira Metrics Dashboard

A comprehensive dashboard for tracking Jira metrics including lead time, cycle time, throughput, and team performance. The application consists of a React frontend and a .NET Core API backend, both containerized for easy deployment.

## Features

- **KPI Dashboard**: Lead time, cycle time, throughput, work in progress, and reopen rate
- **Interactive Charts**: Time in status, trend analysis, and cumulative flow diagrams
- **Team Performance**: Individual team member metrics and performance tracking
- **Issue Management**: Detailed issue table with filtering and pagination
- **Real-time Data**: Refresh capabilities for up-to-date metrics

## Architecture

The application is containerized using a multi-stage Docker build:

- **Frontend**: React + Vite + Chakra UI
- **Backend**: .NET 9.0 Web API
- **Reverse Proxy**: Nginx serving the frontend and proxying API requests
- **Container**: Single container with both services running internally

## Quick Start

### Using Docker Compose (Recommended)

1. **Build and run the application:**
   ```bash
   docker-compose up --build
   ```

2. **Access the dashboard:**
   - Open your browser and navigate to `http://localhost:3000`
   - The dashboard will be served by nginx
   - API requests are automatically proxied to the internal .NET server

### Using Docker Directly

1. **Build the image:**
   ```bash
   docker build -t jira-metrics .
   ```

2. **Run the container:**
   ```bash
   docker run -p 3000:80 jira-metrics
   ```

3. **Access the dashboard:**
   - Navigate to `http://localhost:3000`

## Development

### Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for local development)
- Node.js 18+ (for local development)

### Local Development

1. **Start the backend:**
   ```bash
   cd MetricsServer
   dotnet run
   ```

2. **Start the frontend:**
   ```bash
   cd metric_dashboard
   npm install
   npm run dev
   ```

3. **Access the applications:**
   - Frontend: `http://localhost:5173`
   - Backend: `http://localhost:5024`

## Container Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Container                         │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │   Nginx (Port 80)│    │ .NET API (Port 5024) │                │
│  │                 │    │                 │                │
│  │ • Serves React  │◄──►│ • Jira API      │                │
│  │ • Proxies /api/ │    │ • Metrics Logic │                │
│  │ • Static Assets │    │ • Data Processing│                │
│  └─────────────────┘    └─────────────────┘                │
└─────────────────────────────────────────────────────────────┘
```

### Internal Communication

- **Nginx** serves the React dashboard on the root path (`/`)
- **API requests** to `/api/*` are proxied to the internal .NET server
- **Health checks** are available at `/health`
- **Single external port** (80) exposed for all traffic

## Configuration

### Environment Variables

The application can be configured using environment variables:

```bash
# .NET API Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5024

# Jira Configuration (set in the dashboard UI)
JIRA_URL=your-jira-instance.atlassian.net
JIRA_EMAIL=your-email@company.com
JIRA_API_TOKEN=your-api-token
```

### Port Configuration

- **External Port**: 3000 (mapped to container port 80)
- **Internal API Port**: 5024 (not exposed externally)
- **Health Check**: Available at `http://localhost:3000/health`

## Deployment

### Production Deployment

1. **Build the production image:**
   ```bash
   docker build -t jira-metrics:latest .
   ```

2. **Run with production settings:**
   ```bash
   docker run -d \
     --name jira-metrics \
     -p 80:80 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     --restart unless-stopped \
     jira-metrics:latest
   ```

### Using Traefik (Optional)

For production deployments with SSL termination and load balancing:

```bash
docker-compose --profile traefik up -d
```

This will start:
- Jira Metrics application on port 8080
- Traefik reverse proxy on ports 80/443
- Traefik dashboard on port 8081

## Health Monitoring

The container includes health checks:

```bash
# Check container health
docker ps

# View health check logs
docker logs jira-metrics

# Manual health check
curl http://localhost:3000/health
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure port 8080 is available
2. **Build failures**: Check that all source files are present
3. **API connection issues**: Verify the API proxy configuration in nginx.conf

### Logs

```bash
# View container logs
docker logs jira-metrics

# View nginx logs
docker exec jira-metrics tail -f /var/log/nginx/access.log
docker exec jira-metrics tail -f /var/log/nginx/error.log

# View .NET logs
docker exec jira-metrics dotnet --info
```

## Security Considerations

- The container runs as a non-root user where possible
- API endpoints are proxied through nginx with proper headers
- CORS is configured for the containerized environment
- Health checks help monitor application status

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with Docker
5. Submit a pull request