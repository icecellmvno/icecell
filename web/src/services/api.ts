const API_URL = 'https://host.localhost:8001/api/v1';

interface LoginResponse {
  access_token: string;
  token_type: string;
}

export const api = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const response = await fetch(`${API_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      throw new Error('Giriş başarısız');
    }

    return response.json();
  },

  getUser: async (token: string) => {
    const response = await fetch(`${API_URL}/users/me`, {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      throw new Error('Kullanıcı bilgileri alınamadı');
    }

    return response.json();
  },
}; 