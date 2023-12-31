﻿using Microsoft.EntityFrameworkCore;
using suavesabor_api.Application.Attributes;
using suavesabor_api.Application.Data;
using suavesabor_api.Application.Domain.Base;

namespace suavesabor_api.Application.Repository.Generic.Impl
{
    public class GenericRepositoryImpl<T, TKey> : IGenericRepository<T, TKey> where T : class, IEntity<TKey>
    {
        private readonly DataContext _context;
        public DbSet<T> Dataset { get; set; }
        public GenericRepositoryImpl(DataContext context)
        {
            _context = context;
            Dataset = _context.Set<T>();
        }

        async public Task<T> Create(T Entity)
        {
            Dataset.Add(Entity);
            await _context.SaveChangesAsync();

            return Entity;
        }

        public void Delete(T id)
        {
            var result = Dataset.SingleOrDefault(r => r.Id != null && r.Id.Equals(id));
            if (result == null)
            {
                return;
            }

            Dataset.Remove(result);
            _context.SaveChanges();
        }

        async public Task<bool> Exists(T Entity)
        {
            return await Dataset.AnyAsync(e => e.Id != null && e.Id.Equals(Entity.Id));
        }

        async public Task<List<T>> FindAll()
        {
            IQueryable<T> Query = Dataset;
            Query = getIncludeQuery(Query);

            return await Query.ToListAsync();
        }

        async public Task<T?> FindByID(TKey id)
        {
            IQueryable<T> Query = Dataset;
            Query = getIncludeQuery(Query);

            return await Query.SingleOrDefaultAsync(e => e.Id != null && e.Id.Equals(id));
        }

        async public Task<T?> Update(T Entity)
        {
            var Result = await Dataset.SingleOrDefaultAsync(e => e.Id != null && e.Id.Equals(Entity.Id));
            if (Result == null)
            {
                return null;
            }


            _context.Entry(Result).CurrentValues.SetValues(Entity);
            await _context.SaveChangesAsync();
            return Result;

        }

        private IQueryable<T> getIncludeQuery(IQueryable<T> Query)
        {
            var EntityType = typeof(T);
            if (EntityType.GetCustomAttributes(typeof(IncludeRelatedEntitiesAttribute), true)
                                             .FirstOrDefault() is IncludeRelatedEntitiesAttribute IncludeAttribute)
            {
                foreach (var EntityToInclude in IncludeAttribute.EntitiesToInclude)
                {
                    Query = Query.Include(EntityToInclude);
                }
            }

            return Query;
        }
    }
}
