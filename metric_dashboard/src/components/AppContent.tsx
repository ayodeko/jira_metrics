import React from 'react';
import { useAuth } from '../contexts/AuthContext';
import { LoginPage } from './LoginPage';
import { Dashboard } from '../pages/Dashboard';

export const AppContent: React.FC = () => {
  const { isAuthenticated, login, isLoading } = useAuth();

  const handleLogin = async (credentials: any) => {
    try {
      await login(credentials);
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  };

  if (!isAuthenticated) {
    return <LoginPage onLogin={handleLogin} isLoading={isLoading} />;
  }

  return <Dashboard />;
}; 