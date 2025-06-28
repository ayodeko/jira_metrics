import React from 'react';
import { ChakraProvider } from '@chakra-ui/react';
import { render } from 'react-dom';
import { App } from './App';
import { theme } from './utils/theme';
render(<ChakraProvider theme={theme}>
    <App />
  </ChakraProvider>, document.getElementById('root'));