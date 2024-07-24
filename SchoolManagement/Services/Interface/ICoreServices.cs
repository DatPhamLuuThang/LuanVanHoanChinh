using System.Linq.Expressions;
using CoreModels;

namespace SchoolManagement.Services.Interface;

public interface ICoreServices<T> where T : class
{
    #region Get Services
    T? GetById(string id);
    IQueryable<T> GetAll();

    Task<T?> GetElementAsync(Expression<Func<T, bool>> where, List<Expression<Func<T, dynamic?>>>? includes= null);
    #endregion

    #region Add Services
    Task<ResultMessage> AddAsync(T entity, Guid userId);
    Task<ResultMessage> AddAsync(List<T> entity, Guid userId);
    Task<ResultMessage> DuplicateAsync(T entity, Guid userId);
    #endregion

    #region Update Services
    Task<ResultMessage> UpdateAsync(T entity, Guid userId);
    Task<ResultMessage> UpdateAsync(List<T> entity, Guid userId);
    #endregion

    #region Delete Services
    Task<ResultMessage> SoftDeleteAsync(T entity, Guid userId);
    Task<ResultMessage> SoftDeleteAsync(List<T> entity, Guid userId);
    Task<ResultMessage> HardDeleteAsync(T entity, Guid userId);
    Task<ResultMessage> HardDeleteAsync(IEnumerable<T> entity, Guid userId);
    #endregion
    
}