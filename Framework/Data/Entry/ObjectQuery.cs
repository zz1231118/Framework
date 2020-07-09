using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Framework.Data.Entry
{
    public class ObjectQuery<T> : IQueryable<T>
    {
        private readonly IQueryProvider queryProvider;
        private readonly Expression expression;

        internal ObjectQuery(IQueryProvider queryProvider)
        {
            this.queryProvider = queryProvider;
            this.expression = Expression.Constant(this);
        }

        internal ObjectQuery(IQueryProvider queryProvider, Expression expression)
        {
            this.queryProvider = queryProvider;
            this.expression = expression;
        }

        public IQueryProvider Provider => queryProvider;

        public Expression Expression => expression;

        public Type ElementType => typeof(T);

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
