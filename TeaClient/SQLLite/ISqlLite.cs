using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeaClient.SQLLite
{
   public interface ISqlLite
    {
        SQLiteConnection GetConnection();
    }
}
