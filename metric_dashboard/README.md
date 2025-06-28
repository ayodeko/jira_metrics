# Jira Metrics Dashboard

A modern React dashboard for visualizing Jira metrics and analytics.

## Features

- **Secure Login**: Enter your Jira credentials once and they're saved in your browser
- **Real-time Metrics**: View lead time, cycle time, throughput, work in progress, and reopen rates
- **Interactive Charts**: Time in status, trend analysis, cumulative flow, and sprint velocity charts
- **Issue Management**: Browse and search through Jira issues with filtering and sorting
- **Team Analytics**: View team performance metrics and individual contributor data

## Getting Started

### Prerequisites

- Node.js (v16 or higher)
- A Jira instance with API access
- Your Jira API token

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm run dev
```

3. Open your browser and navigate to `http://localhost:5173`

### Setting Up Jira API Token

1. Go to your [Atlassian account settings](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click "Create API token"
3. Give it a name (e.g., "Metrics Dashboard")
4. Copy the generated token

### First Login

1. Enter your Jira base URL (e.g., `https://your-domain.atlassian.net`)
2. Enter your email address
3. Enter your API token
4. Check "Remember my credentials" to save them in your browser
5. Click "Connect to Jira"

Your credentials will be securely stored in your browser's localStorage and automatically loaded on future visits.

### Logging Out

Click the logout button (red icon) in the top-right corner of the dashboard to clear your saved credentials and return to the login screen.

## API Endpoints

The dashboard communicates with a .NET backend server running on `http://localhost:5024`. Make sure the backend server is running for the dashboard to function properly.

## Development

### Project Structure

```
src/
├── components/          # Reusable UI components
│   ├── charts/         # Chart components
│   ├── LoginPage.tsx   # Login form
│   └── ...
├── contexts/           # React contexts
│   └── AuthContext.tsx # Authentication state management
├── pages/              # Page components
│   └── Dashboard.tsx   # Main dashboard
├── services/           # API services
│   └── metricsService.ts # Jira API integration
└── utils/              # Utility functions
```

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Run ESLint
- `npm run preview` - Preview production build

## Security Notes

- Credentials are stored in browser localStorage (not encrypted)
- API tokens are sent to the backend server for Jira API calls
- Consider using environment variables for production deployments
- Regularly rotate your API tokens for security

## Troubleshooting

### Connection Issues

- Verify your Jira base URL is correct
- Ensure your API token is valid and not expired
- Check that your Jira instance allows API access
- Verify the backend server is running on port 5024

### Data Not Loading

- Check browser console for error messages
- Verify your Jira project has issues with the required fields
- Ensure your API token has sufficient permissions to read issues

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request
