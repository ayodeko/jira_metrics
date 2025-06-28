import React from 'react';
import { Box, Text, useColorModeValue } from '@chakra-ui/react';
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import type { CumulativeFlowData } from '../../services/metricsService';

interface CumulativeFlowChartProps {
  data?: CumulativeFlowData;
  isLoading?: boolean;
  isError?: boolean;
}

// A set of colors for the chart bands
const COLORS = ['#8884d8', '#82ca9d', '#ffc658', '#ff8042', '#0088FE', '#00C49F', '#FFBB28', '#FF8042'];

export const CumulativeFlowChart: React.FC<CumulativeFlowChartProps> = ({
  data,
  isLoading,
  isError,
}) => {
  const bgColor = useColorModeValue('white', 'gray.700');
  const textColor = useColorModeValue('gray.600', 'gray.200');

  if (isLoading) {
    return <Box>Loading Chart...</Box>;
  }

  if (isError || !data || !data.dataPoints || data.dataPoints.length === 0) {
    return <Box>No data available for Cumulative Flow Diagram.</Box>;
  }

  // Transform the data for the chart
  const chartData = data.dataPoints.map(dp => ({
    date: new Date(dp.date).toLocaleDateString('en-CA'), // Format as YYYY-MM-DD
    ...dp.statusCounts,
  }));

  const statuses = Object.keys(chartData[0] || {}).filter(key => key !== 'date');

  return (
    <Box p={4} bg={bgColor} borderRadius="md" boxShadow="sm" color={textColor}>
      <Text fontSize="lg" fontWeight="medium" mb={4}>
        Cumulative Flow Diagram
      </Text>
      <ResponsiveContainer width="100%" height={400}>
        <AreaChart
          data={chartData}
          margin={{
            top: 10,
            right: 30,
            left: 20,
            bottom: 30,
          }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="date" angle={-45} textAnchor="end" height={70} />
          <YAxis label={{ value: 'Issue Count', angle: -90, position: 'insideLeft' }} />
          <Tooltip />
          <Legend wrapperStyle={{ bottom: 0 }} />
          {statuses.map((status, index) => (
            <Area
              key={status}
              type="monotone"
              dataKey={status}
              stackId="1"
              stroke={COLORS[index % COLORS.length]}
              fill={COLORS[index % COLORS.length]}
            />
          ))}
        </AreaChart>
      </ResponsiveContainer>
    </Box>
  );
};