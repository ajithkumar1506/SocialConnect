namespace SocialConnect.Application.Common.Models;

public class CursorPaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public string? NextCursor { get; }
    public bool HasNextPage { get; }

    public CursorPaginatedList(IReadOnlyCollection<T> items, string? nextCursor, bool hasNextPage)
    {
        Items = items;
        NextCursor = nextCursor;
        HasNextPage = hasNextPage;
    }
}
