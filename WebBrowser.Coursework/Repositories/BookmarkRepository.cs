using System;
using System.Collections.Generic;
using Npgsql;
using WebBrowser.Coursework.Entities;

namespace WebBrowser.Coursework.Repositories
{
    public class BookmarkRepository : IRepository<Bookmark>
    {
        private readonly string _connString;
        public BookmarkRepository(string connString) => _connString = connString;

        public void Add(Bookmark entity)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO bookmarks (title, url, created_at) VALUES (@t, @u, @d)", conn);
            cmd.Parameters.AddWithValue("t", entity.Title ?? "");
            cmd.Parameters.AddWithValue("u", entity.Url);
            cmd.Parameters.AddWithValue("d", entity.CreatedAt);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("DELETE FROM bookmarks WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<Bookmark> GetAll()
        {
            var list = new List<Bookmark>();
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT id, title, url, created_at FROM bookmarks ORDER BY created_at DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Bookmark
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Url = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                });
            }
            return list;
        }
    }
}