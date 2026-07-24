// js/components/workspaces.js - TakeNote Workspaces Operations Component

import { state } from '../state.js';
import { apiRequest } from '../api.js';
import { showToast, closeAllModals, openModal, escapeHtml } from './ui.js';
import { loadNotes, renderActiveNote } from './notes.js';
import { populateTaskAssignmentSelect } from './tasks.js';

export async function loadWorkspaces() {
    try {
        const listContainer = document.getElementById('workspacesList');
        listContainer.innerHTML = `
            <div class="skeleton-loader">
                <div class="skeleton-line"></div>
                <div class="skeleton-line"></div>
            </div>`;

        const data = await apiRequest('Workspace');
        state.workspaces = data;
        renderWorkspaces();
    } catch (err) {
        showToast('Error Loading Workspaces', err.message, 'error');
    }
}

export function renderWorkspaces() {
    const listContainer = document.getElementById('workspacesList');
    if (!listContainer) return;
    listContainer.innerHTML = '';

    if (state.workspaces.length === 0) {
        listContainer.innerHTML = '<p class="text-muted text-center p-3 fs-7">No workspaces yet</p>';
        return;
    }

    state.workspaces.forEach(ws => {
        const isActive = state.currentWorkspaceId === ws.id;
        const item = document.createElement('div');
        item.className = `workspace-item ${isActive ? 'active' : ''}`;
        
        // Wrap selectWorkspace in an arrow function so we can bind it dynamically
        item.onclick = () => selectWorkspace(ws.id);
        
        item.innerHTML = `
            <div class="ws-icon-wrap">
                <i class="fa-solid ${ws.isPrivate ? 'fa-lock' : 'fa-users'}"></i>
            </div>
            <div class="ws-details">
                <span class="ws-title">${escapeHtml(ws.title)}</span>
                <span class="ws-meta">ID: ${ws.id}</span>
            </div>
        `;
        listContainer.appendChild(item);
    });
}

export async function selectWorkspace(workspaceId) {
    state.currentWorkspaceId = workspaceId;
    state.activeNoteId = null;
    state.activeNote = null;
    
    // Switch active states in navigation sidebar
    const wsPersonal = document.getElementById('wsPersonal');
    if (wsPersonal) {
        wsPersonal.className = `workspace-item ${workspaceId === null ? 'active' : ''}`;
    }
    renderWorkspaces();

    // Reset filters
    const searchInput = document.getElementById('noteSearchInput');
    const filterPinned = document.getElementById('filterPinnedOnly');
    const filterDate = document.getElementById('filterCreatedAfter');
    if (searchInput) searchInput.value = '';
    if (filterPinned) filterPinned.checked = false;
    if (filterDate) filterDate.value = '';
    
    // Set UI context based on Workspace
    const listTitle = document.getElementById('notesListTitle');
    const assignedFilter = document.getElementById('assignedToMeFilterContainer');
    
    if (workspaceId === null) {
        if (listTitle) listTitle.textContent = 'Personal Notes';
        if (assignedFilter) assignedFilter.style.display = 'none';
        const gearBtn = document.getElementById('btnWorkspaceSettings');
        if (gearBtn) gearBtn.style.display = 'none';
        state.workspaceRole = 'Owner';
        state.workspaceMembers = [];
        await loadNotes();
    } else {
        const workspace = state.workspaces.find(w => w.id === workspaceId);
        if (listTitle) listTitle.textContent = workspace ? workspace.title : 'Workspace';
        if (assignedFilter) assignedFilter.style.display = 'inline-flex';
        const gearBtn = document.getElementById('btnWorkspaceSettings');
        if (gearBtn) gearBtn.style.display = 'inline-flex';
        
        const filterAssigned = document.getElementById('filterAssignedToMe');
        if (filterAssigned) filterAssigned.checked = false;
        
        // Fetch Role & Members
        try {
            const roleData = await apiRequest(`Workspace/${workspaceId}/my-role`);
            state.workspaceRole = roleData.role;
            
            const members = await apiRequest(`Workspace/${workspaceId}/members`);
            state.workspaceMembers = members;
            
            // Populate assignment selector for tasks
            populateTaskAssignmentSelect(members);
        } catch (e) {
            console.error("Error loading workspace metadata:", e);
            state.workspaceRole = 'Viewer';
        }
        
        await loadNotes();
    }

    renderActiveNote();
    
    // Connect to SignalR groups
    if (state.signalrConnection && state.signalrOnline) {
        // Leave old, join new workspace room
        if (workspaceId) {
            state.signalrConnection.invoke("JoinWorkspaceGroup", workspaceId.toString())
                .catch(err => console.error("SignalR JoinWorkspaceGroup Error:", err));
        }
    }
}

