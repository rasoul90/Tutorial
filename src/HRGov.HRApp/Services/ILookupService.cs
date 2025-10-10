using HRGov.HRApp.ViewModels;

namespace HRGov.HRApp.Services;

public interface ILookupService
{
    Task<LookupCollections> GetLookupsAsync();
}
