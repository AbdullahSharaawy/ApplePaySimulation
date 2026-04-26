namespace ApplePaySimulation.Infrastructure.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using ApplePaySimulation.Database.Context;
    using ApplePaySimulation.Database.Entities;
    using ApplePaySimulation.Repository.Abstracts;
    using Microsoft.EntityFrameworkCore;

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplePaySimulationContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplePaySimulationContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByID(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Update(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<int?> Create(T entity)
        {
            // For CreditCard entities, check if user already has a card
            int rowsAffected=0;
            if (typeof(T) == typeof(CreditCard))
            {
                var card = entity as CreditCard;
                var existingCard = await _dbSet
                    .Cast<CreditCard>()
                    .FirstOrDefaultAsync(c => c.UserId == card.UserId);

                if (existingCard != null)
                {
                    _dbSet.Update(entity);
                     rowsAffected = await _context.SaveChangesAsync();
                    return rowsAffected > 0 ? rowsAffected : null;
                }
            }

            await _dbSet.AddAsync(entity);
             rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected > 0 ? rowsAffected : null;
        }

        public async Task<List<T>> GetAllByFilter(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }
    }
}
