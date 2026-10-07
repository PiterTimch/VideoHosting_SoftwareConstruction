using Domain.Entities.Identity;

namespace Domain.Entities.Portfolio;

public class PortfolioSubscriberEntity
{
    public long PortfolioId { get; set; }
    public virtual PortfolioEntity Portfolio { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;
}
