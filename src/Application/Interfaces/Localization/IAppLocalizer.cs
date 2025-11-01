namespace EduCare.Application.Interfaces.Localization;

public interface IAppLocalizer
{
    string this[string key] { get; }
    string GetString(string key, params object[] arguments);
}