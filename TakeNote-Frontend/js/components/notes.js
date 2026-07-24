// js/components/notes.js - TakeNote Notes & Editor Component

import { state } from '../state.js';
import { apiRequest } from '../api.js';
import { showToast, escapeHtml } from './ui.js';
import { renderActiveNoteTasks } from './tasks.js';

export async function loadNotes() {
    try {
        const listContainer = document.getElementById('notesListContainer');
        if (listContainer) {
            listContainer.innerHTML = `
                <div class="skeleton-loader">
                    <div class="skeleton-line"></div>
                    <div class="skeleton-line"></div>
                    <div class="skeleton-line"></div>
                </div>`;
        }

        let notesData = [];
        if (state.currentWorkspaceId === null) {
            notesData = await apiRequest('Note/personal');
        } else {
            notesData = await apiRequest(`Note/workspace/${state.currentWorkspaceId}`);
        }

        state.notes = sortNotes(notesData);
        renderNotesList();
    } catch (err) {
        showToast('Error Loading Notes', err.message, 'error');
    }
}

export function sortNotes(notesList) {
    return [...notesList].sort((a, b) => {
        // Pins first
        if (a.isPinned && !b.isPinned) return -1;
        if (!a.isPinned && b.isPinned) return 1;
        // Then newest created/updated first
        return new Date(b.updatedAt || b.createdAt) - new Date(a.updatedAt || a.createdAt);
    });
}

export function renderNotesList() {
    const listContainer = document.getElementById('notesListContainer');
    if (!listContainer) return;
    listContainer.innerHTML = '';

    if (state.notes.length === 0) {
        listContainer.innerHTML = `
            <div class="empty-state">
                <i class="fa-regular fa-folder-open"></i>
                <p>No notes found</p>
            </div>`;
        return;
    }

    state.notes.forEach(note => {
        const isActive = state.activeNoteId === note.id;
        const card = document.createElement('div');
        card.className = `note-card ${isActive ? 'active' : ''}`;
        
        card.onclick = () => selectNote(note.id);

        const date = new Date(note.updatedAt || note.createdAt).toLocaleDateString();
        
        let tagsHtml = '';
        if (note.tags && note.tags.length > 0) {
            tagsHtml = `<div class="note-card-tags">` + 
                note.tags.map(t => `<span class="tag-pill">${escapeHtml(t)}</span>`).join('') + 
                `</div>`;
        }

        card.innerHTML = `
            <div class="note-card-header">
                <span class="note-card-title">${escapeHtml(note.title || 'Untitled Note')}</span>
                <i class="fa-solid fa-thumbtack note-card-pin ${note.isPinned ? 'pinned' : ''}"></i>
            </div>
            <div class="note-card-body">${escapeHtml(note.content || 'Empty note content...')}</div>
            <div class="note-card-footer">
                <span>${date}</span>
                ${tagsHtml}
            </div>
        `;
        listContainer.appendChild(card);
    });
}

export async function selectNote(noteId) {
    // Clear autosave timers
    clearTimeout(state.saveTimer);
    
    if (state.activeNoteId && state.signalrConnection && state.signalrOnline) {
        // Leave old note hub group
        state.signalrConnection.invoke("LeaveNoteGroup", state.activeNoteId.toString())
            .catch(err => console.log("SignalR LeaveNoteGroup Error:", err));
    }

    state.activeNoteId = noteId;
    
    if (noteId === null) {
        state.activeNote = null;
        renderActiveNote();
        return;
    }

    try {
        const note = await apiRequest(`Note/${noteId}`);
        state.activeNote = note;
        renderActiveNote();

        // Connect to SignalR note room for live typing
        if (state.signalrConnection && state.signalrOnline) {
            state.signalrConnection.invoke("JoinNoteGroup", noteId.toString())
                .catch(err => console.log("SignalR JoinNoteGroup Error:", err));
        }
    } catch (err) {
        showToast('Error Opening Note', err.message, 'error');
        state.activeNoteId = null;
        state.activeNote = null;
        renderActiveNote();
    }
}

