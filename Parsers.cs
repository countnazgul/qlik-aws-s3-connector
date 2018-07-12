using QlikView.Qvx.QvxLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QlikAWSS3Connector
{
    class Parsers 
    {
        public static string GetTableName(string query, List<QvxTable> tables)
        {
            string tableName = "";

            var r = new Regex(@"(from|join)\s+(?<table>\S+)", RegexOptions.IgnoreCase);

            Match m = r.Match(query);
            while (m.Success)
            {
                tableName = m.Groups["table"].Value;
                m = m.NextMatch();
            }

            if (tableName.Length == 0)
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_TABLE_NOT_FOUND, "Table name is rquired");
            }

            bool foundTable = false;

            foreach(QvxTable a in tables)
            {
                if(a.TableName.ToLower() == tableName.ToLower())
                {
                    foundTable = true;
                }
            }

            if (foundTable == false)
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("Table '{0}' not found", tableName));
            }

            return tableName.ToLower();
        }

        public static IDictionary<string, string> GetWhereFields(string query, string tableName)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            try
            {
                var b = query.Substring(query.ToLower().IndexOf("where"));
                b = b.ToLower();
                b = b.Replace("where", "");

                var c = b.Split(new[] { "and" }, StringSplitOptions.None);

                for (int i = 0; i < c.Length; i++)
                {
                    var f = c[i].Trim();
                    var g = f.Split('=');

                    string value = g[1];
                    value = value.Replace("=", "").Trim();
                    value = value.Replace("'", "");

                    dict.Add(g[0].Trim(), value.Trim());
                }

            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("Error parsing the WHERE clause. WHERE clause is present?"));
            }

            if (dict.Count == 0 && tableName != "listbuckets")
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "WHERE clause is required. Please read the documentation");
            }

            return dict;
        }
    }
}
