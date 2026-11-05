using Bazaarche.Application.Common;
using Bazaarche.Infrastructure.Security;

namespace Bazaarche.Api.Infrastructure;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public int? UserId =>
        int.TryParse(accessor.HttpContext?.User.FindFirst(AppClaims.UserId)?.Value, out var id) ? id : null;

    public bool IsAdmin => accessor.HttpContext?.User.IsInRole("Admin") ?? false;
}
