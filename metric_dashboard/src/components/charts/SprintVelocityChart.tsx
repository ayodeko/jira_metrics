import React from 'react';
import { Box, Text, Flex, useColorModeValue } from '@chakra-ui/react';
interface SprintVelocityChartProps {
  data?: any; // In a real implementation, this would be typed properly
}
export const SprintVelocityChart: React.FC<SprintVelocityChartProps> = ({
  data
}) => {
  // In a real implementation, this would use a charting library like Recharts
  // For now, we'll create a placeholder that shows the concept
  return <Box width="100%" height="300px" bg={useColorModeValue('white', 'gray.800')} borderRadius="md" p={4} position="relative">
      <Text fontSize="md" fontWeight="medium" mb={4}>
        Sprint Velocity
      </Text>
      <Flex height="200px" alignItems="flex-end" justifyContent="space-around">
        <Box width="15%" height="60%" bg="blue.500" borderTopRadius="md" />
        <Box width="15%" height="80%" bg="blue.500" borderTopRadius="md" />
        <Box width="15%" height="50%" bg="blue.500" borderTopRadius="md" />
        <Box width="15%" height="90%" bg="blue.500" borderTopRadius="md" />
        <Box width="15%" height="70%" bg="blue.500" borderTopRadius="md" />
      </Flex>
      <Flex justifyContent="space-around" mt={2}>
        {['Sprint 18', 'Sprint 19', 'Sprint 20', 'Sprint 21', 'Sprint 22'].map(sprint => <Text key={sprint} fontSize="xs">
              {sprint}
            </Text>)}
      </Flex>
      <Box position="absolute" left={0} top="50%" transform="translateY(-50%)" ml={-6}>
        <Text fontSize="xs" transform="rotate(-90deg)" width="100px" textAlign="center">
          Story Points
        </Text>
      </Box>
    </Box>;
};