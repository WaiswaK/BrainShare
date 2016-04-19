using System.IO;
using System.Threading.Tasks;
using SQLite;
using BrainShare.Models;
using Windows.Storage;
using System;
using BrainShare.Common;

namespace BrainShare.Database
{
    class DbConnection : IDbConnection
    {
        SQLiteAsyncConnection conn;
        public DbConnection()
        {
            conn = new SQLiteAsyncConnection(Constants.dbPath);
        }
        public async Task<bool> LocalDatabaseNotPresent(string fileName)
        {
           var item = await Constants.appFolder.TryGetItemAsync(fileName);
           return item == null;
        }
        public async Task InitializeDatabase()
        {
            if (await LocalDatabaseNotPresent(Constants.dbName))
            {
                using (var db = new SQLite.SQLiteConnection(Constants.dbPath))
                {
                    db.CreateTable<Subject>();
                    db.CreateTable<Topic>();
                    db.CreateTable<Assignment>();
                    db.CreateTable<Attachment>();
                    db.CreateTable<Video>();
                    db.CreateTable<User>();
                    db.CreateTable<School>();
                    db.CreateTable<Book>();
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
