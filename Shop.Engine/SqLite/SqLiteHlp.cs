using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace Commune.Data
{
  public class SQLiteHlp
  {
    public static string LimitCondition(int offset, int count)
    {
      return string.Format("limit {0} offset {1}", count, offset);
    }

    public static SQLiteConnection OpenConnection(string connectionString)
    {
      SQLiteConnection connection = new SQLiteConnection(connectionString);
      connection.Open();
      return connection;
    }

    public static DataTable GetTable(string query, SQLiteConnection dbConnection, params DbParameter[] parameters)
    {
      SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, dbConnection);
      {
        foreach (DbParameter parameter in parameters)
          adapter.SelectCommand.Parameters.Add(new SQLiteParameter(parameter.Name, parameter.Value));

        DataTable table = new DataTable();
        adapter.Fill(table);
        return table;
      }
    }

    public static object GetScalar(string query, SQLiteConnection dbConnection, params DbParameter[] parameters)
    {
      SQLiteCommand command = new SQLiteCommand(query, dbConnection);
      {
        foreach (DbParameter parameter in parameters)
          command.Parameters.Add(new SQLiteParameter(parameter.Name, parameter.Value));

        return command.ExecuteScalar();
      }
    }

    public static void UpdateTable(string query, SQLiteConnection dbConnection, DataTable table)
    {
      //object maxId = null;
      //if (query.ToLower().Contains("from traffic_object"))
      //  maxId = GetScalar("Select max(obj_id) From traffic_object", dbConnection);

      using (SQLiteTransaction transaction = dbConnection.BeginTransaction())
      {
        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, dbConnection))
        {
          using (SQLiteCommand command = dbConnection.CreateCommand())
          {
            command.Transaction = transaction;
            command.CommandText = query;
            adapter.SelectCommand = command;

            using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder())
            {
              builder.DataAdapter = adapter;
              adapter.Update(table);

              transaction.Commit();
            }
          }
        }
      }
    }

    public static bool TableExist(IDataLayer dbConnection, string database, string tableName)
    {
      object rawCount = dbConnection.GetScalar(database,
        "Select count(*) From sqlite_master Where name = @tableName",
        new DbParameter("tableName", tableName));

      return Convert.ToInt32(rawCount) != 0;
    }

    //public static void UpdateTable(string query, SQLiteConnection dbConnection, DataTable table)
    //{
    //  SQLiteDataAdapter adapter = new SQLiteDataAdapter(string.Format("{0}", query), dbConnection);
    //  using (new SQLiteCommandBuilder(adapter))
    //  {
    //    adapter.Update(table);
    //  }
    //}
  }
}
