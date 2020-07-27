using System.Collections.Generic;

namespace QueryAny
{
    public static class Query
    {
        public static FromClause<TPrimaryEntity> From<TPrimaryEntity>() where TPrimaryEntity : IQueryableEntity
        {
            return new FromClause<TPrimaryEntity>();
        }

        public static QueryClause<TPrimaryEntity> Empty<TPrimaryEntity>() where TPrimaryEntity : IQueryableEntity
        {
            var entities = new QueriedEntities(new List<QueriedEntity>
            {
                new QueriedEntity(typeof(TPrimaryEntity))
            });
            entities.UpdateOptions(true);

            return new QueryClause<TPrimaryEntity>(entities);
        }
    }
}