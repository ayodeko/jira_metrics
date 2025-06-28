import React, { useState } from 'react';
import { Box, Table, Thead, Tbody, Tr, Th, Td, Text, Flex, Button, IconButton, Input, InputGroup, InputLeftElement, useColorModeValue, Skeleton, Stack } from '@chakra-ui/react';
import { ChevronDown, ChevronUp, Search, ChevronLeft, ChevronRight } from 'lucide-react';
interface Issue {
  key: string;
  summary: string;
  created: string;
  done: string;
  leadTime: string;
  cycleTime: string;
  sprint: string;
}
interface IssueTableProps {
  issues?: Issue[];
  isLoading?: boolean;
  isError?: boolean;
}
export const IssueTable: React.FC<IssueTableProps> = ({
  issues = [],
  isLoading = false,
  isError = false
}) => {
  const [sortField, setSortField] = useState<keyof Issue>('done');
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('desc');
  const [searchQuery, setSearchQuery] = useState('');
  const handleSort = (field: keyof Issue) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('desc');
    }
  };
  const getSortIcon = (field: keyof Issue) => {
    if (sortField !== field) return null;
    return sortDirection === 'asc' ? <ChevronUp size={16} /> : <ChevronDown size={16} />;
  };
  const evenRowBg = useColorModeValue('gray.50', 'gray.700');
  // Filter issues based on search query
  const filteredIssues = issues.filter(issue => issue.key.toLowerCase().includes(searchQuery.toLowerCase()) || issue.summary.toLowerCase().includes(searchQuery.toLowerCase()));
  if (isError) {
    return <Box width="100%" p={8} textAlign="center" borderRadius="md" bg={useColorModeValue('red.50', 'red.900')}>
        <Text color="red.500">Error loading issue data. Please try again.</Text>
        <Button colorScheme="red" size="sm" mt={4}>
          Retry
        </Button>
      </Box>;
  }
  return <Box width="100%" overflowX="auto">
      <Flex mb={4} justifyContent="space-between" alignItems="center">
        <InputGroup maxW="300px">
          <InputLeftElement pointerEvents="none">
            <Search size={16} />
          </InputLeftElement>
          <Input placeholder="Search issues" value={searchQuery} onChange={e => setSearchQuery(e.target.value)} size="sm" aria-label="Search issues" _focus={{
          boxShadow: 'outline',
          borderColor: 'blue.500'
        }} />
        </InputGroup>
        <Text fontSize="sm" color="gray.500">
          {filteredIssues.length}{' '}
          {filteredIssues.length === 1 ? 'issue' : 'issues'}
        </Text>
      </Flex>
      {isLoading ? <Stack spacing={2}>
          {Array.from({
        length: 5
      }).map((_, i) => <Skeleton key={i} height="40px" />)}
        </Stack> : filteredIssues.length === 0 ? <Box width="100%" p={8} textAlign="center" borderRadius="md" bg={useColorModeValue('gray.50', 'gray.700')}>
          <Text>No issues match your search criteria.</Text>
          {searchQuery && <Button size="sm" mt={4} onClick={() => setSearchQuery('')} _focus={{
        boxShadow: 'outline',
        borderColor: 'blue.500'
      }}>
              Reset filters
            </Button>}
        </Box> : <Table variant="simple" size="sm">
          <Thead>
            <Tr>
              <Th cursor="pointer" onClick={() => handleSort('key')} _hover={{
            bg: useColorModeValue('gray.50', 'gray.700')
          }} _focus={{
            boxShadow: 'outline',
            outline: '2px solid',
            outlineColor: 'blue.500'
          }} tabIndex={0} aria-label="Sort by key" aria-sort={sortField === 'key' ? (sortDirection === 'asc' ? 'ascending' : 'descending') : undefined}>
                <Flex alignItems="center">KEY {getSortIcon('key')}</Flex>
              </Th>
              <Th>SUMMARY</Th>
              <Th>CREATED</Th>
              <Th cursor="pointer" onClick={() => handleSort('done')} _hover={{
            bg: useColorModeValue('gray.50', 'gray.700')
          }} _focus={{
            boxShadow: 'outline',
            outline: '2px solid',
            outlineColor: 'blue.500'
          }} tabIndex={0} aria-label="Sort by done date" aria-sort={sortField === 'done' ? (sortDirection === 'asc' ? 'ascending' : 'descending') : undefined}>
                <Flex alignItems="center">DONE {getSortIcon('done')}</Flex>
              </Th>
              <Th>LEAD TIME</Th>
              <Th>CYCLE TIME</Th>
              <Th>SPRINT/EPIC</Th>
            </Tr>
          </Thead>
          <Tbody>
            {filteredIssues.map((issue, index) => <Tr key={issue.key} bg={index % 2 === 1 ? evenRowBg : undefined}>
                <Td color="blue.500">{issue.key}</Td>
                <Td maxW="300px" isTruncated title={issue.summary}>
                  {issue.summary}
                </Td>
                <Td>{issue.created}</Td>
                <Td>{issue.done}</Td>
                <Td>{issue.leadTime}</Td>
                <Td>{issue.cycleTime}</Td>
                <Td>
                  <Box display="inline-block" px={2} py={1} bg="blue.100" color="blue.700" fontSize="xs" borderRadius="md">
                    {issue.sprint}
                  </Box>
                </Td>
              </Tr>)}
          </Tbody>
        </Table>}
      <Flex mt={4} justifyContent="flex-end" alignItems="center">
        <Text fontSize="sm" mr={4}>
          Page 1 of 1
        </Text>
        <Button leftIcon={<ChevronLeft size={16} />} size="sm" mr={2} isDisabled _focus={{
        boxShadow: 'outline',
        borderColor: 'blue.500'
      }}>
          Prev
        </Button>
        <Button rightIcon={<ChevronRight size={16} />} size="sm" isDisabled _focus={{
        boxShadow: 'outline',
        borderColor: 'blue.500'
      }}>
          Next
        </Button>
      </Flex>
    </Box>;
};