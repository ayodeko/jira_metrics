import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { JiraCredentials } from '../components/LoginPage';

interface AuthContextType {
  isAuthenticated: boolean;
  credentials: JiraCredentials | null;
  login: (credentials: JiraCredentials) => Promise<boolean>;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [credentials, setCredentials] = useState<JiraCredentials | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // Check for saved credentials on mount
  useEffect(() => {
    const savedCredentials = localStorage.getItem('jiraCredentials');
    if (savedCredentials) {
      try {
        const parsed = JSON.parse(savedCredentials);
        setCredentials(parsed);
        // Don't auto-authenticate, let user explicitly login
      } catch (error) {
        console.error('Failed to parse saved credentials:', error);
        localStorage.removeItem('jiraCredentials');
      }
    }
  }, []);

  const login = async (newCredentials: JiraCredentials): Promise<boolean> => {
    setIsLoading(true);
    try {
      // Test the credentials by making a simple API call
      const response = await fetch('http://localhost:5024/api/metrics/test-connection', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newCredentials),
      });

      if (response.ok) {
        setCredentials(newCredentials);
        setIsAuthenticated(true);
        return true;
      } else {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Authentication failed');
      }
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    setIsAuthenticated(false);
    setCredentials(null);
    localStorage.removeItem('jiraCredentials');
  };

  const value: AuthContextType = {
    isAuthenticated,
    credentials,
    login,
    logout,
    isLoading,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}; 