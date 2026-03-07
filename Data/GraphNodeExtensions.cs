using Microsoft.EntityFrameworkCore;
using Sammlerplattform.Models.ConceptualRelationshipDatabase;
using System.Linq.Expressions;

namespace Sammlerplattform.Data
{
    public static class GraphNodeExtensions
    {
        /// <summary>
        /// Erzwingt eine explizite Projektion für Graph-Nodes
        /// und verhindert SELECT *
        /// </summary>
        public static IQueryable<T> AsGraphNode<T>(
            this IQueryable<T> query,
            Expression<Func<T, T>> projection)
            where T : class, IGraphNode
        {
            return query
                .Select(projection)
                .AsNoTracking();
        }
    }

}
