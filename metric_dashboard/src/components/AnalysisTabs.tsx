import React, { Children } from 'react';
import { Tabs, TabList, TabPanels, Tab, TabPanel, Box, useColorModeValue } from '@chakra-ui/react';

interface AnalysisTabsProps {
  children: React.ReactNode;
  tabLabels: string[];
  defaultIndex?: number;
  index?: number;
  onChange?: (index: number) => void;
}

export const AnalysisTabs: React.FC<AnalysisTabsProps> = ({
  children,
  tabLabels,
  defaultIndex = 0,
  index,
  onChange
}) => {
  const activeBorderColor = useColorModeValue('blue.500', 'blue.300');

  return (
    <Tabs
      variant="line"
      colorScheme="blue"
      defaultIndex={defaultIndex}
      index={index}
      onChange={onChange}
      isLazy
    >
      <TabList borderBottomWidth="1px">
        {tabLabels.map((label, idx) => (
          <Tab
            key={idx}
            _selected={{
              color: 'blue.600',
              borderColor: activeBorderColor,
              borderBottomWidth: '2px'
            }}
            _focus={{
              boxShadow: 'outline',
              outline: '2px solid',
              outlineColor: 'blue.500',
              zIndex: 1
            }}
          >
            {label}
          </Tab>
        ))}
      </TabList>
      <TabPanels>
        {Children.map(children, (child, idx) => (
          <TabPanel key={idx} p={4}>
            {child}
          </TabPanel>
        ))}
      </TabPanels>
    </Tabs>
  );
};