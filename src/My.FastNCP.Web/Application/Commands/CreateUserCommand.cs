namespace My.FastNCP.Web.Application.Commands;

public record CreateUserCommand(string UserName, string Password) : ICommand<CreateUserCommandResponse>;

public record CreateUserCommandResponse(long UserId, string UserName);

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名至少要3位")
            .MaximumLength(10).WithMessage("用户名不能超过10位");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少要6位")
            .MaximumLength(20).WithMessage("密码不能超过20位");
    }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserCommandResponse>
{
    public async Task<CreateUserCommandResponse> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return new CreateUserCommandResponse(Random.Shared.NextInt64(1000000, 9999999), request.UserName);
    }
}