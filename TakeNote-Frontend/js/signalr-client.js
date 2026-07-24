// js/signalr-client.js - TakeNote SignalR Client Integration

import { state } from './state.js';
import { showToast } from './components/ui.js';
import { sortNotes, renderNotesList, renderActiveNoteTags, renderActiveNote } from './components/notes.js';
import { renderActiveNoteTasks } from './components/tasks.js';

export function initSignalR() {
    if (!state.token) return;

    console.log("Setting up SignalR client connection...");
    
    // If connection exists, stop it first
    if (state.signalrConnection) {
        state.signalrConnection.stop();
    }

    // Set connection using global signalR object loaded from CDN
    state.signalrConnection = new window.signalR.HubConnectionBuilder()
        .withUrl(state.hubUrl, {
            accessTokenFactory: () => state.token
        })
        .withAutomaticReconnect()
        .build();

    // REGISTER EVENT HANDLERS
    
    // Note created within active Workspace
    state.signalrConnection.on("ReceiveNewNote", (note) => {
        if (note.workspaceId === state.currentWorkspaceId) {
            showToast('New Note Alert', `"${note.title}" was added dynamically by workspace partner!`, 'info');
            // Add note to local list if it does not exist
            if (!state.notes.some(n => n.id === note.id)) {
                state.notes.unshift(note);
                state.notes = sortNotes(state.notes);
                renderNotesList();
            }
        }
    });

    // Note metadata updated
    state.signalrConnection.on("ReceiveNoteUpdate", (id, dto) => {
        const note = state.notes.find(n => n.id === id);
        if (note) {
            if (dto.title) note.title = dto.title;
            if (dto.content !== undefined) note.content = dto.content;
            if (dto.isPinned !== undefined) note.isPinned = dto.isPinned;
            if (dto.tags) note.tags = dto.tags;
            
            note.updatedAt = new Date().toISOString();
            state.notes = sortNotes(state.notes);
            renderNotesList();
        }

        // If selected active note is updated, sync editor safely
        if (state.activeNoteId === id && state.activeNote) {
            const titleInput = document.getElementById('editorNoteTitle');
            const contentTextarea = document.getElementById('editorNoteContent');
            
            // Only update editor if user does NOT have focus (to prevent cursor jumps)
            if (document.activeElement !== titleInput && dto.title) {
                titleInput.value = dto.title;
                state.activeNote.title = dto.title;
            }
            if (document.activeElement !== contentTextarea && dto.content !== undefined) {
                contentTextarea.value = dto.content;
                state.activeNote.content = dto.content;
            }
            
            if (dto.isPinned !== undefined) {
                state.activeNote.isPinned = dto.isPinned;
            }
            if (dto.tags) {
                state.activeNote.tags = dto.tags;
                renderActiveNoteTags();
            }
        }
    });

    // Note deleted
    state.signalrConnection.on("ReceiveNoteDelete", (id) => {
        const deletedNote = state.notes.find(n => n.id === id);
        if (deletedNote) {
            showToast('Note Removed', `Note "${deletedNote.title}" was deleted.`, 'warning');
            state.notes = state.notes.filter(n => n.id !== id);
            renderNotesList();
        }
        
        if (state.activeNoteId === id) {
            state.activeNoteId = null;
            state.activeNote = null;
            renderActiveNote();
        }
    });

    // User is Typing indicator
    state.signalrConnection.on("UserTyping", (username, noteId) => {
        if (state.activeNoteId && state.activeNoteId.toString() === noteId.toString()) {
            showTypingBubble(username);
        }
    });

    // User joined workspace
    state.signalrConnection.on("UserJoinedWorkspace", (username, workspaceId) => {
        if (state.currentWorkspaceId && state.currentWorkspaceId.toString() === workspaceId.toString()) {
            showToast('Member Joined', `${username} joined workspace room`, 'info');
        }
    });

    // User left workspace
    state.signalrConnection.on("UserLeftWorkspace", (username, workspaceId) => {
        if (state.currentWorkspaceId && state.currentWorkspaceId.toString() === workspaceId.toString()) {
            showToast('Member Left', `${username} left workspace room`, 'warning');
        }
    });

    // Task completed notifications
    state.signalrConnection.on("TaskCompleted", (taskTitle, username) => {
        showToast('Task Update', `${username} ${taskTitle}`, 'success');
    });

    // START SIGNALR CONNECTION
    state.signalrConnection.start()
        .then(() => {
            console.log("SignalR Connection online! 🟢");
            setSignalrStatus(true);
            
            // Join workspace hub room if active
            if (state.currentWorkspaceId) {
                state.signalrConnection.invoke("JoinWorkspaceGroup", state.currentWorkspaceId.toString())
                    .catch(err => console.error("SignalR Join Room Error:", err));
            }
        })
        .catch(err => {
            console.error("SignalR connection failed to start:", err);
            setSignalrStatus(false);
        });
}

export function reconnectSignalR() {
    if (state.signalrConnection) {
        state.signalrConnection.stop()
            .then(() => initSignalR())
            .catch(() => initSignalR());
    } else {
        initSignalR();
    }
}

export function setSignalrStatus(online) {
    state.signalrOnline = online;
    const ind = document.getElementById('connectionIndicator');
    if (!ind) return;
    
    if (online) {
        ind.className = 'conn-indicator connected';
        ind.querySelector('.text').textContent = 'SignalR Online';
    } else {
        ind.className = 'conn-indicator disconnected';
        ind.querySelector('.text').textContent = 'SignalR Offline';
    }
}

// Display typing indicator bubble
export function showTypingBubble(username) {
    const bubble = document.getElementById('liveTypingIndicator');
    const textEl = document.getElementById('liveTypingText');
    if (!bubble || !textEl) return;
    
    textEl.textContent = `${username} is typing...`;
    bubble.classList.remove('hidden');
    
    // Auto hide bubble after 3 seconds of inactivity
    clearTimeout(state.typingTimer);
    state.typingTimer = setTimeout(() => {
        bubble.classList.add('hidden');
    }, 3000);
}
