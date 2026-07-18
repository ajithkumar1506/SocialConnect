using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<Guid>>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostCommandHandler(
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
        CreatePostCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in to create a post.");
        }

        var postContent = string.IsNullOrWhiteSpace(request.Content)
            ? null
            : PostContent.Create(request.Content);

        var post = Post.Create(
            _currentUserService.UserId.Value,
            postContent,
            request.PostType,
            request.Status
        );

        if (request.MediaItems != null && request.MediaItems.Any())
        {
            for (int i = 0; i < request.MediaItems.Count; i++)
            {
                var item = request.MediaItems[i];
                post.AddMedia(MediaUrl.Create(item.Url), item.Type, i, item.FileSize);
            }
        }

        await _postRepository.AddAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(post.Id);
    }
}
