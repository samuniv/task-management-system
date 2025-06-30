using BlazorWasm.Shared.DTOs;
using FluentValidation;

namespace BlazorWasm.Shared.Validators;

public class TaskEditDtoValidator : AbstractValidator<TaskEditDto>
{
    public TaskEditDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Today.AddDays(-1))
            .WithMessage("Due date must be today or in the future")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value");
    }
}

public class CommentCreateDtoValidator : AbstractValidator<CommentCreateDto>
{
    public CommentCreateDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters");

        RuleFor(x => x.TaskId)
            .GreaterThan(0).WithMessage("Valid task ID is required");
    }
}
