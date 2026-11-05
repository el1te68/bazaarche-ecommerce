namespace Bazaarche.Application.Common;

/// <summary>Base exception for expected business errors. Mapped to an HTTP response by the API middleware.</summary>
public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message = "مورد درخواستی پیدا نشد.") : AppException(message, 404);

public class UnauthorizedAppException(string message = "ابتدا وارد حساب کاربری شوید.") : AppException(message, 401);

public class ForbiddenException(string message = "شما به این بخش دسترسی ندارید.") : AppException(message, 403);
