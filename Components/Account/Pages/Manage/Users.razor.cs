using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Data;

namespace TodoApp.Components.Account.Pages.Manage
{
    public partial class Users
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public Users(RoleManager<IdentityRole> manager, UserManager<ApplicationUser> user)
        {
            _roleManager = manager;
            _userManager = user;
        }

        private Dictionary<string, ApplicationUser> UsersById { get; set; } = [];
        private Dictionary<string, IEnumerable<string>> UsersRoles { get; set; } = [];
        private Dictionary<string, string> SelectedRoles { get; set; } = [];
        private List<IdentityRole> Roles { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            await foreach (var user in _userManager.Users.AsAsyncEnumerable())
            {
                var roleNames = await _userManager.GetRolesAsync(user);
                UsersById[user.Id] = user;
                UsersRoles[user.Id] = roleNames;
                SelectedRoles[user.Id] = string.Empty;
            }

            Roles = await _roleManager.Roles.ToListAsync();

            await base.OnInitializedAsync();
        }

        private async Task AddToRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return;
            }

            if (!UsersById.TryGetValue(userId, out var user))
            {
                return;
            }

            if (UsersRoles[userId].Contains(role))
            {
                return;
            }

            await _userManager.AddToRoleAsync(user, role);
            UsersRoles[userId] = UsersRoles[userId].Append(role);
            SelectedRoles[userId] = string.Empty; // Reset selected role
            StateHasChanged();
        }

        private async Task RemoveFromRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return;
            }

            if (!UsersById.TryGetValue(userId, out var user))
            {
                return;
            }

            if (!UsersRoles[userId].Contains(role))
            {
                return;
            }

            await _userManager.RemoveFromRoleAsync(user, role);
            UsersRoles[userId] = UsersRoles[userId].Where(x => x != role);
            StateHasChanged();
        }
    }
}
