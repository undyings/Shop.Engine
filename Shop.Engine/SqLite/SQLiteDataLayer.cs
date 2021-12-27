using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using Commune.Basis;

namespace Commune.Data
{
  public class SQLiteDataLayer : IDataLayer, IDisposable
  {
    object lockObj = new object();

    readonly string connectionString;
    public SQLiteDataLayer(string connectionString)
    {
      this.connectionString = connectionString;
    }

    //public string ConnectionString
    //{
    //  get
    //  {
    //    int threadId = Thread.CurrentThread.ManagedThreadId;
    //    SQLiteConnection dbConnection = DictionaryHlp.GetValueOrDefault(connectionByThreadId, threadId);
    //    if (dbConnection != null)
    //      return dbConnection.ConnectionString;
    //    return "";
    //  }
    //}

    readonly Dictionary<int, SQLiteConnection> connectionByThreadId = new Dictionary<int, SQLiteConnection>();

    public int ConnectionCount
    {
      get
      {
        lock (lockObj)
          return connectionByThreadId.Count;
      }
    }

    SQLiteConnection GetAndCheckConnectionForThread(string database)
    {
      int threadId = Thread.CurrentThread.ManagedThreadId;
      SQLiteConnection dbConnection = DictionaryHlp.GetValueOrDefault(connectionByThreadId, threadId);
      if (dbConnection == null || dbConnection.State == ConnectionState.Broken ||
        dbConnection.State == ConnectionState.Closed)
      {
        CloseConnection(dbConnection);

        //if (TrafficLogging.Default.DeveloperMode)
        //{
        //  TraceHlp2.AddMessage("Создаем подключение к базе данных SQLite '{0}' для потока '{1}'",
        //    connectionString, threadId);
        //}
        dbConnection = SQLiteHlp.OpenConnection(connectionString);
        connectionByThreadId[threadId] = dbConnection;
      }
      return dbConnection;
    }

    static void CloseConnection(SQLiteConnection dbConnection)
    {
      try
      {
        if (dbConnection != null && dbConnection.State == ConnectionState.Broken)
        {
          Logger.AddMessage("Закрываем подключение к базе данных SQLite");
          SQLiteConnection.ClearPool(dbConnection);
          dbConnection.Close();
          dbConnection.Dispose();
        }
      }
      catch (Exception ex)
      {
        Logger.WriteException(new Exception("Ошибка при закрытии подключения к базе данных SQLite", ex));
      }
    }

    public DataTable GetTable(string database, string query, params DbParameter[] parameters)
    {
      lock (lockObj)
      {
        SQLiteConnection dbConnection = GetAndCheckConnectionForThread(database);
        return SQLiteHlp.GetTable(query, dbConnection, parameters);
      }
    }

    public object GetScalar(string database, string query, params DbParameter[] parameters)
    {
      lock (lockObj)
      {
        SQLiteConnection dbConnection = GetAndCheckConnectionForThread(database);
        return SQLiteHlp.GetScalar(query, dbConnection, parameters);
      }
    }

    public void UpdateTable(string database, string query, DataTable table)
    {
      lock (lockObj)
      {
        SQLiteConnection dbConnection = GetAndCheckConnectionForThread(database);
        SQLiteHlp.UpdateTable(query, dbConnection, table);
      }
    }

    public string DbParamPrefix
    {
      get { return "@"; }
    }

    public void Dispose()
    {
      lock (lockObj)
      {
        foreach (SQLiteConnection connection in connectionByThreadId.Values)
        {
          try
          {
            SQLiteConnection.ClearPool(connection);
            connection.Close();
            connection.Dispose();
          }
          catch (Exception ex)
          {
            Logger.WriteException(ex, "Ошибка при закрытии sqlite подключения");
          }
        }
        connectionByThreadId.Clear();
      }
    }
  }
}
