// js/state.js - TakeNote Central Client State

export const state = {
    apiUrl: 'http://localhost:5159/api', // Default Local HTTP Url
    hubUrl: 'http://localhost:5159/collaborationHub',
    token: localStorage.getItem('tn_token') || '',
    user: null,
    workspaces: [],
    currentWorkspaceId: null, // null for Personal Notes
    workspaceRole: 'Owner',   // 'Owner', 'Admin', 'Editor', 'Viewer'
    workspaceMembers: [],     // Active workspace members for dropdown assignment
    notes: [],
    activeNoteId: null,
    activeNote: null,
    signalrConnection: null,
    typingTimer: null,
    saveTimer: null,
    lastTypingSent: 0,
    signalrOnline: false
};