export function renderActiveNote() {
    const activeView = document.getElementById('noteActiveView');
    const emptyView = document.getElementById('noteEmptyView');
    if (!activeView || !emptyView) return;

    if (!state.activeNote) {
        activeView.classList.add('hidden');
        emptyView.classList.remove('hidden');
        return;
    }

    activeView.classList.remove('hidden');
    emptyView.classList.add('hidden');

    const note = state.activeNote;

    // Pin indicator icon
    const pinBtn = document.getElementById('btnPinNote');
    if (pinBtn) {
        if (note.isPinned) {
            pinBtn.className = 'btn btn-icon-only text-warning';
            pinBtn.innerHTML = '<i class="fa-solid fa-bookmark"></i>';
            pinBtn.title = 'Unpin Note';
        } else {
            pinBtn.className = 'btn btn-icon-only text-muted';
            pinBtn.innerHTML = '<i class="fa-regular fa-bookmark"></i>';
            pinBtn.title = 'Pin Note';
        }
    }

    // Owner display
    const ownerDisplay = document.getElementById('noteOwnerUsername');
    if (ownerDisplay) {
        ownerDisplay.textContent = note.createdByUsername || 'Unknown';
    }

    // Enable/Disable based on roles (Viewer cannot edit title/content, add tasks or delete notes)
    const isViewer = state.workspaceRole === 'Viewer';
    const titleInput = document.getElementById('editorNoteTitle');
    const contentTextarea = document.getElementById('editorNoteContent');
    const deleteBtn = document.getElementById('btnDeleteNote');
    const taskForm = document.getElementById('newTaskForm');

    if (titleInput) {
        titleInput.value = note.title || '';
        titleInput.disabled = isViewer;
    }
    
    if (contentTextarea) {
        contentTextarea.value = note.content || '';
        contentTextarea.disabled = isViewer;
    }

    if (deleteBtn) deleteBtn.style.display = isViewer ? 'none' : 'inline-flex';
    if (taskForm) taskForm.style.display = isViewer ? 'none' : 'block';
    
    // Member Assignment dropdown
    const assignSelect = document.getElementById('newTaskAssignSelect');
    if (assignSelect) {
        if (state.currentWorkspaceId !== null && !isViewer) {
            assignSelect.style.display = 'block';
        } else {
            assignSelect.style.display = 'none';
        }
    }

    // Render Tags
    renderActiveNoteTags();

    // Render Tasks
    renderActiveNoteTasks();
}

export function renderActiveNoteTags() {
    const container = document.getElementById('editorTagsContainer');
    if (!container) return;
    container.innerHTML = '';
    
    const note = state.activeNote;
    const isViewer = state.workspaceRole === 'Viewer';

    if (note.tags && note.tags.length > 0) {
        note.tags.forEach(t => {
            const pill = document.createElement('span');
            pill.className = 'tag-pill-interactive';
            
            // Render interactive tags with clean listener
            pill.innerHTML = `
                <span>#${escapeHtml(t)}</span>
                ${isViewer ? '' : `<button class="btn-remove-tag" data-tag="${escapeHtml(t)}"><i class="fa-solid fa-xmark"></i></button>`}
            `;
            
            const btn = pill.querySelector('.btn-remove-tag');
            if (btn) {
                btn.onclick = () => removeActiveNoteTag(t);
            }
            
            container.appendChild(pill);
        });
    } else {
        container.innerHTML = '<span class="text-muted fs-7">No tags</span>';
    }
}

// --- ACTIVE NOTE INPUT HANDLERS (AUTO-SAVE) ---
export function handleNoteTitleInput() {
    if (state.workspaceRole === 'Viewer') return;
    triggerTypingIndicator();
    scheduleAutoSave();
}

export function handleNoteContentInput() {
    if (state.workspaceRole === 'Viewer') return;
    triggerTypingIndicator();
    scheduleAutoSave();
}

export function triggerTypingIndicator() {
    const now = Date.now();
    // Throttle typing indicator requests to once every 2 seconds
    if (now - state.lastTypingSent > 2000) {
        state.lastTypingSent = now;
        if (state.signalrConnection && state.signalrOnline && state.activeNoteId) {
            state.signalrConnection.invoke("SendTypingIndicator", state.activeNoteId.toString())
                .catch(err => console.log("SignalR SendTypingIndicator error:", err));
        }
    }
}

export function scheduleAutoSave() {
    clearTimeout(state.saveTimer);
    state.saveTimer = setTimeout(async () => {
        const title = document.getElementById('editorNoteTitle').value;
        const content = document.getElementById('editorNoteContent').value;
        
        try {
            await apiRequest(`Note/${state.activeNoteId}`, {
                method: 'PUT',
                body: JSON.stringify({
                    title: title,
                    content: content,
                    isPinned: state.activeNote.isPinned,
                    tags: state.activeNote.tags
                })
            });

            // Update state
            state.activeNote.title = title;
            state.activeNote.content = content;
            
            // Sync with sidebar notes list item
            const item = state.notes.find(n => n.id === state.activeNoteId);
            if (item) {
                item.title = title;
                item.content = content;
                item.updatedAt = new Date().toISOString();
                renderNotesList();
            }
        } catch (e) {
            console.error("Auto-save failed:", e);
        }
    }, 1000); // Save 1 second after typing stops
}

