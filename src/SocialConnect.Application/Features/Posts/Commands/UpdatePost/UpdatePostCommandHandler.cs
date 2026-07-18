using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Result<Guid>>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        UpdatePostCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in.");
        }

        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null)
        {
            return Result<Guid>.Failure("Post not found.");
        }

        if (post.AuthorId != _currentUserService.UserId.Value)
        {
            return Result<Guid>.Failure("You are not authorized to update this post.");
        }

        post.UpdateContent(PostContent.Create(request.Content));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(post.Id);
    }
}
