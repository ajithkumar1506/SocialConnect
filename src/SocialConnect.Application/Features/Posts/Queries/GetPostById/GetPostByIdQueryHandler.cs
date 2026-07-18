using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Posts.DTOs;

namespace SocialConnect.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PostDto?> Handle(
        GetPostByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var post = await _context
            .Posts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (post == null)
            return null;

        return _mapper.Map<PostDto>(post);
    }
}
