using Application.Interfaces.Commons;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.QueryParameter;
using Shared.Results;
using System.Linq.Expressions;

namespace Infrastructure.Context.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        #region Implementation
        public void Detach(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
        
        #region Create
        public virtual TEntity Add(TEntity entity)
        {
            return _dbSet.Add(entity).Entity;
        }

        #endregion

        #region Update
        public virtual void Update(TEntity entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        #endregion

        #region Delete
        public virtual TEntity Delete(TEntity entity)
        {
            return _dbSet.Remove(entity).Entity;
        }
        
        public virtual TEntity Delete(TKey id)
        {
            var entity = _dbSet.Find(id);
            return _dbSet.Remove(entity).Entity;
        }

        public virtual void DeleteMulti(Expression<Func<TEntity, bool>> where)
        {
            IEnumerable<TEntity> objects = _dbSet.Where(where).AsEnumerable();
            foreach (TEntity obj in objects)
                _dbSet.Remove(obj);
        }
        
        public virtual void DeleteMulti(IEnumerable<TEntity> where)
        {
            foreach (TEntity obj in where)
                _dbSet.Remove(obj);
        }

        #endregion

        #region Read
        public async Task<bool> IsExistAsync(TKey id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null;
        }
        public virtual TEntity GetSingleById(TKey id)
        {
            return _dbSet.Find(id);
        }

        public virtual IEnumerable<TEntity> GetMany(Expression<Func<TEntity, bool>> where, string includes)
        {
            return _dbSet.Where(where).ToList();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> where)
        {
            return _dbSet.Count(where);
        }

        public IEnumerable<TEntity> GetAll(string[] includes = null)
        {
            if (includes != null && includes.Count() > 0)
            {
                var query = _context.Set<TEntity>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                return query.AsQueryable();
            }

            return _context.Set<TEntity>().AsQueryable();
        }

        public TEntity GetSingleByCondition(Expression<Func<TEntity, bool>> expression, string[] includes = null)
        {
            if (includes != null && includes.Count() > 0)
            {
                var query = _context.Set<TEntity>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                return query.FirstOrDefault(expression);
            }
            return _context.Set<TEntity>().FirstOrDefault(expression);
        }

        public virtual IEnumerable<TEntity> GetMulti(Expression<Func<TEntity, bool>>? predicate, string[] includes = null)
        {
            if (includes != null && includes.Count() > 0)
            {
                var query = _context.Set<TEntity>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                if (predicate != null)
                    query = query.Where(predicate);
                return query.AsQueryable();
            }

            return _context.Set<TEntity>().Where(predicate).AsQueryable();
        }

        public virtual IEnumerable<TEntity> GetMultiNoTracking(Expression<Func<TEntity, bool>>? predicate, string[] includes = null)
        {
            if (includes != null && includes.Count() > 0)
            {
                var query = _context.Set<TEntity>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                if (predicate != null)
                    query = query.Where(predicate);
                return query.AsNoTracking().AsQueryable();
            }

            return _context.Set<TEntity>().Where(predicate).AsQueryable();
        }

        public virtual IEnumerable<TEntity> GetMultiByFilterNoPaging(Expression<Func<TEntity, bool>>? predicate, GenericQueryParameters parameters, string[]? searchProperties, string[]? includes = null)
        {
            IQueryable<TEntity> query = _dbSet;

            //Include navigation properties
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            //Apply predicate filter
            if (predicate != null)
                query = query.Where(predicate);

            // Apply generic filters
            query = query.ApplyFilters(parameters.Filters);

            // Apply search
            query = query.ApplySearch(parameters.Search, searchProperties);

            return query;
        }
       
        public virtual IEnumerable<TEntity> GetMultiPaging(Expression<Func<TEntity, bool>> predicate, out int total, int index = 0, int size = 20, string[] includes = null)
        {
            int skipCount = index * size;
            IQueryable<TEntity> _resetSet;

            if (includes != null && includes.Count() > 0)
            {
                var query = _context.Set<TEntity>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                _resetSet = predicate != null ? query.Where(predicate).AsQueryable() : query.AsQueryable();
            }
            else
            {
                _resetSet = predicate != null ? _context.Set<TEntity>().Where(predicate).AsQueryable() : _context.Set<TEntity>().AsQueryable();
            }

            _resetSet = skipCount == 0 ? _resetSet.Take(size) : _resetSet.Skip(skipCount).Take(size);
            total = _resetSet.Count();
            return _resetSet.AsQueryable();
        }

        public virtual PagedResult<TEntity> GetPaged(GenericQueryParameters parameters, string[]? searchProperties = null, string[]? includes = null)
        {

            return GetPaged(null, parameters, searchProperties, includes);
        }

        public virtual PagedResult<TEntity> GetPaged(Expression<Func<TEntity, bool>>? predicate, GenericQueryParameters parameters, string[]? searchProperties, string[]? includes = null)
        {
            IQueryable<TEntity> query = _dbSet;

            //Include navigation properties
            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            //Apply predicate filter
            if (predicate != null)
                query = query.Where(predicate);

            // Apply generic filters
            query = query.ApplyFilters(parameters.Filters);

            // Apply search
            query = query.ApplySearch(parameters.Search, searchProperties);

            // Apply sorting
            query = query.ApplySorting(parameters.SortBy, parameters.SortDirection);

            // Apply paging
            return query.ToPagedResult(parameters.Index, parameters.PageSize);
        }

        public bool CheckContains(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>().Count(predicate) > 0;
        }

        public TEntity GetSingleById(string id)
        {
            return _dbSet.Find(id);
        }
        #endregion
        
        #endregion
    }
}
