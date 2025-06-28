import React, { useState, useEffect } from 'react';
import { Box, Flex, Stack, useDisclosure, Button } from '@chakra-ui/react';
import { Header } from '../components/Header';
import { KPICard } from '../components/KPICard';
import { AnalysisTabs } from '../components/AnalysisTabs';
import { TimeInStatusChart } from '../components/charts/TimeInStatusChart';
import { TrendChart } from '../components/charts/TrendChart';
import { CumulativeFlowChart } from '../components/charts/CumulativeFlowChart';
import { SprintVelocityChart } from '../components/charts/SprintVelocityChart';
import { IssueTable } from '../components/IssueTable';
import { SideDrawer } from '../components/SideDrawer';
import { metricsService } from '../services/metricsService';
import { RepeatIcon } from '@chakra-ui/icons';
import type { TimeInStatusData, TrendData, CumulativeFlowData, TeamMember } from '../services/metricsService';
import { TeamTable } from '../components/TeamTable';
import { useAuth } from '../contexts/AuthContext';

export const Dashboard: React.FC = () => {
  const { credentials, logout } = useAuth();
  
  const [selectedProject, setSelectedProject] = useState('');
  const [selectedScope, setSelectedScope] = useState<'Issue' | 'Team' | 'Epic' | 'Sprint' | 'Project'>('Issue');
  const [metrics, setMetrics] = useState({
    leadTime: 0,
    cycleTime: 0,
    throughput: 0,
    wip: 0,
    reopenRate: 0
  });
  const [metricsLoading, setMetricsLoading] = useState(false);
  const [metricsError, setMetricsError] = useState<string | null>(null);

  const [issues, setIssues] = useState([]);
  const [issuesLoading, setIssuesLoading] = useState(false);
  const [issuesError, setIssuesError] = useState<string | null>(null);

  const [timeInStatusData, setTimeInStatusData] = useState<TimeInStatusData>();
  const [timeInStatusLoading, setTimeInStatusLoading] = useState(false);
  const [timeInStatusError, setTimeInStatusError] = useState<string | null>(null);

  const [trendData, setTrendData] = useState<TrendData>();
  const [trendLoading, setTrendLoading] = useState(false);
  const [trendError, setTrendError] = useState<string | null>(null);

  const [cumulativeFlowData, setCumulativeFlowData] = useState<CumulativeFlowData | undefined>(undefined);
  const [cumulativeFlowLoading, setCumulativeFlowLoading] = useState(false);
  const [cumulativeFlowError, setCumulativeFlowError] = useState<string | null>(null);

  // For side drawer
  const disclosure = useDisclosure();
  const [selectedMetric, setSelectedMetric] = useState<{
    title: string;
    type: 'leadTime' | 'cycleTime' | 'throughput' | 'wip' | 'reopenRate';
  }>({
    title: '',
    type: 'leadTime'
  });

  const [tabIndex, setTabIndex] = useState(0);
  const [teamMembers, setTeamMembers] = useState<TeamMember[]>([]);
  const [teamLoading, setTeamLoading] = useState(false);
  const [teamError, setTeamError] = useState<string | null>(null);

  const [issuesPage, setIssuesPage] = useState(1);
  const [issuesPageSize] = useState(20);
  const [issuesTotal, setIssuesTotal] = useState(0);

  const fetchMetrics = async () => {
    if (!credentials) return;
    
    setMetricsLoading(true);
    setMetricsError(null);
    try {
      const [leadTime, cycleTime, throughput, wip, reopenRate] = await Promise.all([
        metricsService.getLeadTime(credentials),
        metricsService.getCycleTime(credentials),
        metricsService.getThroughput(credentials),
        metricsService.getWorkInProgress(credentials),
        metricsService.getReopenRate(credentials),
      ]);
      setMetrics({ leadTime, cycleTime, throughput, wip, reopenRate });
    } catch (err) {
      setMetricsError('Failed to load metrics');
    } finally {
      setMetricsLoading(false);
    }
  };

  const fetchIssues = async (page = 1) => {
    if (!credentials) return;
    
    setIssuesLoading(true);
    setIssuesError(null);
    try {
      const issuesData = await metricsService.getIssues(credentials, page, issuesPageSize);
      setIssues(issuesData.issues ?? []);
      setIssuesTotal(issuesData.total ?? 0);
    } catch (err) {
      setIssuesError('Failed to load issues');
    } finally {
      setIssuesLoading(false);
    }
  };

  const fetchTimeInStatus = async () => {
    if (!credentials) return;
    
    setTimeInStatusLoading(true);
    setTimeInStatusError(null);
    try {
      const data = await metricsService.getTimeInStatus(credentials);
      setTimeInStatusData(data);
    } catch (err) {
      setTimeInStatusError('Failed to load time in status data');
    } finally {
      setTimeInStatusLoading(false);
    }
  };

  const fetchTrend = async () => {
    if (!credentials) return;
    
    setTrendLoading(true);
    setTrendError(null);
    try {
      const data = await metricsService.getTrend(credentials);
      setTrendData(data);
    } catch (err) {
      setTrendError('Failed to load trend data');
    } finally {
      setTrendLoading(false);
    }
  };

  const fetchCumulativeFlow = async () => {
    if (!credentials) return;
    
    setCumulativeFlowLoading(true);
    setCumulativeFlowError(null);
    try {
      const data = await metricsService.getCumulativeFlow(credentials);
      setCumulativeFlowData(data);
    } catch (err) {
      setCumulativeFlowError('Failed to load cumulative flow data');
    } finally {
      setCumulativeFlowLoading(false);
    }
  };

  const fetchTeamMetrics = async () => {
    if (!credentials) return;
    
    setTeamLoading(true);
    setTeamError(null);
    try {
      const data = await metricsService.getTeamMetrics(credentials);
      setTeamMembers(data);
    } catch (err) {
      setTeamError('Failed to load team metrics');
    } finally {
      setTeamLoading(false);
    }
  };

  const fetchAll = async () => {
    if (!credentials) return;
    
    await Promise.all([
      fetchMetrics(),
      fetchIssues(),
      fetchTimeInStatus(),
      fetchTrend(),
      fetchCumulativeFlow(),
      fetchTeamMetrics(),
    ]);
  };

  const handleKPICardClick = (title: string, type: 'leadTime' | 'cycleTime' | 'throughput' | 'wip' | 'reopenRate') => {
    setSelectedMetric({
      title,
      type
    });
    disclosure.onOpen();
  };

  const bgColor = '#f7fafc'; // light mode fallback
  const borderColor = '#e2e8f0'; // light mode fallback

  // Fetch team metrics when Team tab is selected
  useEffect(() => {
    if (tabIndex === 1) {
      fetchTeamMetrics();
    }
    if (tabIndex === 2) {
      fetchTrend();
    }
  }, [tabIndex]);

  useEffect(() => {
    if (credentials) {
      fetchIssues(issuesPage);
    }
  }, [issuesPage, credentials]);

  // Load data when credentials are available
  useEffect(() => {
    if (credentials) {
      fetchAll();
    }
  }, [credentials]);

  return (
    <Box bg={bgColor} minH="100vh" width="100%">
      <Header 
        selectedProject={selectedProject} 
        selectedScope={selectedScope} 
        onProjectChange={setSelectedProject} 
        onScopeChange={scope => {
          setSelectedScope(scope);
          // Sync tabIndex with scope
          const tabMap = ['Time in Status', 'Team', 'Trend', 'Cumulative Flow', 'Sprint Velocity'];
          const idx = scope === 'Team' ? 1 : scope === 'Issue' ? 0 : tabIndex;
          setTabIndex(idx);
        }} 
        onLogout={logout}
      />
      <Flex p={6} gap={6} flexDir={{ base: 'column', lg: 'row' }} maxW="1440px" mx="auto">
        {/* Left Rail - KPI Cards */}
        <Stack gap={4} width={{ base: '100%', lg: '280px' }} flexShrink={0}>
          <Flex justify="space-between" align="center">
          <Box fontWeight="medium" fontSize="lg">
            Key Metrics
          </Box>
            <Button
              size="sm"
              onClick={async () => {
                if (credentials) {
                  await metricsService.refresh(credentials);
                  fetchAll();
                }
              }}
            >
              Refresh
            </Button>
          </Flex>
          <KPICard 
            title="Lead Time" 
            value={metrics.leadTime} 
            change={5.3} 
            colorScheme="blue" 
            onClick={() => handleKPICardClick('Lead Time', 'leadTime')} 
            status={metricsLoading ? 'loading' : metricsError ? 'error' : 'default'}
          />
          <KPICard 
            title="Cycle Time" 
            value={metrics.cycleTime} 
            change={-3.1} 
            colorScheme="purple" 
            onClick={() => handleKPICardClick('Cycle Time', 'cycleTime')} 
            status={metricsLoading ? 'loading' : metricsError ? 'error' : 'default'}
          />
          <KPICard 
            title="Throughput" 
            value={metrics.throughput} 
            change={12.5} 
            colorScheme="green" 
            onClick={() => handleKPICardClick('Throughput', 'throughput')} 
            status={metricsLoading ? 'loading' : metricsError ? 'error' : 'default'}
          />
          <KPICard 
            title="Work in Progress" 
            value={metrics.wip} 
            change={8.2} 
            colorScheme="orange" 
            onClick={() => handleKPICardClick('Work in Progress', 'wip')} 
            status={metricsLoading ? 'loading' : metricsError ? 'error' : 'default'}
          />
          <KPICard 
            title="Reopen Rate" 
            value={`${metrics.reopenRate}%`} 
            change={-2.1} 
            colorScheme="red" 
            onClick={() => handleKPICardClick('Reopen Rate', 'reopenRate')} 
            status={metricsLoading ? 'loading' : metricsError ? 'error' : 'default'}
          />
          {metricsError && <Box color="red.500" fontSize="sm">{metricsError}</Box>}
        </Stack>

        {/* Right Panel - Analysis Tabs & Issue Table */}
        <Flex 
          flex={1} 
          flexDir="column" 
          bg={bgColor} 
          borderRadius="md" 
          border="1px solid" 
          borderColor={borderColor} 
          overflow="hidden"
        >
          <Box p={4}>
            <AnalysisTabs
              tabLabels={['Time in Status', 'Team', 'Trend', 'Cumulative Flow', 'Sprint Velocity']}
              index={tabIndex}
              onChange={setTabIndex}
            >
              <Box>
                <TimeInStatusChart data={timeInStatusData} isLoading={timeInStatusLoading} isError={!!timeInStatusError} />
              </Box>
              <Box>
                <TeamTable teamMembers={teamMembers.map(m => ({ ...m, name: m.name || m.displayName || '' }))} isLoading={teamLoading} isError={!!teamError} />
              </Box>
              <Box>
                <TrendChart data={trendData} isLoading={trendLoading} isError={!!trendError} />
              </Box>
              <Box>
                <CumulativeFlowChart data={cumulativeFlowData} isLoading={cumulativeFlowLoading} isError={!!cumulativeFlowError} />
              </Box>
              <Box>
                <SprintVelocityChart />
              </Box>
            </AnalysisTabs>
            {timeInStatusError && <Box color="red.500" fontSize="sm">{timeInStatusError}</Box>}
            {trendError && <Box color="red.500" fontSize="sm">{trendError}</Box>}
            {cumulativeFlowError && <Box color="red.500" fontSize="sm">{cumulativeFlowError}</Box>}
          </Box>
          <Box borderTop="1px solid" borderColor={borderColor} p={4} flexGrow={1}>
            {tabIndex === 0 && (
              <IssueTable
                issues={issues}
                isLoading={issuesLoading}
                isError={!!issuesError}
              />
            )}
            {issuesError && <Box color="red.500" fontSize="sm">{issuesError}</Box>}
          </Box>
        </Flex>
      </Flex>
      <SideDrawer 
        isOpen={disclosure.isOpen} 
        onClose={disclosure.onClose} 
        title={selectedMetric.title} 
        metricType={selectedMetric.type} 
      />
    </Box>
  );
};