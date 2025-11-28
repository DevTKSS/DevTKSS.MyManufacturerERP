using DevTKSS.MyManufacturerERP.Application.Common.Interfaces;

namespace DevTKSS.MyManufacturerERP.WebApi.Endpoints.Authentication;

public class User : IUser
{
    public string? Id { get; init; }
}