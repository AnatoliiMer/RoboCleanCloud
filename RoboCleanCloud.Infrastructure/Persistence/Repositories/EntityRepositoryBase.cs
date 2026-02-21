using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Infrastructure.Persistence.Repositories;

public abstract class EntityRepositoryBase<T> : RepositoryBase<T>, IEntityRepository<T>
    where T : Entity
{
    protected EntityRepositoryBase(ApplicationDbContext context) : base(context)
    {
    }
}