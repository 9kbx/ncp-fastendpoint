namespace My.FastNCP.Web.Application.Commands;

public record DemoCommand(long Id) : ICommand<DemoCommandResponse>;

public record DemoCommandResponse(long Id, DateTime UpdateAt);

public class DemoCommandValidator : AbstractValidator<DemoCommand>
{
    public DemoCommandValidator()
    {
        // 添加验证规则示例：
        // RuleFor(x => x.Property).NotEmpty();
    }
}

public class DemoCommandHandler : ICommandHandler<DemoCommand, DemoCommandResponse>
{
    public async Task<DemoCommandResponse> Handle(
        DemoCommand request,
        CancellationToken cancellationToken)
    {
        return new(request.Id, DateTime.Now);
    }
}