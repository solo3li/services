using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ServicesApp.Infrastructure;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null) return Task.CompletedTask;

        // Check if user has the specific permission claim
        var hasPermission = context.User.Claims.Any(c => c.Type == "Permission" && c.Value == requirement.Permission);
        
        // Super admin bypass (optional: admin@services.io always has all)
        var userEmail = context.User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == "admin@services.io") hasPermission = true;

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    public AuthorizePermissionAttribute(string permission) : base(permission)
    {
    }
}

// Policy Provider to dynamically create policies for any permission string
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null) return policy;

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
