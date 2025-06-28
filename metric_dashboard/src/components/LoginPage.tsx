import React, { useState, useEffect } from 'react';
import {
  Box,
  VStack,
  HStack,
  FormControl,
  FormLabel,
  Input,
  Button,
  Heading,
  Text,
  useToast,
  Card,
  CardBody,
  InputGroup,
  InputRightElement,
  IconButton,
  Checkbox,
  FormHelperText,
} from '@chakra-ui/react';
import { ViewIcon, ViewOffIcon } from '@chakra-ui/icons';

export interface JiraCredentials {
  baseUrl: string;
  email: string;
  apiToken: string;
}

interface LoginPageProps {
  onLogin: (credentials: JiraCredentials) => void;
  isLoading?: boolean;
}

export const LoginPage: React.FC<LoginPageProps> = ({ onLogin, isLoading = false }) => {
  const [credentials, setCredentials] = useState<JiraCredentials>({
    baseUrl: '',
    email: '',
    apiToken: '',
  });
  const [showApiToken, setShowApiToken] = useState(false);
  const [rememberCredentials, setRememberCredentials] = useState(true);
  const [errors, setErrors] = useState<Partial<JiraCredentials>>({});
  const toast = useToast();

  // Load saved credentials on component mount
  useEffect(() => {
    const savedCredentials = localStorage.getItem('jiraCredentials');
    if (savedCredentials) {
      try {
        const parsed = JSON.parse(savedCredentials);
        setCredentials(parsed);
        setRememberCredentials(true);
      } catch (error) {
        console.error('Failed to parse saved credentials:', error);
      }
    }
  }, []);

  const validateForm = (): boolean => {
    const newErrors: Partial<JiraCredentials> = {};

    if (!credentials.baseUrl.trim()) {
      newErrors.baseUrl = 'Base URL is required';
    } else if (!credentials.baseUrl.startsWith('http://') && !credentials.baseUrl.startsWith('https://')) {
      newErrors.baseUrl = 'Base URL must start with http:// or https://';
    }

    if (!credentials.email.trim()) {
      newErrors.email = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(credentials.email)) {
      newErrors.email = 'Please enter a valid email address';
    }

    if (!credentials.apiToken.trim()) {
      newErrors.apiToken = 'API Token is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      // Save credentials to localStorage if remember is checked
      if (rememberCredentials) {
        localStorage.setItem('jiraCredentials', JSON.stringify(credentials));
      } else {
        localStorage.removeItem('jiraCredentials');
      }

      onLogin(credentials);
    } catch (error) {
      toast({
        title: 'Login failed',
        description: 'Please check your credentials and try again.',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const handleInputChange = (field: keyof JiraCredentials, value: string) => {
    setCredentials(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  return (
    <Box
      minH="100vh"
      bg="gray.50"
      display="flex"
      alignItems="center"
      justifyContent="center"
      p={4}
    >
      <Card maxW="md" w="full" shadow="lg">
        <CardBody p={8}>
          <VStack spacing={6} align="stretch">
            <Box textAlign="center">
              <Heading size="lg" color="blue.600" mb={2}>
                Jira Metrics Dashboard
              </Heading>
              <Text color="gray.600">
                Enter your Jira credentials to access metrics
              </Text>
            </Box>

            <form onSubmit={handleSubmit}>
              <VStack spacing={4} align="stretch">
                <FormControl isInvalid={!!errors.baseUrl}>
                  <FormLabel>Jira Base URL</FormLabel>
                  <Input
                    type="url"
                    placeholder="https://your-domain.atlassian.net"
                    value={credentials.baseUrl}
                    onChange={(e) => handleInputChange('baseUrl', e.target.value)}
                    size="lg"
                  />
                  {errors.baseUrl && (
                    <FormHelperText color="red.500">{errors.baseUrl}</FormHelperText>
                  )}
                </FormControl>

                <FormControl isInvalid={!!errors.email}>
                  <FormLabel>Email</FormLabel>
                  <Input
                    type="email"
                    placeholder="your-email@example.com"
                    value={credentials.email}
                    onChange={(e) => handleInputChange('email', e.target.value)}
                    size="lg"
                  />
                  {errors.email && (
                    <FormHelperText color="red.500">{errors.email}</FormHelperText>
                  )}
                </FormControl>

                <FormControl isInvalid={!!errors.apiToken}>
                  <FormLabel>API Token</FormLabel>
                  <InputGroup size="lg">
                    <Input
                      type={showApiToken ? 'text' : 'password'}
                      placeholder="Enter your Jira API token"
                      value={credentials.apiToken}
                      onChange={(e) => handleInputChange('apiToken', e.target.value)}
                    />
                    <InputRightElement>
                      <IconButton
                        aria-label={showApiToken ? 'Hide token' : 'Show token'}
                        icon={showApiToken ? <ViewOffIcon /> : <ViewIcon />}
                        onClick={() => setShowApiToken(!showApiToken)}
                        variant="ghost"
                        size="sm"
                      />
                    </InputRightElement>
                  </InputGroup>
                  {errors.apiToken && (
                    <FormHelperText color="red.500">{errors.apiToken}</FormHelperText>
                  )}
                  <FormHelperText>
                    You can find your API token in your{' '}
                    <a
                      href="https://id.atlassian.com/manage-profile/security/api-tokens"
                      target="_blank"
                      rel="noopener noreferrer"
                      style={{ color: 'blue', textDecoration: 'underline' }}
                    >
                      Atlassian account settings
                    </a>
                  </FormHelperText>
                </FormControl>

                <FormControl>
                  <Checkbox
                    isChecked={rememberCredentials}
                    onChange={(e) => setRememberCredentials(e.target.checked)}
                    size="lg"
                  >
                    Remember my credentials
                  </Checkbox>
                </FormControl>

                <Button
                  type="submit"
                  colorScheme="blue"
                  size="lg"
                  isLoading={isLoading}
                  loadingText="Connecting..."
                  w="full"
                >
                  Connect to Jira
                </Button>
              </VStack>
            </form>
          </VStack>
        </CardBody>
      </Card>
    </Box>
  );
}; 