namespace ReviewService.Api.Contracts;

public sealed record CreateReviewRequest(int Rating, string? Comment);

public sealed record ReviewResponse(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAtUtc);

public sealed record ProductReviewSummaryResponse(
    Guid ProductId,
    double AverageRating,
    int ReviewCount,
    IReadOnlyList<ReviewResponse> Reviews);
