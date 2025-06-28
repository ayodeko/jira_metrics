import axios from 'axios';
import { JiraCredentials } from '../components/LoginPage';

const API_BASE_URL = 'http://localhost:5024/api/metrics';

export interface TimeInStatusData {
  statusDurations: {
    status: string;
    averageDays: number;
  }[];
}

export interface MetricTrend {
  date: string;
  value: number;
}

export interface TrendData {
  leadTime: MetricTrend[];
  cycleTime: MetricTrend[];
  throughput: MetricTrend[];
}

export interface FlowPoint {
  date: string;
  statusCounts: { [key: string]: number };
}

export interface CumulativeFlowData {
  dataPoints: FlowPoint[];
}

export interface TeamMember {
  accountId: string;
  name: string;
  displayName?: string;
  email: string;
  avatarUrl: string;
  issuesAssigned: number;
  issuesCompleted: number;
  averageLeadTime: number;
  averageCycleTime: number;
  throughput: number;
}

// Create axios instance with interceptors
const createApiClient = (credentials: JiraCredentials) => {
  const client = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  // Add credentials to every request
  client.interceptors.request.use((config) => {
    config.data = {
      ...config.data,
      credentials,
    };
    return config;
  });

  return client;
};

export const metricsService = {
  // KPI Metrics
  async getLeadTime(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post('/leadtime', { credentials });
    return response.data.leadTime;
  },

  async getCycleTime(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post('/cycletime', { credentials });
    return response.data.cycleTime;
  },

  async getThroughput(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post('/throughput', { credentials });
    return response.data.throughput;
  },

  async getWorkInProgress(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post('/wip', { credentials });
    return response.data.workInProgress;
  },

  async getReopenRate(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post('/reopenrate', { credentials });
    return response.data.reopenRate;
  },

  // Chart Data
  async getTimeInStatus(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post<TimeInStatusData>('/timeinstatus', { credentials });
    return response.data;
  },

  async getTrend(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post<TrendData>('/trend', { credentials });
    return response.data;
  },

  async getCumulativeFlow(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post<CumulativeFlowData>('/cumulativeflow', { credentials });
    return response.data;
  },

  // Issues
  async getIssues(credentials: JiraCredentials, page = 1, pageSize = 20) {
    const client = createApiClient(credentials);
    const response = await client.post('/issuetable', { 
      credentials,
      page, 
      pageSize 
    });
    return response.data;
  },

  async refresh(credentials: JiraCredentials) {
    const client = createApiClient(credentials);
    const response = await client.post('/refresh', { credentials });
    if (response.status !== 200) throw new Error('Failed to refresh metrics');
    return response.data;
  },

  async getTeamMetrics(credentials: JiraCredentials): Promise<TeamMember[]> {
    try {
      const client = createApiClient(credentials);
      const response = await client.post<TeamMember[]>('/teamtable', { credentials });
      return response.data;
    } catch (error) {
      console.error('Error fetching team metrics:', error);
      throw error;
    }
  },

  // Test connection
  async testConnection(credentials: JiraCredentials): Promise<boolean> {
    try {
      const client = createApiClient(credentials);
      const response = await client.post('/test-connection', { credentials });
      return response.status === 200;
    } catch (error) {
      console.error('Connection test failed:', error);
      return false;
    }
  }
}; 