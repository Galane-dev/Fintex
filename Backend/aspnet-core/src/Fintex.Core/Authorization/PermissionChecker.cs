using Abp.Authorization;
using Fintex.Authorization.Roles;
using Fintex.Authorization.Users;

namespace Fintex.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
