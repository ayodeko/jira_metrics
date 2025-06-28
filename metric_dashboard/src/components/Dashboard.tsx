import React, { useState, useEffect } from 'react';
import { Box, Grid, Button, useToast, useColorModeValue } from '@chakra-ui/react';
import { Header } from './Header';
import { AnalysisTabs } from './AnalysisTabs';
import { KPICard } from './KPICard';
import { IssueTable } from './IssueTable';
import { TeamTable } from './TeamTable';
import { TrendChart } from './charts/TrendChart';
import { TimeInStatusChart } from './charts/TimeInStatusChart';
import { CumulativeFlowChart } from './charts/CumulativeFlowChart';
import { metricsService, TeamMember } from '../services/metricsService';

const SCOPE_TAB_MAP = ['Issue', 'Team', 'Epic', 'Sprint'] as const;
type Scope = typeof SCOPE_TAB_MAP[number];

export const Dashboard: React.FC = () => {
  const [selectedProject, setSelectedProject] = useState('');
  const [selectedScope, setSelectedScope] = useState<Scope>('Issue');
  const [tabIndex, setTabIndex] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [isError, setIsError] = useState(false);
  const [teamMembers, setTeamMembers] = useState<TeamMember[]>([]);
  const toast = useToast();
  const bgColor = useColorModeValue('gray.50', 'gray.900');

  // Sync tabIndex <-> selectedScope
  useEffect(() => {
    const newIndex = SCOPE_TAB_MAP.indexOf(selectedScope);
    if (tabIndex !== newIndex) {
      setTabIndex(newIndex);
    }
  }, [selectedScope]);
  useEffect(() => {
    const newScope = SCOPE_TAB_MAP[tabIndex];
    if (selectedScope !== newScope) {
      setSelectedScope(newScope);
    }
  }, [tabIndex]);

  const handleRefresh = async () => {
    setIsLoading(true);
    setIsError(false);
    try {
      const teamData = await metricsService.getTeamMetrics();
      setTeamMembers(teamData);
      toast({
        title: 'Data refreshed',
        status: 'success',
        duration: 2000,
        isClosable: true
      });
    } catch (error) {
      setIsError(true);
      toast({
        title: 'Error refreshing data',
        description: 'Please try again later',
        status: 'error',
        duration: 5000,
        isClosable: true
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    handleRefresh();
  }, []);

  // Handler to ensure only valid scopes are set
  const handleScopeChange = (scope: string) => {
    if (SCOPE_TAB_MAP.includes(scope as Scope)) {
      setSelectedScope(scope as Scope);
    }
  };

  return (
    <Box minH="100vh" bg={bgColor}>
      <Header
        selectedProject={selectedProject}
        selectedScope={selectedScope}
        onProjectChange={setSelectedProject}
        onScopeChange={handleScopeChange}
      />
      <Box p={4}>
        <Grid templateColumns="repeat(4, 1fr)" gap={4} mb={4}>
          <KPICard
            title="Lead Time"
            value="3.2"
            unit="days"
            change={-0.5}
            status={isLoading ? 'loading' : isError ? 'error' : 'default'}
            colorScheme="blue"
          />
          <KPICard
            title="Cycle Time"
            value="2.1"
            unit="days"
            change={0.3}
            status={isLoading ? 'loading' : isError ? 'error' : 'default'}
            colorScheme="purple"
          />
          <KPICard
            title="Throughput"
            value="12"
            unit="issues/week"
            change={2}
            status={isLoading ? 'loading' : isError ? 'error' : 'default'}
            colorScheme="green"
          />
          <KPICard
            title="Work in Progress"
            value="8"
            unit="issues"
            change={-1}
            status={isLoading ? 'loading' : isError ? 'error' : 'default'}
            colorScheme="orange"
          />
        </Grid>

        <div style={{ background: 'yellow', padding: 8, textAlign: 'center', fontWeight: 'bold' }}>DEBUG: AnalysisTabs should be below</div>
        <AnalysisTabs
          tabLabels={['Issues', 'Team', 'Epic', 'Sprint']}
          index={tabIndex}
          onChange={setTabIndex}
        >
          <Box>
            <IssueTable isLoading={isLoading} isError={isError} />
          </Box>
          <Box>
            <TeamTable teamMembers={teamMembers} isLoading={isLoading} isError={isError} />
          </Box>
          <Box>
            <TrendChart isLoading={isLoading} isError={isError} />
          </Box>
          <Box>
            <TimeInStatusChart isLoading={isLoading} isError={isError} />
          </Box>
        </AnalysisTabs>

        <Box mt={4}>
          <CumulativeFlowChart isLoading={isLoading} isError={isError} />
        </Box>
      </Box>
    </Box>
  );
}; 