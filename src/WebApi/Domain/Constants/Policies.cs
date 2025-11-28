namespace DevTKSS.MyManufacturerERP.Application.Common.Security;

/// <summary>
/// Defines the authorization policies used in the application.
/// </summary>
public static class Policies
{
    public const string CanPurge = "CanPurge";
    public const string CanDelete = "CanDelete";
    public const string CanEdit = "CanEdit";
    public const string CanView = "CanView";
}
