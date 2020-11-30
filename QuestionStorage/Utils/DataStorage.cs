using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QuestionStorage.Utils
{
    public static class DataStorage
    {
        internal static async Task<List<T>> GetListAsync<T>(DbSet<T> dbSet) where T : class =>
            await dbSet.ToListAsync();

        internal static async Task<HashSet<T>> GetHashSetAsync<T>(DbSet<T> dbSet) where T : class =>
            new HashSet<T>(await GetListAsync(dbSet));
        
        internal static List<TU> GetTypedListByPredicateAndSelector<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TU>> selector) where T : class =>
            dbSet
                .Where(predicate)
                .Select(selector)
                .ToList();
        
        internal static HashSet<TU> GetTypedHashSetByPredicateAndSelector<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TU>> selector) where T : class =>
            new HashSet<TU>(GetTypedListByPredicateAndSelector(dbSet, predicate, selector));

        internal static async Task<List<TU>> GetTypedListByPredicateAndSelectorAsync<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TU>> selector) where T : class =>
            await dbSet
                .Where(predicate)
                .Select(selector)
                .ToListAsync();

        internal static async Task<HashSet<TU>> GetTypedHashSetByPredicateAndSelectorAsync<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TU>> selector) where T : class =>
            new HashSet<TU>(await GetTypedListByPredicateAndSelectorAsync(dbSet, predicate, selector));

        internal static async Task<List<T>> GetListByPredicateAsync<T>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate) where T : class =>
            await dbSet
                .Where(predicate)
                .ToListAsync();

        internal static async Task<List<TU>> GetTypedListBySelectorAsync<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, TU>> selector) where T : class =>
            await dbSet
                .Select(selector)
                .ToListAsync();

        internal static async Task<HashSet<TU>> GetTypedHashSetBySelectorAsync<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, TU>> selector) where T : class =>
            new HashSet<TU>(await GetTypedListBySelectorAsync(dbSet, selector));

        internal static List<TU> GetTypedListBySelector<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, TU>> selector) where T : class =>
            dbSet
                .Select(selector)
                .ToList();

        internal static HashSet<TU> GetTypedHashSetBySelector<T, TU>(DbSet<T> dbSet,
            Expression<Func<T, TU>> selector) where T : class =>
            new HashSet<TU>(GetTypedListBySelector(dbSet, selector));

        internal static async Task<T> GetByPredicateAsync<T>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate) where T : class =>
            await dbSet.FirstOrDefaultAsync(predicate);
        
        internal static async Task<bool> CheckByPredicateAsync<T>(DbSet<T> dbSet,
            Expression<Func<T, bool>> predicate) where T : class =>
            await dbSet.AnyAsync(predicate);

        internal static async Task<int> GetIdAsync<T>(DbSet<T> dbSet, Expression<Func<T, int>> selector)
            where T : class =>
            await dbSet.MaxAsync(selector) + 1;
    }
}