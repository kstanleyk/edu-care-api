namespace EduCare.Application.Helpers.Exceptions;

public class NotFoundException(string name, object key) : ApplicationException($"{name} ({key}) is not found");