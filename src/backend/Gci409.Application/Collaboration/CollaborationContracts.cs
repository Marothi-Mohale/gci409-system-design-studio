using FluentValidation;
using Gci409.Domain.Collaboration;

namespace Gci409.Application.Collaboration;

public sealed record CreateCommentThreadRequest(CommentTargetType TargetType, Guid TargetId, string InitialComment);

public sealed record AddCommentRequest(string Body);

public sealed record CommentResponse(Guid Id, Guid? CreatedByUserId, string Body, DateTimeOffset CreatedAtUtc);

public sealed record CommentThreadSummaryResponse(Guid Id, CommentTargetType TargetType, Guid TargetId, CommentStatus Status, int CommentCount, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);

public sealed record CommentThreadDetailResponse(Guid Id, CommentTargetType TargetType, Guid TargetId, CommentStatus Status, IReadOnlyCollection<CommentResponse> Comments, DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);

public sealed class CreateCommentThreadRequestValidator : AbstractValidator<CreateCommentThreadRequest>
{
    public CreateCommentThreadRequestValidator()
    {
        RuleFor(x => x.TargetType).IsInEnum();
        RuleFor(x => x.TargetId).NotEmpty();
        RuleFor(x => x.InitialComment).NotEmpty().MaximumLength(4000);
    }
}

public sealed class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
