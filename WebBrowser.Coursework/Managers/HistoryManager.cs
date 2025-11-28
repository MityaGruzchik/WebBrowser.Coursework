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

        public IEnumerable<HistoryItem> Search(string query)
        {
            var all = _repository.GetAll();
            return all.Where(x => x.Title.ToLower().Contains(query.ToLower()) ||
                                  x.Url.ToLower().Contains(query.ToLower()));
        }

        public void ClearAllHistory()
        {
            var all = _repository.GetAll();
            foreach (var item in all)
            {
                _repository.Delete(item.Id);
            }
        }
    }
}