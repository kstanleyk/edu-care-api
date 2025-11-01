using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduCare.Application.Features.Core.Dtos;

public record StudentDto
{
    public Guid Id { get; init; }
    public string StudentId { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? MiddleName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public DateTime CreatedOn { get; init; }

    // Parameterless constructor for AutoMapper
    public StudentDto() { }
}