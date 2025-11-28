using System.Collections.Generic;
namespace WebBrowser.Coursework.Repositories
{
    public interface IRepository<T>
    {
        void Add(T entity);
        void Delete(int id);
        IEnumerable<T> GetAll();
    }
}