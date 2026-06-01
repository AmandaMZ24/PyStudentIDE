using Microsoft.EntityFrameworkCore;
using PyStudentIDE.Application.Interfaces;
using PyStudentIDE.Infrastructure.Data;

namespace PyStudentIDE.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public T? GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(int id)
    {
        var entity = _dbSet.Find(id);
        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }
}
