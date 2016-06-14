using System.Threading.Tasks;
using SQLite;
using System;
using BrainShare.Common;

namespace BrainShare.Database
{
    class DbConnection : IDbConnection
    {
        SQLiteAsyncConnection conn;
        public DbConnection()
        {
            conn = new SQLiteAsyncConnection(Constant.dbPath);
        }
        public async Task<bool> LocalDatabaseNotPresent(string fileName)
        {
            try
            {
                await Constant.appFolder.TryGetItemAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task InitializeDatabase()
        {
            if (await LocalDatabaseNotPresent(Constant.dbName))
            {
                using (var db = new SQLiteConnection(Constant.dbPath))
                {
                    db.CreateTable<Subject>();
                    db.CreateTable<Topic>();
                    db.CreateTable<Assignment>();
                    db.CreateTable<Attachment>();
                    db.CreateTable<Video>();
                    db.CreateTable<User>();
                    db.CreateTable<School>();
                    db.CreateTable<Book>();
                    db.CreateTable<log>();
                };
            }
            else {
                 }
          }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            return conn;
        }
    }
}
