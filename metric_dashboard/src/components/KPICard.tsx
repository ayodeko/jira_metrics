import React from 'react';
import { Box, Text, Stat, StatLabel, StatNumber, StatHelpText, StatArrow, Flex, Skeleton, useColorModeValue } from '@chakra-ui/react';
interface KPICardProps {
  title: string;
  value: number | string;
  unit?: string;
  change: number;
  changeLabel?: string;
  status?: 'default' | 'loading' | 'error';
  colorScheme?: 'blue' | 'purple' | 'green' | 'orange' | 'red';
  onClick?: () => void;
}
export const KPICard: React.FC<KPICardProps> = ({
  title,
  value,
  unit = '',
  change,
  changeLabel = 'vs last period',
  status = 'default',
  colorScheme = 'blue',
  onClick
}) => {
  const bgColor = useColorModeValue('white', 'gray.800');
  const borderColor = useColorModeValue('gray.100', 'gray.700');
  const colorMap = {
    blue: 'blue.100',
    purple: 'purple.100',
    green: 'green.100',
    orange: 'orange.100',
    red: 'red.100'
  };
  const textColorMap = {
    blue: 'blue.600',
    purple: 'purple.600',
    green: 'green.600',
    orange: 'orange.600',
    red: 'red.600'
  };
  const indicatorColor = useColorModeValue(colorMap[colorScheme], `${colorScheme}.800`);
  const titleColor = useColorModeValue(textColorMap[colorScheme], `${colorScheme}.300`);
  const isPositive = change > 0;
  const isNegative = change < 0;
  return <Box bg={bgColor} borderRadius="md" boxShadow="sm" borderLeft="4px solid" borderColor={indicatorColor} p={4} width="100%" height="96px" onClick={onClick} cursor={onClick ? 'pointer' : 'default'} transition="all 0.2s" _hover={{
    boxShadow: onClick ? 'md' : 'sm',
    transform: onClick ? 'translateY(-2px)' : 'none'
  }} _focus={{
    boxShadow: 'outline',
    outline: '2px solid',
    outlineColor: 'blue.500'
  }} tabIndex={onClick ? 0 : undefined} role={onClick ? 'button' : undefined} aria-label={onClick ? `View detailed ${title} metrics` : undefined}>
      <Skeleton isLoaded={status !== 'loading'}>
        <Stat>
          <StatLabel color={titleColor} fontWeight="medium">
            {title}
          </StatLabel>
          <StatNumber fontSize="32px" fontWeight="bold" lineHeight="40px">
            {status === 'error' ? 'Error' : value}
            {unit && <Box as="span" fontSize="md" ml={1}>
                {unit}
              </Box>}
          </StatNumber>
          <StatHelpText mb={0}>
            {status !== 'error' && <Flex alignItems="center">
                <StatArrow type={isPositive ? 'increase' : 'decrease'} color={isPositive ? 'green.500' : isNegative ? 'red.500' : 'gray.500'} />
                <Text color={isPositive ? 'green.500' : isNegative ? 'red.500' : 'gray.500'} fontSize="sm">
                  {Math.abs(change)}% {changeLabel}
                </Text>
              </Flex>}
            {status === 'error' && <Text color="red.500" fontSize="sm">
                Failed to load data
              </Text>}
          </StatHelpText>
        </Stat>
      </Skeleton>
    </Box>;
};