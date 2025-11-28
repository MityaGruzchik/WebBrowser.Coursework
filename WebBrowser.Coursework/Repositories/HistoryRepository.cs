using System;
using System.Collections.Generic;
using Npgsql;
using WebBrowser.Coursework.Entities;

namespace WebBrowser.Coursework.Repositories
{
    public class HistoryRepository : IRepository<HistoryItem>
    {
        private readonly string _connString;
        public HistoryRepository(string connString) => _connString = connString;

        public void Add(HistoryItem entity)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO history (title, url, visit_date) VALUES (@t, @u, @d)", conn);
            cmd.Parameters.AddWithValue("t", entity.Title ?? "");
            cmd.Parameters.AddWithValue("u", entity.Url);
            cmd.Parameters.AddWithValue("d", entity.VisitDate);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM history WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<HistoryItem> GetAll()
        {
            var list = new List<HistoryItem>();
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT id, title, url, visit_date FROM history ORDER BY visit_date DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new HistoryItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Url = reader.GetString(2),
                    VisitDate = reader.GetDateTime(3)
                });
            }
            return list;
        }
    }
}