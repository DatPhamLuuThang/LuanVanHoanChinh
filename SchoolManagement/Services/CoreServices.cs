using System.Linq.Expressions;
using CoreModels;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.SchoolManagement;
using SchoolManagement.Services.Interface;
using Utilities;


namespace SchoolManagement.Services;

public class CoreServices<T> : ICoreServices<T> where T : CoreModel<Guid>
{
    /* Data Services */
    private readonly SchoolManagementDbContext _dbContext;
    private readonly DateTime _time = DateTime.Now;

    private readonly ResultMessage _result = new ResultMessage
    {
        IsSuccess = true,
        Message = Helper.NotyfMsg.Success
    };

    public CoreServices(SchoolManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    #region Get Services

    public T? GetById(string id)
    {
        return id.Trim().Replace("-", "").Length != 32 ? 
            null 
            : _dbContext.Set<T>().Find(Guid.Parse(id));
    }

    public IQueryable<T> GetAll()
    {
        return _dbContext.Set<T>().Where(x => !x.IsDeleted);
    }
    
    public async Task<T?> GetElementAsync(Expression<Func<T, bool>> where, List<Expression<Func<T, dynamic?>>>? includes= null)
    {
        try
        {
            var data = _dbContext.Set<T>().AsNoTracking();

            includes?.ForEach(include => data = data.Include(include));
            
            return await data.FirstOrDefaultAsync(where);
        }
        catch
        {
            return null;
        }
       
    }

    #endregion

    #region Add Services
    public async Task<ResultMessage> AddAsync(T entity, Guid userId)
    {
        SetParams(entity, userId, Helper.Method.Add);

        try
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    public async Task<ResultMessage> AddAsync(List<T> entity, Guid userId)
    {
        SetParams(entity, userId, Helper.Method.Add);

        try
        {
            await _dbContext.Set<T>().AddRangeAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }
    public async Task<ResultMessage> DuplicateAsync(T entity, Guid userId)
    {
        SetParams(entity, userId, Helper.Method.Duplicate);

        try
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    #endregion

    #region Update Services

    public async Task<ResultMessage> UpdateAsync(T entity, Guid userId)
    {
        try
        {
            SetParams(entity, userId, Helper.Method.Update);

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    public async Task<ResultMessage> UpdateAsync(List<T> entity, Guid userId)
    {
        try
        {
            SetParams(entity, userId, Helper.Method.Update);

            _dbContext.UpdateRange(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    #endregion

    #region Delete Services

    public async Task<ResultMessage> SoftDeleteAsync(T entity, Guid userId)
    {
        try
        {
            SetParams(entity, userId, Helper.Method.SoftDelete);

            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    public async Task<ResultMessage> SoftDeleteAsync(List<T> entity, Guid userId)
    {
        try
        {
            SetParams(entity, userId, Helper.Method.SoftDelete);

            _dbContext.UpdateRange(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    public async Task<ResultMessage> HardDeleteAsync(T entity, Guid userId)
    {
        try
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    public async Task<ResultMessage> HardDeleteAsync(IEnumerable<T> entity, Guid userId)
    {
        try
        {
            _dbContext.Set<T>().RemoveRange(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _result.IsSuccess = false;
            _result.Message = Helper.NotyfMsg.Error;
            Console.WriteLine("DevMsg: " + ex.Message);
        }

        return _result;
    }

    #endregion

    #region Private Functions

    private void SetParams(T entity, Guid userId, string method)
    {
        switch (method)
        {
            case 
                Helper.Method.Add:
                entity.IsActive = true;
                entity.IsDeleted = false;

                entity.CreatedBy = userId;
                entity.CreatedAt = _time;

                entity.UpdatedBy = Guid.Empty;
                entity.UpdatedAt = null;

                entity.DeletedBy = Guid.Empty;
                entity.DeletedAt = null;
                break;

            case 
                Helper.Method.Update:
                entity.UpdatedBy = userId;
                entity.UpdatedAt = _time;
                break;

            case 
                Helper.Method.SoftDelete:
                entity.IsDeleted = true;
                entity.DeletedBy = userId;
                entity.DeletedAt = _time;
                break;
        }
    }

    private void SetParams(List<T> entities, Guid userId, string method)
    {
        entities.ForEach(entity => SetParams(entity, userId, method));
    }

    #endregion
}