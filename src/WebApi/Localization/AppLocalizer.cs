using System.Reflection;
using EduCare.Application.Interfaces.Localization;
using Microsoft.Extensions.Localization;

namespace EduCare.CoreApi.Localization;

public class AppLocalizer : IAppLocalizer
{
    private readonly IStringLocalizer _localizer;

    public AppLocalizer(IStringLocalizerFactory factory)
    {
        // SharedResource is just a marker class
        //var type = typeof(SharedResources);
        //_localizer = factory.Create(type);
        _localizer = factory.Create("SharedResources", Assembly.GetExecutingAssembly().GetName().Name);
    }

    public string this[string key] => _localizer[key];

    public string GetString(string key, params object[] arguments)
    {
        var value = _localizer[key];
        return arguments.Length == 0 ? value.Value : string.Format(value.Value, arguments);
    }
}