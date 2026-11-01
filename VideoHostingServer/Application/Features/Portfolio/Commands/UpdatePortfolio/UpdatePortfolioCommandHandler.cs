using Application.Interfaces;
using Application.Mappings;
using Application.Models.Portfolio;
using Domain.Entities.Portfolio;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolio.Commands.UpdatePortfolio;

public class UpdatePortfolioCommandHandler(
    IGenericRepository<PortfolioEntity, long> portfolios,
    PortfolioMappingProfile mapper,
    IImageService images,
    ICurrentUserService currentUser) : IRequestHandler<UpdatePortfolioCommand, PortfolioItemModel>
{
    public async Task<PortfolioItemModel> Handle(UpdatePortfolioCommand request, CancellationToken cancellationToken)
    {
        var ownerId = currentUser.GetCurrentUserId();
        var entity = await portfolios.AsQurable().Include(x => x.Subscribers)
            .FirstOrDefaultAsync(x => x.Id == request.Model.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Портфоліо не знайдено.");
        if (entity.Id != ownerId && !currentUser.IsInRole("Admin"))
            throw new UnauthorizedAccessException("Немає прав редагувати це портфоліо.");

        mapper.MapToEntity(request.Model, entity);
        if (request.Model.AvatarImage is not null)
        {
            if (entity.AvatarImage is not null) await images.DeleteImageAsync(entity.AvatarImage);
            entity.AvatarImage = await images.SaveImageAsync(request.Model.AvatarImage);
        }
        if (request.Model.BannerImage is not null)
        {
            if (entity.BannerImage is not null) await images.DeleteImageAsync(entity.BannerImage);
            entity.BannerImage = await images.SaveImageAsync(request.Model.BannerImage);
        }
        await portfolios.UpdateAsync(entity);
        var result = mapper.MapToItemModel(entity);
        result.OwnerId = entity.Id;
        return result;
    }
}
