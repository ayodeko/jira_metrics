import React from 'react';
import { Box, Flex, Text, useColorModeValue } from '@chakra-ui/react';
import type { TimeInStatusData } from '../../services/metricsService';

interface TimeInStatusChartProps {
  data?: TimeInStatusData;
  isLoading?: boolean;
  isError?: boolean;
}

export const TimeInStatusChart: React.FC<TimeInStatusChartProps> = ({
  data,
  isLoading = false,
  isError = false
}) => {
  // Use real data if available, otherwise fallback to placeholder
  const statusData = data?.statusDurations?.length
    ? data.statusDurations.map(d => ({ status: d.status, value: d.averageDays }))
    : [
        { status: 'TO DO', value: 6 },
        { status: 'IN PROGRESS', value: 8 },
        { status: 'CODE REVIEW', value: 4 },
        { status: 'QA', value: 3 },
        { status: 'DONE', value: 9 }
      ];
  const statusColors = {
    'TO DO': 'gray.200',
    'IN PROGRESS': 'blue.200',
    'CODE REVIEW': 'purple.200',
    QA: 'orange.200',
    DONE: 'green.200'
  };
  const bg = useColorModeValue('white', 'gray.800');
  const gridColor = useColorModeValue('gray.100', 'gray.700');

  if (isLoading) {
    return <Box width="100%" height="320px" bg={bg} borderRadius="lg" p={6}><Text>Loading...</Text></Box>;
  }
  if (isError) {
    return <Box width="100%" height="320px" bg={bg} borderRadius="lg" p={6}><Text color="red.500">Failed to load data</Text></Box>;
  }

  // Find max value for scaling
  const maxValue = Math.max(...statusData.map(d => d.value), 1);

  return (
    <Box width="100%" minH="320px" bg={bg} borderRadius="lg" p={6} boxShadow="sm" position="relative">
      <Text fontSize="lg" fontWeight="bold" mb={2} color="blue.700">Time in Status</Text>
      <Flex direction="row" align="flex-start" position="relative">
        {/* Y Axis Label */}
        <Box minW="40px" display="flex" alignItems="center" justifyContent="center" height="220px" mr={2}>
          <Text fontSize="sm" color="gray.500" transform="rotate(-90deg)">Days</Text>
        </Box>
        {/* Chart Area */}
        <Box position="relative" width="100%" height="220px" overflow="visible" display="flex" alignItems="flex-end" bg={gridColor} borderRadius="md" px={4} py={2}>
          {/* Subtle grid lines */}
          {[0.25, 0.5, 0.75].map((frac, i) => (
            <Box key={i} position="absolute" left={0} right={0} bottom={`${frac * 100}%`} height="1px" bg="gray.200" opacity={0.5} />
          ))}
          <Flex width="100%" height="100%" alignItems="flex-end" justifyContent="space-between" zIndex={1}>
            {statusData.map((d, i) => (
              <Box key={d.status} width="15%" mx={2} display="flex" flexDir="column" alignItems="center">
                {/* Bar */}
                <Box
                  height={`${(d.value / maxValue) * 180}px`}
                  width="100%"
                  bg={statusColors[d.status as keyof typeof statusColors] || 'gray.300'}
                  borderRadius="md"
                  transition="height 0.3s"
                  boxShadow="xs"
                  mb={1}
                />
                {/* Value label */}
                <Text fontSize="sm" fontWeight="medium" color="gray.700" mt={1}>{d.value.toFixed(1)} days</Text>
                {/* Status label directly under the bar */}
                <Text fontSize="sm" color="gray.600" mt={1} textAlign="center" whiteSpace="nowrap">{d.status}</Text>
              </Box>
            ))}
          </Flex>
        </Box>
      </Flex>
      {/* X Axis Label */}
      <Box width="100%" textAlign="center" mt={2}>
        <Text fontSize="sm" color="gray.500">Status</Text>
      </Box>
    </Box>
  );
};