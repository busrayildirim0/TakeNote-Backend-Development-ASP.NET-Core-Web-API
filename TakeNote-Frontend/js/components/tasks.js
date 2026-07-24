// js/components/tasks.js - TakeNote Tasks Checklist Component

import { state } from '../state.js';
import { apiRequest } from '../api.js';
import { showToast, escapeHtml } from './ui.js';

export function renderActiveNoteTasks() {
    const container = document.getElementById('tasksListContainer');
    const badge = document.getElementById('tasksBadgeCount');
    if (!container || !badge) return;
    container.innerHTML = '';

    const tasks = state.activeNote.tasks || [];
    badge.textContent = `${tasks.length} Task${tasks.length === 1 ? '' : 's'}`;

    if (tasks.length === 0) {
        container.innerHTML = '<p class="text-center text-muted fs-7 py-3">No tasks created for this note.</p>';
        return;
    }

    const isViewer = state.workspaceRole === 'Viewer';

    tasks.forEach(task => {
        const isOverdue = task.dueDate && new Date(task.dueDate) < new Date() && !task.isCompleted;
        const formattedDate = task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '';
        
        // Find assigned user display name
        let assigneeName = '';
        if (task.assignedToId) {
            const member = state.workspaceMembers.find(m => m.userId === task.assignedToId);
            assigneeName = member ? member.username : 'Assigned';
        }

        const item = document.createElement('div');
        item.className = `task-item ${task.isCompleted ? 'completed' : ''}`;
        
        item.innerHTML = `
            <div class="task-item-left">
                <label class="task-checkbox-wrapper">
                    <input type="checkbox" class="task-check" data-id="${task.id}" ${task.isCompleted ? 'checked' : ''} 
                        ${isViewer ? 'disabled' : ''}>
                    <span class="custom-checkbox"></span>
                </label>
                <span class="task-desc" title="${escapeHtml(task.description)}">${escapeHtml(task.description)}</span>
                <div class="task-meta-pills">
                    ${formattedDate ? `<span class="task-pill-due ${isOverdue ? 'overdue' : ''}"><i class="fa-regular fa-calendar"></i> ${formattedDate}</span>` : ''}
                    ${assigneeName ? `<span class="task-pill-assignee"><i class="fa-regular fa-user"></i> ${escapeHtml(assigneeName)}</span>` : ''}
                </div>
            </div>
            ${isViewer ? '' : `
                <button class="btn btn-icon-only text-danger btn-delete-task" data-id="${task.id}" title="Delete Task">
                    <i class="fa-regular fa-trash-can" style="font-size: 0.85rem"></i>
                </button>
            `}
        `;

        const checkInput = item.querySelector('.task-check');
        if (checkInput) {
            checkInput.onclick = () => toggleTaskComplete(task.id);
        }

        const deleteBtn = item.querySelector('.btn-delete-task');
        if (deleteBtn) {
            deleteBtn.onclick = () => deleteTask(task.id);
        }

        container.appendChild(item);
    });
}

export function populateTaskAssignmentSelect(members) {
    const select = document.getElementById('newTaskAssignSelect');
    if (!select) return;
    select.innerHTML = '<option value="">Unassigned</option>';
    
    members.forEach(m => {
        const opt = document.createElement('option');
        opt.value = m.userId;
        opt.textContent = m.username;
        select.appendChild(opt);
    });
}

export async function handleAddTask(e) {
    e.preventDefault();
    const descInput = document.getElementById('newTaskDesc');
    const dateInput = document.getElementById('newTaskDueDate');
    const assignSelect = document.getElementById('newTaskAssignSelect');

    const description = descInput.value.trim();
    const dueDate = dateInput.value ? new Date(dateInput.value).toISOString() : null;
    const assignedToId = assignSelect.value || null;

    if (!description) return;

    try {
        const newTask = await apiRequest('TaskItem', {
            method: 'POST',
            body: JSON.stringify({
                noteId: state.activeNoteId,
                description,
                dueDate,
                assignedToId
            })
        });

        // Add to active note lists locally
        if (!state.activeNote.tasks) {
            state.activeNote.tasks = [];
        }
        state.activeNote.tasks.push(newTask);
        
        // Notify others via SignalR if inside workspace
        if (state.currentWorkspaceId && state.signalrConnection && state.signalrOnline) {
            state.signalrConnection.invoke("NotifyTaskCompleted", state.currentWorkspaceId.toString(), `created task: "${description}"`)
                .catch(e => console.log(e));
        }

        renderActiveNoteTasks();

        // Reset fields
        descInput.value = '';
        dateInput.value = '';
        assignSelect.value = '';
    } catch (err) {
        showToast('Error creating task', err.message, 'error');
    }
}

export async function toggleTaskComplete(taskId) {
    try {
        await apiRequest(`TaskItem/${taskId}/toggle`, { method: 'PATCH' });
        
        const task = state.activeNote.tasks.find(t => t.id === taskId);
        if (task) {
            task.isCompleted = !task.isCompleted;
            
            // Notify other members via SignalR
            if (task.isCompleted && state.currentWorkspaceId && state.signalrConnection && state.signalrOnline) {
                state.signalrConnection.invoke("NotifyTaskCompleted", state.currentWorkspaceId.toString(), `completed task: "${task.description}"`)
                    .catch(e => console.log(e));
            }
        }
        
        renderActiveNoteTasks();
    } catch (err) {
        showToast('Error updating task', err.message, 'error');
    }
}

export async function deleteTask(taskId) {
    if (confirm('Delete this task?')) {
        try {
            await apiRequest(`TaskItem/${taskId}`, { method: 'DELETE' });
            
            state.activeNote.tasks = state.activeNote.tasks.filter(t => t.id !== taskId);
            renderActiveNoteTasks();
        } catch (err) {
            showToast('Error deleting task', err.message, 'error');
        }
    }
}
