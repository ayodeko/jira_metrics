import React from 'react';
import { Box, Flex, Text, Select, Button, ButtonGroup, IconButton, Tooltip, useColorModeValue, InputGroup, Input, InputRightElement } from '@chakra-ui/react';
import { RefreshCw, Share2, ChevronDown, Calendar, LogOut } from 'lucide-react';

interface HeaderProps {
  selectedProject: string;
  selectedScope: 'Issue' | 'Team' | 'Epic' | 'Sprint' | 'Project';
  onProjectChange: (project: string) => void;
  onScopeChange: (scope: 'Issue' | 'Team' | 'Epic' | 'Sprint' | 'Project') => void;
  onRefresh?: () => void;
  onLogout?: () => void;
  dateRange?: { startDate: string | null, endDate: string | null };
  onDateRangeChange?: (start: string | null, end: string | null) => void;
}

export const Header: React.FC<HeaderProps> = ({
  selectedProject,
  selectedScope,
  onProjectChange,
  onScopeChange,
  onRefresh,
  onLogout,
  dateRange,
  onDateRangeChange
}) => {
  const bgColor = useColorModeValue('white', 'gray.800');
  const borderColor = useColorModeValue('gray.200', 'gray.700');
  
  return <Box as="header" position="sticky" top={0} bg={bgColor} borderBottom="1px solid" borderColor={borderColor} height="48px" zIndex={10} width="100%" px={4}>
      <Flex height="100%" alignItems="center" justifyContent="space-between">
        <Flex alignItems="center">
          <Text fontSize="xl" fontWeight="semibold" color="blue.600" mr={6} _focus={{
          boxShadow: 'outline',
          outline: '2px solid',
          outlineColor: 'blue.500'
        }} tabIndex={0}>
            JiraProcX
          </Text>
          <Box maxW="200px" mr={4}>
            <Select value={selectedProject} onChange={e => onProjectChange(e.target.value)} size="sm" aria-label="Select project" _focus={{
            boxShadow: 'outline',
            borderColor: 'blue.500'
          }}>
              <option value="">Select project</option>
              <option value="project1">Project Alpha</option>
              <option value="project2">Project Beta</option>
              <option value="project3">Project Gamma</option>
            </Select>
          </Box>
        </Flex>
        <Flex alignItems="center" flex={1} justifyContent="center">
          <ButtonGroup size="sm" isAttached variant="outline" aria-label="Scope selection" mr={4}>
            {(['Issue', 'Team', 'Epic', 'Sprint'] as const).map(scope => (
              <Button
                key={scope}
                isActive={selectedScope === scope}
                onClick={() => onScopeChange(scope)}
                _active={{
                  bg: 'blue.500',
                  color: 'white'
                }}
                _focus={{
                  boxShadow: 'outline',
                  borderColor: 'blue.500'
                }}
                aria-pressed={selectedScope === scope}
              >
                {scope}
              </Button>
            ))}
          </ButtonGroup>
          <Flex alignItems="center" gap={2}>
            <Input
              type="date"
              size="sm"
              value={dateRange?.startDate || ''}
              onChange={e => onDateRangeChange?.(e.target.value || null, dateRange?.endDate || null)}
              max={dateRange?.endDate || undefined}
              placeholder="Start date"
              aria-label="Start date"
              width="120px"
            />
            <Text mx={1}>-</Text>
            <Input
              type="date"
              size="sm"
              value={dateRange?.endDate || ''}
              onChange={e => onDateRangeChange?.(dateRange?.startDate || null, e.target.value || null)}
              min={dateRange?.startDate || undefined}
              placeholder="End date"
              aria-label="End date"
              width="120px"
            />
          </Flex>
        </Flex>
        <Flex alignItems="center">
          <Tooltip label="Refresh data" aria-label="Refresh data">
            <IconButton icon={<RefreshCw size={16} />} aria-label="Refresh data" size="sm" variant="ghost" mr={2} _focus={{
            boxShadow: 'outline',
            borderColor: 'blue.500'
          }} onClick={onRefresh} />
          </Tooltip>
          <Button rightIcon={<ChevronDown size={16} />} size="sm" variant="ghost" mr={2} _focus={{
          boxShadow: 'outline',
          borderColor: 'blue.500'
        }}>
            Export
          </Button>
          <Tooltip label="Share dashboard" aria-label="Share dashboard">
            <IconButton icon={<Share2 size={16} />} aria-label="Share dashboard" size="sm" variant="ghost" mr={2} _focus={{
            boxShadow: 'outline',
            borderColor: 'blue.500'
          }} />
          </Tooltip>
          <Tooltip label="Logout" aria-label="Logout">
            <IconButton 
              icon={<LogOut size={16} />} 
              aria-label="Logout" 
              size="sm" 
              variant="ghost" 
              colorScheme="red"
              _focus={{
                boxShadow: 'outline',
                borderColor: 'red.500'
              }} 
              onClick={onLogout} 
            />
          </Tooltip>
        </Flex>
      </Flex>
    </Box>;
};