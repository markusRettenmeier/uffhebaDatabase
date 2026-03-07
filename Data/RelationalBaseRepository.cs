using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Reflection;

namespace Sammlerplattform.Data
{
    public class RelationalBaseRepository<TEntity>(DbIdentityContext context) where TEntity : class
    {
        private static readonly char[] charArray = [','];
        internal DbIdentityContext context = context;
        internal DbSet<TEntity> dbSet = context.Set<TEntity>();

        public virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (string includeProperty in includeProperties.Split
                (charArray, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return orderBy != null ? [.. orderBy(query)] : [.. query];
        }

        public virtual TEntity? GetByID(object id)
        {
            return dbSet.Find(id);
        }

        public virtual TEntity Insert(TEntity entity)
        {
            EntityEntry<TEntity> entityEntry = dbSet.Add(entity);
            return entityEntry != null ? entityEntry.Entity : throw new NullReferenceException();
        }

        public void AddMemberToCollection<TMember>(TEntity parentEntity,
                                                   Expression<Func<TEntity, ICollection<TMember>>> navigationProperty,
                                                   TMember member)
        where TMember : class
        {
            PropertyInfo? property = (navigationProperty.Body as MemberExpression)?.Member as PropertyInfo;
            ICollection<TMember>? collection = property?.GetValue(parentEntity) as ICollection<TMember>;
            collection?.Add(member);
        }
        public void RemoveMemberFromCollection<TMember>(TEntity parentEntity,
                                            Expression<Func<TEntity, ICollection<TMember>>> navigationProperty,
                                            TMember member)
        where TMember : class
        {
            PropertyInfo? property = (navigationProperty.Body as MemberExpression)?.Member as PropertyInfo;
            ICollection<TMember>? collection = property?.GetValue(parentEntity) as ICollection<TMember>;
            _ = (collection?.Remove(member));
        }

        public void SetForeignKey<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> foreignKeyProperty, TProperty value)
        {
            PropertyInfo? property = (foreignKeyProperty.Body as MemberExpression)?.Member as PropertyInfo;
            property?.SetValue(entity, value);
        }

        public virtual void Delete(object id)
        {
            TEntity? entityToDelete = dbSet.Find(id);
            if (entityToDelete != null)
            {
                Delete(entityToDelete);
            }
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State.Equals(EntityState.Detached))
            {
                _ = dbSet.Attach(entityToDelete);
            }
            _ = dbSet.Remove(entityToDelete);
        }
    }
}
