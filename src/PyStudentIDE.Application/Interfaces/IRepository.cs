namespace PyStudentIDE.Application.Interfaces;

public interface IRepository<T> where T : class
{
    T? GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    int SaveChanges();
    Task<int> SaveChangesAsync();
}
