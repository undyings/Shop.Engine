using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Commune.Data
{
  public class SQLiteDatabasePull : IDataLayer, IDisposable
  {
    readonly string connectionStringFormat;
    public string DataPath;
    public SQLiteDatabasePull(string connectionStringFormat, string dataPath)
    {
      this.connectionStringFormat = connectionStringFormat;
      this.DataPath = dataPath;
    }

    readonly object lockObj = new object();
    readonly Dictionary<string, SQLiteDataLayer> connectionByDatabase = 
      new Dictionary<string, SQLiteDataLayer>();

    IDataLayer GetOrCreateConnection(string database)
    {
      lock (lockObj)
      {
        SQLiteDataLayer connection;
        if (!connectionByDatabase.TryGetValue(database, out connection))
        {
          connection = new SQLiteDataLayer(string.Format(connectionStringFormat,
            Path.Combine(DataPath, database)));
          connectionByDatabase[database] = connection;
        }
        return connection;
      }
    }

    public int ConnectionCount
    {
      get
      {
        int count = 0;
        lock (lockObj)
        {
          foreach (SQLiteDataLayer connection in connectionByDatabase.Values)
            count += connection.ConnectionCount;
        }
        return count;
      }
    }

    public void CloseConnection(string database)
    {
      SQLiteDataLayer connection;
      if (connectionByDatabase.TryGetValue(database, out connection))
        connection.Dispose();
      connectionByDatabase.Remove(database);
    }

    public object GetScalar(string database, string query, params DbParameter[] parameters)
    {
      IDataLayer connection = GetOrCreateConnection(database);
      return connection.GetScalar(database, query, parameters);
    }

    public System.Data.DataTable GetTable(string database, string query, params DbParameter[] parameters)
    {
      IDataLayer connection = GetOrCreateConnection(database);
      return connection.GetTable(database, query, parameters);
    }

    public void UpdateTable(string database, string query, System.Data.DataTable table)
    {
      IDataLayer connection = GetOrCreateConnection(database);
      connection.UpdateTable(database, query, table);
    }

    public string DbParamPrefix
    {
      get { return "@"; }
    }

    public void Dispose()
    {
      lock (lockObj)
      {
        foreach (SQLiteDataLayer connection in connectionByDatabase.Values)
          connection.Dispose();
        connectionByDatabase.Clear();
      }
    }
  }
}
