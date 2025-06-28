import React from 'react';
import { Box, Text, useColorModeValue } from '@chakra-ui/react';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
} from 'recharts';

interface MetricTrend {
  date: string | Date;
  value: number;
}
interface TrendData {
  leadTime: MetricTrend[];
  cycleTime: MetricTrend[];
  throughput: MetricTrend[];
}
interface TrendChartProps {
  data?: TrendData;
  isLoading?: boolean;
  isError?: boolean;
}

export const TrendChart: React.FC<TrendChartProps> = ({
  data,
  isLoading = false,
  isError = false
}) => {
  if (isLoading) {
    return <Box width="100%" height="300px" bg={useColorModeValue('white', 'gray.800')} borderRadius="md" p={4}><Text>Loading...</Text></Box>;
  }
  if (isError) {
    return <Box width="100%" height="300px" bg={useColorModeValue('white', 'gray.800')} borderRadius="md" p={4}><Text color="red.500">Failed to load data</Text></Box>;
  }
  if (!data || !data.leadTime.length) {
    return <Box width="100%" height="300px" bg={useColorModeValue('white', 'gray.800')} borderRadius="md" p={4}><Text>No trend data available</Text></Box>;
  }
  // Merge data by date for charting
  const chartData = data.leadTime.map((lt, i) => ({
    week: typeof lt.date === 'string' ? lt.date.substring(0, 10) : (lt.date as Date).toISOString().substring(0, 10),
    leadTime: lt.value,
    cycleTime: data.cycleTime[i]?.value ?? null,
    throughput: data.throughput[i]?.value ?? null,
  }));
  return (
    <Box width="100%" height="350px" bg={useColorModeValue('white', 'gray.800')} borderRadius="md" p={4} position="relative">
      <Text fontSize="md" fontWeight="medium" mb={4}>
        Trend Analysis
      </Text>
      <ResponsiveContainer width="100%" height={250}>
        <LineChart data={chartData} margin={{ top: 10, right: 30, left: 10, bottom: 30 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="week" angle={-30} textAnchor="end" height={60} />
          <YAxis yAxisId="left" label={{ value: 'Days', angle: -90, position: 'insideLeft' }} />
          <YAxis yAxisId="right" orientation="right" label={{ value: 'Throughput', angle: 90, position: 'insideRight' }} />
          <Tooltip />
          <Legend />
          <Line yAxisId="left" type="monotone" dataKey="leadTime" name="Lead Time (days)" stroke="#3182ce" strokeWidth={2} dot={false} />
          <Line yAxisId="left" type="monotone" dataKey="cycleTime" name="Cycle Time (days)" stroke="#9f7aea" strokeWidth={2} dot={false} />
          <Line yAxisId="right" type="monotone" dataKey="throughput" name="Throughput" stroke="#38a169" strokeWidth={2} dot={false} />
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
};