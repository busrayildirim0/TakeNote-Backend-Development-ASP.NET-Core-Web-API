// js/api.js - TakeNote API Request Fetch Client

import { state } from './state.js';
import { handleLogout } from './components/auth.js';

export async function apiRequest(endpoint, options = {}) {
    const url = `${state.apiUrl}/${endpoint}`;
    
    // Inject headers
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    if (state.token) {
        headers['Authorization'] = `Bearer ${state.token}`;
    }

    const config = {
        ...options,
        headers
    };

    try {
        const response = await fetch(url, config);
        
        if (response.status === 204) {
            return null; // NoContent
        }

        if (response.status === 401) {
            handleLogout();
            throw new Error('Session expired. Please login again.');
        }

        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || `Request failed with status ${response.status}`);
        }
        return data;
    } catch (error) {
        console.error(`API Error [${endpoint}]:`, error);
        throw error;
    }
}
