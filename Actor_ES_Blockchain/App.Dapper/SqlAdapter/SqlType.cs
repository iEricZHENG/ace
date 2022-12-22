using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Dapper.SqlAdapter
{
    public enum SqlType
    {
        SqlServer = 0,
        MySql = 1,
        SQLite = 2,
        Npgsql = 3
    }
}
