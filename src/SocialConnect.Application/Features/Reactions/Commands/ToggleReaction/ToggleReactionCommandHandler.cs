using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Reactions.Commands.ToggleReaction;

public class ToggleReactionCommandHandler : IRequestHandler<ToggleReactionCommand, Result<bool>>
{
    private readonly IReactionRepository _reactionRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ToggleReactionCommandHandler(
        IReactionRepository reactionRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _reactionRepository = reactionRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        ToggleReactionCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<bool>.Failure("User must be logged in to react.");
        }

        var userId = _currentUserService.UserId.Value;

        // Verify target exists and get the reaction-supporting entity
        IReactionCountable? target = null;

        if (request.TargetType == TargetType.Post)
        {
            var post = await _postRepository.GetByIdAsync(request.TargetId, cancellationToken);
            if (post != null)
                target = new PostAdapter(post);
        }
        else if (request.TargetType == TargetType.Comment)
        {
            var comment = await _commentRepository.GetByIdAsync(
                request.TargetId,
                cancellationToken
            );
            if (comment != null)
                target = new CommentAdapter(comment);
        }

        if (target == null)
        {
            return Result<bool>.Failure("Target entity not found.");
        }

        // Check if user already reacted to this target
        var existingReaction = await _reactionRepository.GetUserReactionAsync(
            userId,
            request.TargetId,
            request.TargetType,
            cancellationToken
        );

        if (existingReaction != null)
        {
            if (existingReaction.ReactionType == request.ReactionType)
            {
                // Toggle off: remove reaction
                await _reactionRepository.DeleteAsync(existingReaction, cancellationToken);
                target.DecrementReactionCount();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(false);
            }
            else
            {
                // Update reaction type: change the type, keep count same
                await _reactionRepository.DeleteAsync(existingReaction, cancellationToken);
                var newReaction = Reaction.Create(
                    userId,
                    request.TargetId,
                    request.TargetType,
                    request.ReactionType
                );
                await _reactionRepository.AddAsync(newReaction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }
        else
        {
            // Toggle on: add reaction
            var newReaction = Reaction.Create(
                userId,
                request.TargetId,
                request.TargetType,
                request.ReactionType
            );
            await _reactionRepository.AddAsync(newReaction, cancellationToken);
            target.IncrementReactionCount();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }

    // Helper interface & adapters to keep logic clean and DRY
    private interface IReactionCountable
    {
        void IncrementReactionCount();
        void DecrementReactionCount();
    }

    private class PostAdapter : IReactionCountable
    {
        private readonly Post _post;

        public PostAdapter(Post post) => _post = post;

        public void IncrementReactionCount() => _post.IncrementReactionCount();

        public void DecrementReactionCount() => _post.DecrementReactionCount();
    }

    private class CommentAdapter : IReactionCountable
    {
        private readonly Comment _comment;

        public CommentAdapter(Comment comment) => _comment = comment;

        public void IncrementReactionCount() => _comment.IncrementReactionCount();

        public void DecrementReactionCount() => _comment.DecrementReactionCount();
    }
}
