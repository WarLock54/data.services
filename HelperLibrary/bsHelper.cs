using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
   public class bsHelper<E>where E: class,new()
    {
        public enum ExpresionMergeTypes { And = 1, Or = 2 };

        public static Expression<Func<E, bool>> MergeExpression(Expression<Func<E, bool>> E1, Expression<Func<E, bool>> E2, ExpresionMergeTypes mergeTypes) {
            if (mergeTypes == ExpresionMergeTypes.And)
            {
                Expression<Func<E, bool>> result = Expression.Lambda<Func<E, bool>>(Expression.And(E1.Body, E2.Body), E1.Parameters.Single());
                return result;
            }
            else if (mergeTypes == ExpresionMergeTypes.Or) {
                Expression<Func<E,bool>> result=Expression.Lambda<Func<E,bool>>(Expression.Or(E1.Body,E2.Body), E1.Parameters.Single());
            }
            return E1;
        }
        public static PropertyInfo GetPropertyInfo(string propName)
        {
            Type typeE = typeof(E);
            PropertyInfo myPropInfo = typeE.GetProperty(propName);
            return myPropInfo == null ? null : myPropInfo;
        }
        public static IQueryable<E> ApplyFinalPredicate(IQueryable<E> dbQuery, Expression<Func<E, bool>> QUERYpredicate, Expression<Func<E, bool>> bsPredicate) {
            IQueryable<E> query = dbQuery;
            if(bsPredicate != null)
                query = query.Where(bsPredicate);
            if(QUERYpredicate!=null)
                query = query.Where(QUERYpredicate);
            return query;
        }
    }
}