export async function handleCreateWorkspace(e) {
    e.preventDefault();
    const title = document.getElementById('createWsTitle').value.trim();
    const description = document.getElementById('createWsDesc').value.trim();
    const isPrivate = document.getElementById('createWsIsPrivate').checked;

    try {
        const newWs = await apiRequest('Workspace', {
            method: 'POST',
            body: JSON.stringify({ title, description, isPrivate })
        });
        
        showToast('Workspace Created', `Successfully created workspace "${newWs.title}"`, 'success');
        closeAllModals();
        
        // Reset form
        document.getElementById('createWsTitle').value = '';
        document.getElementById('createWsDesc').value = '';
        
        await loadWorkspaces();
        await selectWorkspace(newWs.id);
    } catch (err) {
        showToast('Error Creating Workspace', err.message, 'error');
    }
}

export async function openPublicWorkspacesModal() {
    openModal('publicWorkspacesModal');
    const listEl = document.getElementById('publicWorkspacesList');
    listEl.innerHTML = '<p class="text-center text-muted p-3">Searching public spaces...</p>';

    try {
        const data = await apiRequest('Workspace/public');
        listEl.innerHTML = '';
        
        if (data.length === 0) {
            listEl.innerHTML = '<p class="text-center text-muted p-3">No public spaces found</p>';
            return;
        }

        data.forEach(ws => {
            const isJoined = state.workspaces.some(w => w.id === ws.id);
            const card = document.createElement('div');
            card.className = 'public-space-item';
            
            // Set up button event
            card.innerHTML = `
                <div class="public-space-info">
                    <span class="public-space-title">${escapeHtml(ws.title)}</span>
                    <span class="public-space-desc">${escapeHtml(ws.description || 'No description')}</span>
                </div>
                ${isJoined ? 
                    `<button class="btn btn-outline btn-sm" disabled>Joined</button>` :
                    `<button class="btn btn-primary btn-sm btn-join-space" data-id="${ws.id}">Join</button>`
                }
            `;
            
            const joinBtn = card.querySelector('.btn-join-space');
            if (joinBtn) {
                joinBtn.onclick = () => joinPublicWorkspace(ws.id);
            }
            
            listEl.appendChild(card);
        });
    } catch (err) {
        listEl.innerHTML = `<p class="text-center text-danger p-3">${err.message}</p>`;
    }
}

export async function joinPublicWorkspace(wsId) {
    try {
        await apiRequest(`Workspace/${wsId}/join`, { method: 'POST' });
        showToast('Joined Workspace', 'You have successfully joined the workspace.', 'success');
        closeAllModals();
        await loadWorkspaces();
        await selectWorkspace(wsId);
    } catch (err) {
        showToast('Error', err.message, 'error');
    }
}

export function openWorkspaceSettingsModal() {
    if (!state.currentWorkspaceId) return;
    
    const workspace = state.workspaces.find(w => w.id === state.currentWorkspaceId);
    if (!workspace) return;

    document.getElementById('wsSettingsTitle').textContent = `${workspace.title} - Settings`;
    document.getElementById('myWorkspaceRole').textContent = `My Role: ${state.workspaceRole}`;
    
    // RBAC: Show/Hide elements based on role
    const isAdmin = state.workspaceRole === 'Admin' || state.workspaceRole === 'Owner' || workspace.ownerId === state.user.id;
    
    // Update Details section (Admin only)
    const detailsSection = document.getElementById('wsUpdateDetailsSection');
    if (isAdmin) {
        detailsSection.style.display = 'block';
        document.getElementById('updateWsTitle').value = workspace.title;
        document.getElementById('updateWsDesc').value = workspace.description || '';
    } else {
        detailsSection.style.display = 'none';
    }

    // Add member section (Admin only)
    document.getElementById('wsAddMemberSection').style.display = isAdmin ? 'block' : 'none';
    
    // Delete workspace button (Owner / Admin only)
    document.getElementById('btnDeleteWorkspace').style.display = isAdmin ? 'block' : 'none';

    renderMembersList();
    openModal('workspaceSettingsModal');
}

