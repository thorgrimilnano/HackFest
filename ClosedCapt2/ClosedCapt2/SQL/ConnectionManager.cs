using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ClosedCapt2.SQL
{
    public static class ConnectionManager
    {
        public static SqlConnection GetConnection()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = "captinclose.database.windows.net";
            builder.UserID = "captinClose";
            builder.Password = "OhMyGod2018";
            builder.InitialCatalog = "dbo";

            return new SqlConnection("Server=tcp:captinclose.database.windows.net,1433;Initial Catalog=CaptinClose;Persist Security Info=False;User ID=CaptinClose;Password=OhMyGod2018;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
}