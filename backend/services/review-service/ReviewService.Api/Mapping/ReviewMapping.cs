using ReviewService.Api.Contracts;
using ReviewService.Api.Domain;

namespace ReviewService.Api.Mapping;

public static class ReviewMapping
{
    public static ReviewResponse ToResponse(this Review review) => new(
        review.Id,
        review.ProductId,
        review.UserId,
        review.Rating,
        review.Comment,
        review.CreatedAtUtc);
}
