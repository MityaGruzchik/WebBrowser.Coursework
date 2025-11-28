using System;
namespace WebBrowser.Coursework.Entities
{
    public class HistoryItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime VisitDate { get; set; }
    }
}