using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TeaClient.SQLLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SqlLiteConn))]
namespace TeaClient.SQLLite
{
    public class SqlLiteConn : ISqlLite
    {
        SQLiteConnection ISqlLite.GetConnection()
        {
            var dbase = "TeaCollection";
            var dbpath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(dbpath, dbase);
            var connection = new SQLiteConnection(path);
            return connection;
        }
    }
}
