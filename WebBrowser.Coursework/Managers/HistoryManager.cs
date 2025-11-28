using System;
using System.Collections.Generic;
using System.Linq;
using WebBrowser.Coursework.Entities;
using WebBrowser.Coursework.Repositories;

namespace WebBrowser.Coursework.Managers
{
    public class HistoryManager
    {
        private readonly IRepository<HistoryItem> _repository;

        // Dependency Injection через конструктор
        public HistoryManager(IRepository<HistoryItem> repository)
        {
            _repository = repository;
        }

        public void AddEntry(string title, string url)
        {
            var item = new HistoryItem
            {
                Title = string.IsNullOrEmpty(title) ? url : title,
                Url = url,
                VisitDate = DateTime.Now
            };
            _repository.Add(item);
        }

        public IEnumerable<HistoryItem> GetAllHistory()
        {
            return _repository.GetAll();
        }

        // Приклад бізнес-логіки: пошук в історії
        public IEnumerable<HistoryItem> Search(string query)
        {
            var all = _repository.GetAll();
            return all.Where(x => x.Title.ToLower().Contains(query.ToLower()) ||
                                  x.Url.ToLower().Contains(query.ToLower()));
        }

        public void ClearAllHistory()
        {
            // У реальному проєкті тут має бути метод DeleteAll в репозиторії.
            // Для прикладу видаляємо по одному (через наявний контракт IRepository)
            var all = _repository.GetAll();
            foreach (var item in all)
            {
                _repository.Delete(item.Id);
            }
        }
    }
}