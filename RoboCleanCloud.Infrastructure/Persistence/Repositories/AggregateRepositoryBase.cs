using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Infrastructure.Persistence.Repositories;

public abstract class AggregateRepositoryBase<T> : RepositoryBase<T>, IAggregateRepository<T>
    where T : AggregateRoot
{
    protected AggregateRepositoryBase(ApplicationDbContext context) : base(context)
    {
    }
}