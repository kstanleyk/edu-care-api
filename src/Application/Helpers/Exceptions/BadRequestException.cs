namespace EduCare.Application.Helpers.Exceptions;

public class BadRequestException(string message) : ApplicationException(message);