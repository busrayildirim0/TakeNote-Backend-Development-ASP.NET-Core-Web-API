// js/components/auth.js - TakeNote Auth Operations Component

import { state } from '../state.js';
import { apiRequest } from '../api.js';
import { showToast, closeAllModals } from './ui.js';
import { initSignalR } from '../signalr-client.js';
import { selectWorkspace, loadWorkspaces } from './workspaces.js';

export function showSection(sectionId) {
    document.getElementById('authSection').classList.add('hidden');
    document.getElementById('dashboardSection').classList.add('hidden');
    document.getElementById(sectionId).classList.remove('hidden');
}

export async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById('loginEmail').value.trim();
    const password = document.getElementById('loginPassword').value;

    try {
        const data = await apiRequest('Auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });

        state.token = data.token;
        localStorage.setItem('tn_token', data.token);
        
        showToast('Success', `Welcome back, ${data.username}!`, 'success');
        
        // Load app
        await loadUserProfile();
        showSection('dashboardSection');
        await loadWorkspaces();
        await selectWorkspace(null); // Load personal notes first
        initSignalR();
    } catch (err) {
        showToast('Login Failed', err.message, 'error');
    }
}

export async function handleRegister(e) {
    e.preventDefault();
    const username = document.getElementById('registerUsername').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;

    try {
        await apiRequest('Auth/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });

        showToast('Account Created', 'Successfully registered! Please sign in.', 'success');
        // Switch to login tab
        document.getElementById('tabLogin').classList.add('active');
        document.getElementById('tabRegister').classList.remove('active');
        document.getElementById('loginForm').classList.remove('hidden');
        document.getElementById('registerForm').classList.add('hidden');
        
        document.getElementById('loginEmail').value = email;
    } catch (err) {
        showToast('Registration Failed', err.message, 'error');
    }
}

export async function loadUserProfile() {
    const data = await apiRequest('User/profile');
    state.user = data;
    
    // Update profile displays
    document.getElementById('headerUsername').textContent = data.username;
    document.getElementById('headerEmail').textContent = data.email;
    document.getElementById('headerUserAvatar').textContent = data.username.substring(0, 1).toUpperCase();
    
    document.getElementById('profileUsername').value = data.username;
    document.getElementById('profileEmail').value = data.email;
}

export async function handleUpdateProfile(e) {
    e.preventDefault();
    const username = document.getElementById('profileUsername').value.trim();

    try {
        const updated = await apiRequest('User/profile', {
            method: 'PUT',
            body: JSON.stringify({ username })
        });
        
        state.user = updated;
        document.getElementById('headerUsername').textContent = updated.username;
        document.getElementById('headerUserAvatar').textContent = updated.username.substring(0, 1).toUpperCase();
        closeAllModals();
        showToast('Profile Settings', 'Profile details updated successfully', 'success');
    } catch (err) {
        showToast('Error', err.message, 'error');
    }
}

export async function handleDeleteAccount() {
    if (confirm('WARNING: Are you absolutely sure you want to permanently delete your account? This action is irreversible.')) {
        try {
            await apiRequest('User/account', { method: 'DELETE' });
            showToast('Account Deleted', 'Your account has been successfully removed.', 'warning');
            handleLogout();
        } catch (err) {
            showToast('Error', err.message, 'error');
        }
    }
}

export function handleLogout() {
    state.token = '';
    state.user = null;
    state.workspaces = [];
    state.notes = [];
    state.activeNoteId = null;
    state.activeNote = null;
    localStorage.removeItem('tn_token');
    
    if (state.signalrConnection) {
        state.signalrConnection.stop();
        state.signalrConnection = null;
    }
    
    showSection('authSection');
}
