﻿using HMES.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.Repositories.GenericRepositories
{
    public class GenericRepositories<T> : IGenericRepositories<T> where T : class
    {
        private readonly HmesContext context;
        protected DbSet<T> dbSet;

        public GenericRepositories(HmesContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        protected HmesContext Context
        {
            get
            {
                if (context == null!) return new HmesContext();
                return context;
            }
        }

        private IQueryable<T> GetQueryable(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10;

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query.AsNoTracking().AsSplitQuery();
        }

        public async Task<IEnumerable<T>> GetList(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null,
            bool asNoTracking = true // thêm flag để chủ động chọn
        )
        {
            var query = GetQueryable(filter, orderBy, includeProperties, pageIndex, pageSize);
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<T> GetSingle(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "")
        {
            var query = GetQueryable(filter, orderBy, includeProperties, null, null);
            if (orderBy != null)
                query = orderBy(query);
            return await query.FirstOrDefaultAsync();
        }

        public async Task Insert(T entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task InsertRange(List<T> entity)
        {
            await dbSet.AddRangeAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task UpdateRange(List<T> entities)
        {
            foreach (var entity in entities)
            {
                dbSet.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
            }
            await context.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            if (context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            context.Entry(entity).State = EntityState.Deleted;

            await context.SaveChangesAsync();
        }

        public async Task DeleteRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                if (context.Entry(entity).State == EntityState.Detached)
                {
                    dbSet.Attach(entity);
                }

                context.Entry(entity).State = EntityState.Deleted;
            }

            await context.SaveChangesAsync();
        }
    }
}
