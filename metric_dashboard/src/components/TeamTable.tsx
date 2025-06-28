import React, { useState } from 'react';
import { Box, Table, Thead, Tbody, Tr, Th, Td, Text, Flex, Button, IconButton, Input, InputGroup, InputLeftElement, useColorModeValue, Skeleton } from '@chakra-ui/react';
import { ChevronDown, ChevronUp, Search, ChevronLeft, ChevronRight } from 'lucide-react';

interface TeamMember {
  name: string;
  issuesAssigned: number;
  issuesCompleted: number;
  averageLeadTime: number;
  averageCycleTime: number;
  throughput: number;
}

interface TeamTableProps {
  teamMembers?: TeamMember[];
  isLoading?: boolean;
  isError?: boolean;
}

export const TeamTable: React.FC<TeamTableProps> = ({
  teamMembers = [],
  isLoading = false,
  isError = false
}) => {
  const [sortField, setSortField] = useState<keyof TeamMember>('name');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [searchQuery, setSearchQuery] = useState('');

  const handleSort = (field: keyof TeamMember) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getSortIcon = (field: keyof TeamMember) => {
    if (sortField !== field) return null;
    return sortDirection === 'asc' ? <ChevronUp size={16} /> : <ChevronDown size={16} />;
  };

  const evenRowBg = useColorModeValue('gray.50', 'gray.700');

  // Filter team members based on search query
  const filteredTeamMembers = teamMembers.filter(member => 
    member.name && member.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (isError) {
    return (
      <Box width="100%" p={8} textAlign="center" borderRadius="md" bg={useColorModeValue('red.50', 'red.900')}>
        <Text color="red.500">Error loading team data. Please try again.</Text>
        <Button colorScheme="red" size="sm" mt={4}>
          Retry
        </Button>
      </Box>
    );
  }

  if (isLoading) {
    return (
      <Box width="100%" p={4}>
        <Skeleton height="40px" mb={4} />
        <Skeleton height="40px" mb={4} />
        <Skeleton height="40px" mb={4} />
        <Skeleton height="40px" />
      </Box>
    );
  }

  return (
    <Box width="100%">
      <Flex mb={4} justifyContent="space-between" alignItems="center">
        <InputGroup maxW="300px">
          <InputLeftElement pointerEvents="none">
            <Search size={16} />
          </InputLeftElement>
          <Input
            placeholder="Search team members..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            _focus={{
              boxShadow: 'outline',
              borderColor: 'blue.500'
            }}
          />
        </InputGroup>
      </Flex>

      {filteredTeamMembers.length === 0 ? (
        <Box width="100%" p={8} textAlign="center" borderRadius="md" bg={useColorModeValue('gray.50', 'gray.700')}>
          <Text>No team members match your search criteria.</Text>
          {searchQuery && (
            <Button
              size="sm"
              mt={4}
              onClick={() => setSearchQuery('')}
              _focus={{
                boxShadow: 'outline',
                borderColor: 'blue.500'
              }}
            >
              Reset filters
            </Button>
          )}
        </Box>
      ) : (
        <Table variant="simple" size="sm">
          <Thead>
            <Tr>
              <Th
                cursor="pointer"
                onClick={() => handleSort('name')}
                _hover={{
                  bg: useColorModeValue('gray.50', 'gray.700')
                }}
                _focus={{
                  boxShadow: 'outline',
                  outline: '2px solid',
                  outlineColor: 'blue.500'
                }}
                tabIndex={0}
                aria-label="Sort by name"
                aria-sort={sortField === 'name' ? sortDirection : undefined}
              >
                <Flex alignItems="center">
                  NAME {getSortIcon('name')}
                </Flex>
              </Th>
              <Th
                cursor="pointer"
                onClick={() => handleSort('issuesAssigned')}
                _hover={{
                  bg: useColorModeValue('gray.50', 'gray.700')
                }}
                _focus={{
                  boxShadow: 'outline',
                  outline: '2px solid',
                  outlineColor: 'blue.500'
                }}
                tabIndex={0}
                aria-label="Sort by issues assigned"
                aria-sort={sortField === 'issuesAssigned' ? sortDirection : undefined}
              >
                <Flex alignItems="center">
                  ISSUES ASSIGNED {getSortIcon('issuesAssigned')}
                </Flex>
              </Th>
              <Th
                cursor="pointer"
                onClick={() => handleSort('issuesCompleted')}
                _hover={{
                  bg: useColorModeValue('gray.50', 'gray.700')
                }}
                _focus={{
                  boxShadow: 'outline',
                  outline: '2px solid',
                  outlineColor: 'blue.500'
                }}
                tabIndex={0}
                aria-label="Sort by issues completed"
                aria-sort={sortField === 'issuesCompleted' ? sortDirection : undefined}
              >
                <Flex alignItems="center">
                  ISSUES COMPLETED {getSortIcon('issuesCompleted')}
                </Flex>
              </Th>
              <Th
                cursor="pointer"
                onClick={() => handleSort('averageLeadTime')}
                _hover={{
                  bg: useColorModeValue('gray.50', 'gray.700')
                }}
                _focus={{
                  boxShadow: 'outline',
                  outline: '2px solid',
                  outlineColor: 'blue.500'
                }}
                tabIndex={0}
                aria-label="Sort by average lead time"
                aria-sort={sortField === 'averageLeadTime' ? sortDirection : undefined}
              >
                <Flex alignItems="center">
                  AVG LEAD TIME {getSortIcon('averageLeadTime')}
                </Flex>
              </Th>
              <Th
                cursor="pointer"
                onClick={() => handleSort('averageCycleTime')}
                _hover={{
                  bg: useColorModeValue('gray.50', 'gray.700')
                }}
                _focus={{
                  boxShadow: 'outline',
                  outline: '2px solid',
                  outlineColor: 'blue.500'
                }}
                tabIndex={0}
                aria-label="Sort by average cycle time"
                aria-sort={sortField === 'averageCycleTime' ? sortDirection : undefined}
              >
                <Flex alignItems="center">
                  AVG CYCLE TIME {getSortIcon('averageCycleTime')}
                </Flex>
              </Th>
              <Th
                cursor="pointer"
                onClick={() => handleSort('throughput')}
                _hover={{
                  bg: useColorModeValue('gray.50', 'gray.700')
                }}
                _focus={{
                  boxShadow: 'outline',
                  outline: '2px solid',
                  outlineColor: 'blue.500'
                }}
                tabIndex={0}
                aria-label="Sort by throughput"
                aria-sort={sortField === 'throughput' ? sortDirection : undefined}
              >
                <Flex alignItems="center">
                  THROUGHPUT {getSortIcon('throughput')}
                </Flex>
              </Th>
            </Tr>
          </Thead>
          <Tbody>
            {filteredTeamMembers.map((member, index) => (
              <Tr key={member.name} bg={index % 2 === 1 ? evenRowBg : undefined}>
                <Td fontWeight="medium">{member.name}</Td>
                <Td>{member.issuesAssigned} issues</Td>
                <Td>{member.issuesCompleted} issues</Td>
                <Td>{member.averageLeadTime.toFixed(1)} days</Td>
                <Td>{member.averageCycleTime.toFixed(1)} days</Td>
                <Td>{member.throughput} issues/week</Td>
              </Tr>
            ))}
          </Tbody>
        </Table>
      )}
    </Box>
  );
}; 