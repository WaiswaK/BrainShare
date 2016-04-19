using System.Threading.Tasks;
using SQLite;

namespace BrainShare.Database
{
    interface IDbConnection
    {
        Task InitializeDatabase();
        SQLiteAsyncConnection GetAsyncConnection();
    }
}
