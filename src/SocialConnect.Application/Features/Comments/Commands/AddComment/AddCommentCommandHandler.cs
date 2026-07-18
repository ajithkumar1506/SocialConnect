using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Comments.Commands.AddComment;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<Guid>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        AddCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in to comment.");
        }

        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            return Result<Guid>.Failure("Post not found.");
        }

        string parentPath = "";
        int parentDepth = 0;

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(
                request.ParentCommentId.Value,
                cancellationToken
            );
            if (parentComment == null)
            {
                return Result<Guid>.Failure("Parent comment not found.");
            }
            parentPath = parentComment.Path;
            parentDepth = parentComment.Depth;
        }

        var comment = Comment.Create(
            request.PostId,
            _currentUserService.UserId.Value,
            request.Content,
            request.ParentCommentId,
            parentPath,
            parentDepth
        );

        post.IncrementCommentCount();

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(comment.Id);
    }
}
