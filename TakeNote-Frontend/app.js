// app.js - TakeNote Modular Client Entry & Orchestrator

import { state } from './js/state.js';
import { apiRequest } from './js/api.js';
import { initSignalR, reconnectSignalR } from './js/signalr-client.js';
import { openModal, closeAllModals, showToast } from './js/components/ui.js';
import { 
    handleLogin, 
    handleRegister, 
    loadUserProfile, 
    handleUpdateProfile, 
    handleDeleteAccount, 
    handleLogout,
    showSection
} from './js/components/auth.js';
import { 
    loadWorkspaces, 
    selectWorkspace, 
    handleCreateWorkspace, 
    openPublicWorkspacesModal, 
    openWorkspaceSettingsModal, 
    addWorkspaceMember, 
    updateWorkspaceDetails, 
    leaveCurrentWorkspace, 
    deleteCurrentWorkspace 
} from './js/components/workspaces.js';
import { 
    handleCreateNoteBtnClick, 
    toggleActiveNotePin, 
    deleteActiveNote, 
    handleNoteTitleInput, 
    handleNoteContentInput, 
    handleNewTagKeydown, 
    addTagFromInput, 
    triggerSearch, 
    clearDateFilter 
} from './js/components/notes.js';
import { handleAddTask } from './js/components/tasks.js';

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    setupBackendSelectors();
    initApp();
});

function initApp() {
    if (state.token) {
        loadUserProfile()
            .then(() => {
                showSection('dashboardSection');
                loadWorkspaces();
                selectWorkspace(null); // Load personal notes first
                initSignalR();
            })
            .catch(err => {
                console.error("Auth initialization failed, logging out:", err);
                handleLogout();
            });
    } else {
        showSection('authSection');
    }
}

// --- BACKEND API DYNAMIC CONFIGURATION ---
function setupBackendSelectors() {
    const authSelect = document.getElementById('authBackendSelect');
    const authCustomContainer = document.getElementById('authCustomUrlContainer');
    const authCustomInput = document.getElementById('authCustomUrlInput');
    const dashboardSelect = document.getElementById('dashboardBackendSelect');

    // Load saved API URL if any
    const savedUrl = localStorage.getItem('tn_api_url');
    if (savedUrl) {
        state.apiUrl = savedUrl;
        state.hubUrl = savedUrl.replace(/\/api$/, '') + '/collaborationHub';
        
        setSelectorValue(authSelect, savedUrl);
        setSelectorValue(dashboardSelect, savedUrl);
    } else {
        authSelect.value = state.apiUrl;
        dashboardSelect.value = state.apiUrl;
    }

    authSelect.addEventListener('change', (e) => {
        if (e.target.value === 'custom') {
            authCustomContainer.classList.remove('hidden');
        } else {
            authCustomContainer.classList.add('hidden');
            updateApiUrls(e.target.value);
            dashboardSelect.value = e.target.value;
        }
    });

    authCustomInput.addEventListener('change', (e) => {
        if (e.target.value.trim()) {
            updateApiUrls(e.target.value.trim());
            addCustomOptionIfMissing(dashboardSelect, e.target.value.trim());
            dashboardSelect.value = e.target.value.trim();
        }
    });

    dashboardSelect.addEventListener('change', (e) => {
        if (e.target.value === 'custom') {
            openCustomUrlModal();
        } else {
            updateApiUrls(e.target.value);
            authSelect.value = e.target.value;
            if (state.token) {
                showToast('API Connection', 'Reconnecting to new API Server...', 'info');
                loadWorkspaces();
                selectWorkspace(state.currentWorkspaceId);
                reconnectSignalR();
            }
        }
    });
}

function updateApiUrls(url) {
    state.apiUrl = url;
    state.hubUrl = url.replace(/\/api$/, '') + '/collaborationHub';
    localStorage.setItem('tn_api_url', url);
}

function setSelectorValue(selectEl, val) {
    let matched = false;
    for (let option of selectEl.options) {
        if (option.value === val) {
            selectEl.value = val;
            matched = true;
            break;
        }
    }
    if (!matched && val) {
        addCustomOptionIfMissing(selectEl, val);
        selectEl.value = val;
    }
}

function addCustomOptionIfMissing(selectEl, val) {
    let exists = Array.from(selectEl.options).some(opt => opt.value === val);
    if (!exists) {
        const opt = document.createElement('option');
        opt.value = val;
        opt.textContent = `Custom (${val.substring(0, 25)}...)`;
        selectEl.insertBefore(opt, selectEl.options[selectEl.options.length - 1]);
    }
}

function openCustomUrlModal() {
    document.getElementById('customUrlInput').value = state.apiUrl;
    openModal('customUrlModal');
}

function saveCustomUrl() {
    const val = document.getElementById('customUrlInput').value.trim();
    if (val) {
        updateApiUrls(val);
        
        const authSelect = document.getElementById('authBackendSelect');
        const dashboardSelect = document.getElementById('dashboardBackendSelect');
        addCustomOptionIfMissing(authSelect, val);
        addCustomOptionIfMissing(dashboardSelect, val);
        authSelect.value = val;
        dashboardSelect.value = val;
        
        closeAllModals();
        showToast('API Configuration', 'Successfully switched api configuration', 'success');

        if (state.token) {
            loadWorkspaces();
            selectWorkspace(state.currentWorkspaceId);
            reconnectSignalR();
        }
    }
}

// --- BIND ES6 MODULE FUNCTIONS TO WINDOW OBJECT FOR HTML INLINE CALLBACKS ---
window.switchAuthTab = (tab) => {
    if (tab === 'login') {
        document.getElementById('tabLogin').classList.add('active');
        document.getElementById('tabRegister').classList.remove('active');
        document.getElementById('loginForm').classList.remove('hidden');
        document.getElementById('registerForm').classList.add('hidden');
    } else {
        document.getElementById('tabLogin').classList.remove('active');
        document.getElementById('tabRegister').classList.add('active');
        document.getElementById('loginForm').classList.add('hidden');
        document.getElementById('registerForm').classList.remove('hidden');
    }
};

window.handleLogin = handleLogin;
window.handleRegister = handleRegister;
window.openProfileModal = openProfileModal;
window.handleLogout = handleLogout;
window.selectWorkspace = selectWorkspace;
window.openCreateWorkspaceModal = () => openModal('createWorkspaceModal');
window.openPublicWorkspacesModal = openPublicWorkspacesModal;
window.triggerSearch = triggerSearch;
window.clearDateFilter = clearDateFilter;
window.toggleActiveNotePin = toggleActiveNotePin;
window.deleteActiveNote = deleteActiveNote;
window.handleNoteTitleInput = handleNoteTitleInput;
window.handleNewTagKeydown = handleNewTagKeydown;
window.addTagFromInput = addTagFromInput;
window.handleNoteContentInput = handleNoteContentInput;
window.handleAddTask = handleAddTask;
window.closeAllModals = closeAllModals;
window.handleCreateWorkspace = handleCreateWorkspace;
window.updateWorkspaceDetails = updateWorkspaceDetails;
window.addWorkspaceMember = addWorkspaceMember;
window.leaveCurrentWorkspace = leaveCurrentWorkspace;
window.deleteCurrentWorkspace = deleteCurrentWorkspace;
window.handleUpdateProfile = handleUpdateProfile;
window.handleDeleteAccount = handleDeleteAccount;
window.saveCustomUrl = saveCustomUrl;
window.openWorkspaceSettingsModal = openWorkspaceSettingsModal;
window.handleCreateNoteBtnClick = handleCreateNoteBtnClick;
