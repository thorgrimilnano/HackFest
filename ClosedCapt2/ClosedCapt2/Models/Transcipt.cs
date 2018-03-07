using ClosedCapt2.SQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ClosedCapt2.Models
{
    public class Transcipt
    {
        public string Text {get;set;}
        public int ID { get; private set; }

        public void CreateNewTranscript(int session)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                connection.Open();
                var tsql = String.Format("INSERT INTO transcript (Session, NativeTextBlob, TranslatedTextBlob) output INSERTED.TranscriptId values ({0}, '', '')", session);

                using(var command = new SqlCommand(tsql, connection))
                {
                    ID = (int) command.ExecuteScalar();
                }
            }
        }

        public void AppendToTranscript(int transcriptId, string text)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                connection.Open();
                var tsql = String.Format("UPDATE transcript SET TranslatedTextBlob = TranslatedTextBlob + ' {0}' where Session = {1}", text, transcriptId);

                using(var command = new SqlCommand(tsql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}