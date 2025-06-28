import React from 'react';
import { Drawer, DrawerBody, DrawerHeader, DrawerOverlay, DrawerContent, DrawerCloseButton, Box, Text, Tabs, TabList, TabPanels, Tab, TabPanel, Flex, useColorModeValue } from '@chakra-ui/react';
interface SideDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  metricType: 'leadTime' | 'cycleTime' | 'throughput' | 'wip' | 'reopenRate';
}
export const SideDrawer: React.FC<SideDrawerProps> = ({
  isOpen,
  onClose,
  title,
  metricType
}) => {
  const bgColor = useColorModeValue('white', 'gray.800');
  const renderPlaceholderChart = (title: string) => <Box height="200px" bg={useColorModeValue('gray.50', 'gray.700')} borderRadius="md" display="flex" alignItems="center" justifyContent="center" mb={4}>
      <Text>{title} Chart</Text>
    </Box>;
  return <Drawer isOpen={isOpen} placement="right" onClose={onClose} size="md">
      <DrawerOverlay />
      <DrawerContent bg={bgColor}>
        <DrawerCloseButton _focus={{
        boxShadow: 'outline',
        bg: 'blue.50'
      }} />
        <DrawerHeader borderBottomWidth="1px">{title} Details</DrawerHeader>
        <DrawerBody>
          <Text fontSize="sm" color="gray.500" mb={4}>
            {metricType === 'leadTime' && 'Time from creation to completion'}
            {metricType === 'cycleTime' && 'Time from in progress to done'}
            {metricType === 'throughput' && 'Number of items completed per period'}
            {metricType === 'wip' && 'Current work in progress items'}
            {metricType === 'reopenRate' && 'Percentage of issues reopened after completion'}
          </Text>
          <Tabs colorScheme="blue" isLazy>
            <TabList>
              <Tab _selected={{
              color: 'blue.600',
              borderColor: 'blue.500'
            }} _focus={{
              boxShadow: 'outline',
              outline: '2px solid',
              outlineColor: 'blue.500',
              zIndex: 1
            }}>
                Distribution
              </Tab>
              <Tab _selected={{
              color: 'blue.600',
              borderColor: 'blue.500'
            }} _focus={{
              boxShadow: 'outline',
              outline: '2px solid',
              outlineColor: 'blue.500',
              zIndex: 1
            }}>
                Trend Over Time
              </Tab>
              <Tab _selected={{
              color: 'blue.600',
              borderColor: 'blue.500'
            }} _focus={{
              boxShadow: 'outline',
              outline: '2px solid',
              outlineColor: 'blue.500',
              zIndex: 1
            }}>
                By Sprint
              </Tab>
            </TabList>
            <TabPanels>
              <TabPanel>
                {renderPlaceholderChart('Histogram')}
                <Box mb={6}>
                  <Flex justifyContent="space-between" mb={2}>
                    <Text fontSize="sm" fontWeight="medium">
                      Percentiles
                    </Text>
                    <Text fontSize="sm" fontWeight="medium">
                      Days
                    </Text>
                  </Flex>
                  {[{
                  percentile: '50th',
                  value: '3.2'
                }, {
                  percentile: '70th',
                  value: '4.5'
                }, {
                  percentile: '85th',
                  value: '6.1'
                }, {
                  percentile: '95th',
                  value: '9.7'
                }].map(item => <Flex key={item.percentile} justifyContent="space-between" py={1} borderBottomWidth="1px" borderColor={useColorModeValue('gray.100', 'gray.700')}>
                      <Text fontSize="sm">{item.percentile} percentile</Text>
                      <Text fontSize="sm" fontWeight="medium">
                        {item.value}
                      </Text>
                    </Flex>)}
                </Box>
              </TabPanel>
              <TabPanel>
                {renderPlaceholderChart('Trend')}
                <Box mb={6}>
                  <Flex justifyContent="space-between" mb={2}>
                    <Text fontSize="sm" fontWeight="medium">
                      Period
                    </Text>
                    <Text fontSize="sm" fontWeight="medium">
                      Average
                    </Text>
                  </Flex>
                  {[{
                  period: 'Last Week',
                  value: '3.8'
                }, {
                  period: '2 Weeks Ago',
                  value: '4.2'
                }, {
                  period: '3 Weeks Ago',
                  value: '5.1'
                }, {
                  period: '4 Weeks Ago',
                  value: '4.7'
                }].map(item => <Flex key={item.period} justifyContent="space-between" py={1} borderBottomWidth="1px" borderColor={useColorModeValue('gray.100', 'gray.700')}>
                      <Text fontSize="sm">{item.period}</Text>
                      <Text fontSize="sm" fontWeight="medium">
                        {item.value}
                      </Text>
                    </Flex>)}
                </Box>
              </TabPanel>
              <TabPanel>
                {renderPlaceholderChart('Sprint Comparison')}
                <Box mb={6}>
                  <Flex justifyContent="space-between" mb={2}>
                    <Text fontSize="sm" fontWeight="medium">
                      Sprint
                    </Text>
                    <Text fontSize="sm" fontWeight="medium">
                      Average
                    </Text>
                  </Flex>
                  {[{
                  sprint: 'Sprint 22',
                  value: '3.5'
                }, {
                  sprint: 'Sprint 21',
                  value: '4.1'
                }, {
                  sprint: 'Sprint 20',
                  value: '5.2'
                }, {
                  sprint: 'Sprint 19',
                  value: '4.8'
                }].map(item => <Flex key={item.sprint} justifyContent="space-between" py={1} borderBottomWidth="1px" borderColor={useColorModeValue('gray.100', 'gray.700')}>
                      <Text fontSize="sm">{item.sprint}</Text>
                      <Text fontSize="sm" fontWeight="medium">
                        {item.value}
                      </Text>
                    </Flex>)}
                </Box>
              </TabPanel>
            </TabPanels>
          </Tabs>
        </DrawerBody>
      </DrawerContent>
    </Drawer>;
};