export async function handleCreateNoteBtnClick() {
    try {
        const body = {
            title: 'New Note',
            content: '',
            workspaceId: state.currentWorkspaceId,
            isPinned: false,
            tags: []
        };

        const newNote = await apiRequest('Note', {
            method: 'POST',
            body: JSON.stringify(body)
        });

        state.notes.unshift(newNote);
        state.notes = sortNotes(state.notes);
        renderNotesList();
        await selectNote(newNote.id);
        
        showToast('Note Created', `Created note: "${newNote.title}"`, 'success');
    } catch (err) {
        showToast('Error', err.message, 'error');
    }
}

export async function deleteActiveNote() {
    if (confirm('Are you sure you want to delete this note?')) {
        const noteId = state.activeNoteId;
        try {
            await apiRequest(`Note/${noteId}`, { method: 'DELETE' });
            
            showToast('Note Deleted', 'The note was successfully deleted.', 'info');
            
            state.notes = state.notes.filter(n => n.id !== noteId);
            renderNotesList();
            await selectNote(null);
        } catch (err) {
            showToast('Error deleting note', err.message, 'error');
        }
    }
}

export async function toggleActiveNotePin() {
    const isPinned = !state.activeNote.isPinned;
    try {
        await apiRequest(`Note/${state.activeNoteId}`, {
            method: 'PUT',
            body: JSON.stringify({
                title: state.activeNote.title,
                content: state.activeNote.content,
                isPinned: isPinned,
                tags: state.activeNote.tags
            })
        });

        state.activeNote.isPinned = isPinned;
        
        // Sync sidebar
        const note = state.notes.find(n => n.id === state.activeNoteId);
        if (note) {
            note.isPinned = isPinned;
            state.notes = sortNotes(state.notes);
            renderNotesList();
        }

        renderActiveNote();
    } catch (err) {
        showToast('Error', err.message, 'error');
    }
}

// --- TAGS OPERATIONS ---
export function handleNewTagKeydown(e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        addTagFromInput();
    }
}

export function addTagFromInput() {
    const input = document.getElementById('newTagInput');
    if (!input) return;
    const tag = input.value.trim().replace(/^#/, ''); // Remove leading hash if typed

    if (!tag) return;
    if (state.activeNote.tags.includes(tag)) {
        input.value = '';
        return;
    }

    state.activeNote.tags.push(tag);
    input.value = '';
    
    saveNoteTags();
}

export function removeActiveNoteTag(tag) {
    state.activeNote.tags = state.activeNote.tags.filter(t => t !== tag);
    saveNoteTags();
}

async function saveNoteTags() {
    try {
        await apiRequest(`Note/${state.activeNoteId}`, {
            method: 'PUT',
            body: JSON.stringify({
                title: state.activeNote.title,
                content: state.activeNote.content,
                isPinned: state.activeNote.isPinned,
                tags: state.activeNote.tags
            })
        });

        // Sync list
        const note = state.notes.find(n => n.id === state.activeNoteId);
        if (note) {
            note.tags = state.activeNote.tags;
            renderNotesList();
        }

        renderActiveNoteTags();
    } catch (err) {
        showToast('Error updating tags', err.message, 'error');
    }
}

// --- SEARCH & FILTERS ---
let searchDebounceTimer = null;
export function triggerSearch() {
    clearTimeout(searchDebounceTimer);
    searchDebounceTimer = setTimeout(async () => {
        const query = document.getElementById('noteSearchInput').value.trim();
        const pinnedOnly = document.getElementById('filterPinnedOnly').checked;
        const createdAfter = document.getElementById('filterCreatedAfter').value;
        const assignedToMe = document.getElementById('filterAssignedToMe') ? document.getElementById('filterAssignedToMe').checked : false;

        // Build query string
        const params = new URLSearchParams();
        if (query) params.append('query', query);
        if (pinnedOnly) params.append('pinnedOnly', 'true');
        if (createdAfter) params.append('createdAfter', new Date(createdAfter).toISOString());

        try {
            let searchResults = [];
            if (state.currentWorkspaceId === null) {
                searchResults = await apiRequest(`Note/personal/search?${params.toString()}`);
            } else {
                if (assignedToMe) params.append('assignedToMe', 'true');
                searchResults = await apiRequest(`Note/workspace/${state.currentWorkspaceId}/search?${params.toString()}`);
            }

            state.notes = sortNotes(searchResults);
            renderNotesList();
        } catch (err) {
            console.error("Search failed:", err);
        }
    }, 400);
}

export function clearDateFilter() {
    const filterCreated = document.getElementById('filterCreatedAfter');
    if (filterCreated) filterCreated.value = '';
    triggerSearch();
}
