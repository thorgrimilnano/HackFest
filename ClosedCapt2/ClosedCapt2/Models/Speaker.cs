using ClosedCapt2.SQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ClosedCapt2.Models
{
    public class Speaker
    {
        public int ID { get; private set; }
        public string Language_Code { get; private set; }
        public string Name { get; private set; }
        private string _facialRecId;

        public Speaker(string facialRecId)
        {
            _facialRecId = facialRecId;
        }

        public void GetSpeaker()
        {
            using(var connection = ConnectionManager.GetConnection())
            {
                connection.Open();
                GetSpeakerFromSQL(connection);
            }
        }

        private void GetSpeakerFromSQL(SqlConnection connection)
        {
            string tsql = String.Format("SELECT u.UserId, u.Name, l.Mode from users u inner join language l on u.LanguageId = l.LanguageId where FR_ID = '{0}' ", _facialRecId);

            using (var command = new SqlCommand(tsql, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ID = reader.GetInt32(0);
                        Name = reader.GetString(1);
                        Language_Code = reader.GetString(2);
                    }
                }
            }
        }
    }
}