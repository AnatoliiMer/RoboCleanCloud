using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Application.Interfaces.Repositories;

// Базовый интерфейс для всех репозиториев
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

// Репозиторий для Aggregate Roots
public interface IAggregateRepository<T> : IRepository<T> where T : AggregateRoot
{
    // Специфические методы для AggregateRoot можно добавить здесь
}

// Репозиторий для обычных Entity
public interface IEntityRepository<T> : IRepository<T> where T : Entity
{
    // Специфические методы для Entity можно добавить здесь
}