using Bazaarche.Application.Common;

namespace Bazaarche.Api.Infrastructure;

public static class FormFileExtensions
{
    public static FileUpload ToUpload(this IFormFile file) => new(file.OpenReadStream(), file.FileName, file.Length);
}