export function renderMembersList() {
    const listEl = document.getElementById('workspaceMembersList');
    if (!listEl) return;
    listEl.innerHTML = '';

    const isAdmin = state.workspaceRole === 'Admin' || state.workspaceRole === 'Owner';

    state.workspaceMembers.forEach(m => {
        const isSelf = m.userId === state.user.id;
        const item = document.createElement('div');
        item.className = 'member-item';
        
        // Remove button (shown only for admins, can't remove oneself, and can't remove owner unless admin is owner)
        let removeBtnHtml = '';
        if (isAdmin && !isSelf && m.role !== 'Owner') {
            removeBtnHtml = `
                <button class="btn btn-icon-only text-danger btn-remove-member" data-id="${m.userId}" title="Remove Member">
                    <i class="fa-solid fa-user-minus"></i>
                </button>
            `;
        }

        item.innerHTML = `
            <div class="member-item-info">
                <span class="member-item-name">${escapeHtml(m.username)} ${isSelf ? '(You)' : ''}</span>
                <span class="member-item-email">${escapeHtml(m.email)}</span>
            </div>
            <div class="member-item-right">
                <span class="badge-role role-${m.role.toLowerCase()}">${m.role}</span>
                ${removeBtnHtml}
            </div>
        `;

        const rmBtn = item.querySelector('.btn-remove-member');
        if (rmBtn) {
            rmBtn.onclick = () => removeWorkspaceMember(m.userId);
        }

        listEl.appendChild(item);
    });
}

export async function addWorkspaceMember() {
    const input = document.getElementById('addMemberIdentifier');
    const roleSelect = document.getElementById('addMemberRole');
    const userIdentifier = input.value.trim();
    const role = roleSelect.value;

    if (!userIdentifier) {
        showToast('Validation Error', 'Please enter a username or email address.', 'warning');
        return;
    }

    try {
        await apiRequest(`Workspace/${state.currentWorkspaceId}/members`, {
            method: 'POST',
            body: JSON.stringify({ userIdentifier, role })
        });

        showToast('Invitation Sent', `Successfully added member "${userIdentifier}" as ${role}`, 'success');
        input.value = '';
        
        // Reload members list
        const updatedMembers = await apiRequest(`Workspace/${state.currentWorkspaceId}/members`);
        state.workspaceMembers = updatedMembers;
        renderMembersList();
        populateTaskAssignmentSelect(updatedMembers);
    } catch (err) {
        showToast('Invite Failed', err.message, 'error');
    }
}

export async function removeWorkspaceMember(userId) {
    if (confirm('Are you sure you want to remove this member from the workspace?')) {
        try {
            await apiRequest(`Workspace/${state.currentWorkspaceId}/members/${userId}`, { method: 'DELETE' });
            showToast('Member Removed', 'Successfully removed member from workspace.', 'success');
            
            // Reload members list
            const updatedMembers = await apiRequest(`Workspace/${state.currentWorkspaceId}/members`);
            state.workspaceMembers = updatedMembers;
            renderMembersList();
            populateTaskAssignmentSelect(updatedMembers);
        } catch (err) {
            showToast('Error', err.message, 'error');
        }
    }
}

export async function updateWorkspaceDetails() {
    const title = document.getElementById('updateWsTitle').value.trim();
    const description = document.getElementById('updateWsDesc').value.trim();

    if (!title) {
        showToast('Validation Error', 'Title cannot be blank.', 'warning');
        return;
    }

    try {
        await apiRequest(`Workspace/${state.currentWorkspaceId}`, {
            method: 'PUT',
            body: JSON.stringify({ title, description })
        });

        showToast('Updated', 'Workspace settings saved.', 'success');
        
        // Refresh local details
        await loadWorkspaces();
        const ws = state.workspaces.find(w => w.id === state.currentWorkspaceId);
        document.getElementById('notesListTitle').textContent = ws.title;
        closeAllModals();
    } catch (err) {
        showToast('Error', err.message, 'error');
    }
}

export async function leaveCurrentWorkspace() {
    if (confirm('Are you sure you want to leave this workspace? You will lose access to its shared notes.')) {
        try {
            await apiRequest(`Workspace/${state.currentWorkspaceId}/leave`, { method: 'POST' });
            showToast('Left Workspace', 'You have successfully left the workspace.', 'info');
            closeAllModals();
            await loadWorkspaces();
            await selectWorkspace(null); // Return to personal
        } catch (err) {
            showToast('Error', err.message, 'error');
        }
    }
}

export async function deleteCurrentWorkspace() {
    if (confirm('CRITICAL WARNING: Are you sure you want to delete this workspace? This will permanently delete ALL notes and tasks inside it for all members.')) {
        try {
            await apiRequest(`Workspace/${state.currentWorkspaceId}`, { method: 'DELETE' });
            showToast('Workspace Deleted', 'The workspace was successfully deleted.', 'warning');
            closeAllModals();
            await loadWorkspaces();
            await selectWorkspace(null); // Return to personal
        } catch (err) {
            showToast('Error', err.message, 'error');
        }
    }
}
