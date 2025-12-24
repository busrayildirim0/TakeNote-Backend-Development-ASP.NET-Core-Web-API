using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TakeNote.Core.Entities;
using TakeNote.Core.Interfaces;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.Service.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkspaceService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;

        public WorkspaceService(
            IUnitOfWork unitOfWork,
            ILogger<WorkspaceService> logger,
            UserManager<User> userManager,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // CREATE
        public async Task<WorkspaceDto> CreateAsync(WorkspaceCreateDto dto, Guid userId)
        {
            _logger.LogInformation("Creating workspace '{Title}' for user {UserId}", dto.Title, userId);

            var workspace = new Workspace
            {
                Title = dto.Title,
                Description = dto.Description,
                IsPrivate = dto.IsPrivate,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                Members = new List<WorkspaceMember>
                {
                    new WorkspaceMember { UserId = userId, Role = "Admin", JoinedAt = DateTime.UtcNow }
                }
            };

            await _unitOfWork.Workspaces.AddAsync(workspace);
            await _unitOfWork.CompleteAsync();

            return MapToDto(workspace);
        }

        // GET BY ID
        public async Task<WorkspaceDto> GetByIdAsync(int id, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) throw new Exception("Workspace not found");

            bool isMember = workspace.OwnerId == userId ||
                (await _unitOfWork.Workspaces.ListAsync(w => w.Id == id && w.Members.Any(m => m.UserId == userId))).Any();

            if (workspace.IsPrivate && !isMember)
                throw new UnauthorizedAccessException("You do not have access to this private workspace.");

            return MapToDto(workspace);
        }

        // GET AVAILABLE
        public async Task<IEnumerable<WorkspaceDto>> GetAvailableWorkspacesAsync(Guid userId)
        {
            var workspaces = await _unitOfWork.Workspaces.ListAsync(w =>
                w.OwnerId == userId || w.Members.Any(m => m.UserId == userId));
            return workspaces.Select(MapToDto);
        }

        // GET PUBLIC (Üye olmayanlar için)
        public async Task<IEnumerable<WorkspaceDto>> GetPublicWorkspacesAsync(Guid userId)
        {
            var allPublic = await _unitOfWork.Workspaces.ListAsync(w => !w.IsPrivate);
            var result = new List<WorkspaceDto>();

            foreach (var workspace in allPublic)
            {
                if (workspace.OwnerId != userId)
                {
                    var isMember = (await _unitOfWork.Workspaces.ListAsync(w =>
                        w.Id == workspace.Id && w.Members.Any(m => m.UserId == userId))).Any();

                    if (!isMember) result.Add(MapToDto(workspace));
                }
            }
            return result;
        }

        // ADD MEMBER - USERNAME/EMAIL İLE
        public async Task AddMemberAsync(int workspaceId, AddMemberDto dto, Guid currentUserId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.OwnerId != currentUserId)
                throw new UnauthorizedAccessException("Only the workspace admin can add members.");

            // USERNAME veya EMAIL İLE KULLANICI BUL
            User? userToAdd = null;

            // Önce userId varsa direkt bul
            if (dto.UserId != Guid.Empty)
            {
                userToAdd = await _userManager.FindByIdAsync(dto.UserId.ToString());
            }
            // Yoksa UserIdentifier'a bak (Username veya Email olabilir)
            else if (!string.IsNullOrEmpty(dto.UserIdentifier))
            {
                // Email formatında mı kontrol et
                if (dto.UserIdentifier.Contains("@"))
                {
                    userToAdd = await _userManager.FindByEmailAsync(dto.UserIdentifier);
                }
                else
                {
                    userToAdd = await _userManager.FindByNameAsync(dto.UserIdentifier);
                }
            }

            if (userToAdd == null)
                throw new Exception("Kullanıcı bulunamadı. Lütfen doğru username veya email giriniz.");

            // Zaten üye mi?
            var isAlreadyMember = workspace.Members.Any(m => m.UserId == userToAdd.Id) || workspace.OwnerId == userToAdd.Id;
            if (isAlreadyMember)
                throw new Exception("Bu kullanıcı zaten alan üyesi.");

            // Ekle
            workspace.Members.Add(new WorkspaceMember
            {
                UserId = userToAdd.Id,
                WorkspaceId = workspaceId,
                Role = dto.Role ?? "Viewer",
                JoinedAt = DateTime.UtcNow
            });

            _unitOfWork.Workspaces.Update(workspace);
            await _unitOfWork.CompleteAsync();

            await _notificationService.NotifyMemberAddedAsync(workspaceId, userToAdd.Id);
            _logger.LogInformation("User {NewUserId} added to workspace {WsId}", userToAdd.Id, workspaceId);
        }

        // REMOVE MEMBER
        public async Task RemoveMemberAsync(int workspaceId, Guid memberIdToRemove, Guid currentUserId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.OwnerId != currentUserId)
                throw new UnauthorizedAccessException("Only the workspace admin can remove members.");

            if (memberIdToRemove == currentUserId)
                throw new Exception("You cannot remove yourself. Use 'Leave' instead.");

            var member = workspace.Members.FirstOrDefault(m => m.UserId == memberIdToRemove);
            if (member == null) throw new Exception("Member not found in workspace");

            workspace.Members.Remove(member);
            _unitOfWork.Workspaces.Update(workspace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {MemberId} removed from workspace {WsId}", memberIdToRemove, workspaceId);
        }

        // JOIN
        public async Task JoinAsync(int workspaceId, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.IsPrivate)
                throw new UnauthorizedAccessException("Cannot join a private workspace without invitation.");

            if (workspace.OwnerId == userId)
                throw new Exception("You are already the owner of this workspace.");

            var isMember = workspace.Members.Any(m => m.UserId == userId);
            if (isMember) throw new Exception("You are already a member of this workspace.");

            workspace.Members.Add(new WorkspaceMember
            {
                UserId = userId,
                WorkspaceId = workspaceId,
                Role = "Viewer",
                JoinedAt = DateTime.UtcNow
            });

            _unitOfWork.Workspaces.Update(workspace);
            await _unitOfWork.CompleteAsync();

            await _notificationService.NotifyMemberAddedAsync(workspaceId, userId);
            _logger.LogInformation("User {UserId} joined workspace {WorkspaceId}", userId, workspaceId);
        }

        // LEAVE
        public async Task LeaveAsync(int workspaceId, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.OwnerId == userId)
                throw new Exception("Owner cannot leave. Delete workspace instead.");

            var member = workspace.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) throw new Exception("You are not a member of this workspace");

            workspace.Members.Remove(member);
            _unitOfWork.Workspaces.Update(workspace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {UserId} left workspace {WsId}", userId, workspaceId);
        }

        // UPDATE
        public async Task UpdateAsync(int id, WorkspaceUpdateDto dto, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.OwnerId != userId)
                throw new UnauthorizedAccessException("Only owner can update");

            if (dto.Title != null) workspace.Title = dto.Title;
            if (dto.Description != null) workspace.Description = dto.Description;
            if (dto.IsPrivate.HasValue) workspace.IsPrivate = dto.IsPrivate.Value;

            _unitOfWork.Workspaces.Update(workspace);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Workspace updated: {WorkspaceId}", id);
        }

        // DELETE
        public async Task DeleteAsync(int id, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) return;

            if (workspace.OwnerId != userId)
                throw new UnauthorizedAccessException("Only owner can delete");

            _unitOfWork.Workspaces.Delete(workspace);
            await _unitOfWork.CompleteAsync();
        }

        // GET MEMBERS
        public async Task<IEnumerable<WorkspaceMemberDto>> GetMembersAsync(int workspaceId, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            bool isMember = workspace.OwnerId == userId || workspace.Members.Any(m => m.UserId == userId);
            if (workspace.IsPrivate && !isMember)
                throw new UnauthorizedAccessException("Access denied");

            var membersList = new List<WorkspaceMemberDto>();

            // Owner ekle
            var owner = await _userManager.FindByIdAsync(workspace.OwnerId.ToString());
            if (owner != null)
            {
                membersList.Add(new WorkspaceMemberDto
                {
                    UserId = workspace.OwnerId,
                    Username = owner.UserName ?? "Unknown",
                    Email = owner.Email ?? "",
                    Role = "Admin",
                    JoinedAt = workspace.CreatedAt
                });
            }

            // Diğer üyeler
            foreach (var member in workspace.Members.Where(m => m.UserId != workspace.OwnerId))
            {
                membersList.Add(new WorkspaceMemberDto
                {
                    UserId = member.UserId,
                    Username = member.User.UserName ?? "Unknown",
                    Email = member.User.Email ?? "",
                    Role = member.Role,
                    JoinedAt = member.JoinedAt
                });
            }

            return membersList;
        }

        // GET USER ROLE
        public async Task<string> GetUserRoleAsync(int workspaceId, Guid userId)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdWithMembersAsync(workspaceId);
            if (workspace == null) throw new Exception("Workspace not found");

            if (workspace.OwnerId == userId) return "Admin";

            var member = workspace.Members.FirstOrDefault(m => m.UserId == userId);
            return member?.Role ?? "None";
        }

        private static WorkspaceDto MapToDto(Workspace w)
        {
            return new WorkspaceDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                IsPrivate = w.IsPrivate,
                CreatedAt = w.CreatedAt,
                OwnerId = w.OwnerId
            };
        }
    }
}