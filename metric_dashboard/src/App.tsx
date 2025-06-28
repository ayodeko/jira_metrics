import React from 'react';
import { ChakraProvider } from '@chakra-ui/react';
import { AuthProvider } from './contexts/AuthContext';
import { AppContent } from './components/AppContent';

export function App() {
  return (
    <ChakraProvider>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </ChakraProvider>
  );
}