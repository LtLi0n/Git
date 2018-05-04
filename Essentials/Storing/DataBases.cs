using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Storing
{
    public class DataBases
    {
        public UserSQL UserDB { get; private set; }

        public DataBases()
        {
            UserDB = new UserSQL("DataBases/users.sqlite");
        }

        public async Task Open()
        {
            await UserDB.Open();
        }

        public void CreateDB(string path) => new SQLiteConnection($"Data Source={path}.sqlite;Version=3;").Open();
    }
}
