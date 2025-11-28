using Microsoft.AspNetCore.SignalR;

namespace TakeNote.API.Hubs
{
    //
    public class CollaborationHub : Hub
    {
        // Kullanıcı bir notu açtığında o notun "Odasına" (Group) katılır
        public async Task JoinNoteGroup(string noteId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, noteId);
        }

        public async Task LeaveNoteGroup(string noteId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, noteId);
        }
    }
}