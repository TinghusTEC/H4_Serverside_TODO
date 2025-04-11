using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;

namespace TodoApp.Components.Account.Pages.Manage
{
    public partial class Roles
    {

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public Roles(RoleManager<IdentityRole> manager, UserManager<ApplicationUser> user)
        {
            _roleManager = manager;
            _userManager = user;
        }

        private IEnumerable<IdentityRole> AllRoles { get; set; } = [];
        protected override async Task OnInitializedAsync()
        {
            AllRoles = [.. _roleManager.Roles];

            await base.OnInitializedAsync();
        }

        private async Task AddRole(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                Console.WriteLine("Role could not be created, value is null or empty");
                return;
            }

            if (AllRoles.Any(x => x.Name == role))
            {
                Console.WriteLine("Any check");
                return;
            }

            IdentityRole newRole = new(role);
            await _roleManager.CreateAsync(newRole);
            AllRoles = AllRoles.Append(newRole);
            StateHasChanged();
        }

        private async Task RemoveRole(IdentityRole role)
        {
            if (role is null)
            {
                return;
            }

            if (!AllRoles.Contains(role))
            {
                return;
            }

            await _roleManager.DeleteAsync(role);
            AllRoles = AllRoles.Where(x => x != role);
            StateHasChanged();
        }
    }
}

