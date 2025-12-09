// Extend Window interface to include __env
declare global {
  interface Window {
    __env?: {
      apiUrl: string;
      authUrl: string;
    };
  }
}

// Read from window.__env or use defaults
export const environment = {
  production: true,
  apiUrl: window.__env?.apiUrl || 'http://localhost:5000/api',
  authUrl: window.__env?.authUrl || 'http://localhost:5063/api'
};
