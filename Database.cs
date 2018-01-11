using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace AgsysServer
{
  class Database : IDisposable
  {
    private bool _IsError = false;
    private string _IdError = "";
    private string _ErrorInfo = "";

    private DbConnection _Connection = null;
    private DbTransaction _Transaction = null;

    private List<DatabaseQuery> _Query = new List<DatabaseQuery>();

    private string _ConnectionString = "";


    public bool IsError
    {
      get { return _IsError; }
      private set { _IsError = value; }
    }
    public string IdError
    {
      get { return _IdError; }
      private set { _IdError = value; }
    }
    public string ErrorInfo
    {
      get { return _ErrorInfo; }
      private set { _ErrorInfo = value; }
    }

    private DbConnection Connection
    {
      get { return _Connection; }
      set { _Connection = value; }
    }
    private DbTransaction Transaction
    {
      get { return _Transaction; }
      set { _Transaction = value; }
    }

    public bool IsConnect
    {
      get
      {
        bool r = false;
        if (Connection != null)
        {
          if (Connection.State == ConnectionState.Open) { r = true; }
        }
        return r;
      }
    }

    public string ConnectionString
    {
      get { return _ConnectionString; }
      set { _ConnectionString = value; }
    }

    public List<DatabaseQuery> Query
    {
      get { return _Query; }
      private set { _Query = value; }
    }

    public DatabaseQuery QueryCurrent
    {
      get
      {
        int i = (Query.Count - 1);
        if (i < 0) { return null; }
        else { return _Query[i]; }
      }
      set { }
    }
    public DatabaseQuery QueryLast
    {
      get { return QueryCurrent; }
      set { }
    }
    public DatabaseQuery Qc
    {
      get { return QueryCurrent; }
      set { }
    }


    public Database()
    {
    }
    public Database(string cs = "")
    {
      ConnectionString = cs;
      ConnectRun();
    }
    ~Database()
    {
      Final();
    }

    public void Dispose()
    {
      Final();
      GC.SuppressFinalize(this);
    }
    private void Final()
    {
      try
      {
        if (Transaction != null) { Transaction.Dispose(); }
        if (Connection != null) { Connection.Dispose(); }

        if (Query != null)
        {
          for (int i = 0; i < Query.Count; i++) { Query[i].Dispose(); }

          Query.Clear();
        }
      }
      catch(Exception ex) { Console.WriteLine(ex.Message); }
    }

    private void ClearError()
    {
      IsError = false;
      IdError = "";
      ErrorInfo = "";
    }

    public void Connect()
    {
      ConnectRun();
    }

    public void Connect(string cs = "")
    {
      if (!string.IsNullOrWhiteSpace(cs)) { ConnectionString = cs; }
      ConnectRun();
    }

    //BEGIN ConnectRun
    private void ConnectRun()
    {
      Final();
      ClearError();     

      if (string.IsNullOrWhiteSpace(ConnectionString))
      {
        IsError = true;
        IdError = "Database.Connect.E.10";
        ErrorInfo = IdError;
      }
      else
      {
        try
        {
          Connection = new SqlConnection();
          Connection.ConnectionString = ConnectionString;
          Connection.Open();
        }
        catch (Exception ex)
        {
          IsError = true;
          IdError = "Database.Connect.E.20";
          ErrorInfo = ex.Message;
        }
      }
    }

    public void Disconnect()
    {
      if (Connection != null) { Connection.Close(); }

      ClearError();
    }

    public void BeginTransaction()
    {
      if (IsConnect)
      {
        Rollback();

        try
        {
          Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }
        catch (Exception ex)
        {
          IsError = true;
          IdError = "Database.BeginTransaction.E.10";
          ErrorInfo = ex.Message;
        }
      }
    }

    public void Commit()
    {
      try
      {
        if (Transaction != null)
        {
          if (Transaction.Connection != null) { Transaction.Commit(); }
        }

        Transaction = null;
      }
      catch (Exception ex)
      {
        IsError = true;
        IdError = "Database.Commit.E.10";
        ErrorInfo = ex.Message;
      }
    }

    public void Rollback()
    {
      try
      {
        if (Transaction != null)
        {
          if (Transaction.Connection != null) { Transaction.Rollback(); }
        }

        Transaction = null;
      }
      catch (Exception ex)
      {
        IsError = true;
        IdError = "Database.Rollback.E.10";
        ErrorInfo = ex.Message;
      }
    }

    public int NewQuery(bool withTransaction = false)
    {
      if (Connection != null)
      {
        if (withTransaction) { BeginTransaction(); }

        Query.Add(new DatabaseQuery(Connection, Transaction));

        return (Query.Count - 1);
      }
      else { return -1; }
    }

  }

  enum DatabaseCommandType
  {
    Sql,
    Procedure
  }

  enum DatabaseParameterType
  {
    Number,
    Text,
    File,
    Table
  }


  class DatabaseQuery : IDisposable
  {
    private bool _IsError = false;
    private string _IdError = "";
    private string _ErrorInfo = "";

    private DbConnection Connection = null;
    private DbTransaction Transaction = null;
    private DbCommand Query = null;

    private DatabaseCommandType _CmdType = DatabaseCommandType.Sql;

    private DataTable _Table = null;

    private int _RowAmount = 0;
    private int _ColAmount = 0;

    private bool _CanClearPool = false;


    public bool IsError
    {
      get { return _IsError; }
      private set { _IsError = value; }
    }
    public string IdError
    {
      get { return _IdError; }
      private set { _IdError = value; }
    }
    public string ErrorInfo
    {
      get { return _ErrorInfo; }
      private set { _ErrorInfo = value; }
    }


    public DatabaseCommandType CmdType
    {
      get { return _CmdType; }
      set
      {
        if (value == DatabaseCommandType.Procedure)
        {
          Query.CommandType = CommandType.StoredProcedure;
          _CmdType = DatabaseCommandType.Procedure;
        }
        else
        {
          Query.CommandType = CommandType.Text;
          _CmdType = DatabaseCommandType.Sql;
        }
      }
    }

    public DataTable Table
    {
      get { return _Table; }
      private set { _Table = value; }
    }
    public int RowAmount
    {
      get { return _RowAmount; }
      private set { _RowAmount = value; }
    }
    public int ColAmount
    {
      get { return _ColAmount; }
      private set { _ColAmount = value; }
    }

    public bool CanClearPool
    {
      get { return _CanClearPool; }
      set { _CanClearPool = value; }
    }


    public DatabaseQuery(DbConnection c = null, DbTransaction t = null)
    {
      Connection = c;
      Transaction = t;

      Query = new SqlCommand();

      Query.Connection = Connection;
      Query.Transaction = Transaction;

      CmdType = DatabaseCommandType.Sql;
    }
    ~DatabaseQuery()
    {
      Final();
    }

    public void Dispose()
    {
      Final();
      GC.SuppressFinalize(this);
    }
    private void Final()
    {
      if (Table != null) { Table.Dispose(); }
      if (Query != null) { Query.Dispose(); }
    }

    //BEGIN ClearData
    private void ClearData()
    {
      Table = null;

      RowAmount = 0;
      ColAmount = 0;
    }
    //END ClearData


    //BEGIN ClearPool
    private void ClearPool()
    {
      try
      {
        if (CanClearPool) { SqlConnection.ClearPool((SqlConnection)Connection); }
      }
      catch (Exception ex)
      {
        IsError = true;
        IdError += "_AND_DatabaseQuery.ClearPool.E.10)";
        ErrorInfo += "_AND_" + ex.Message;
      }
    }
    //END ClearPool


    //BEGIN Open
    public void Open(string sql = "")
    {
      if (!IsError)
      {
        if (string.IsNullOrWhiteSpace(sql))
        {
          IsError = true;
          IdError = "DatabaseQuery.Open.E.10";
          ErrorInfo = IdError;
        }
      }

      if (!IsError)
      {
        ClearData();

        DbDataReader dataReader = null;

        try
        {
          Query.CommandText = sql.Trim();

          if (CmdType == DatabaseCommandType.Procedure) { Query.Prepare(); }

          dataReader = Query.ExecuteReader();

          Table = new DataTable();
          Table.Load(dataReader);

          RowAmount = Table.Rows.Count;
          ColAmount = Table.Columns.Count;
        }
        catch (Exception ex)
        {
          IsError = true;
          IdError = "DatabaseQuery.Open.E.20";
          ErrorInfo = ex.Message;

          ClearPool();
        }
        finally
        {
          if (dataReader != null) { dataReader.Close(); }
          Query.Parameters.Clear();
        }
      }
    }
    //END Open


    //BEGIN Exec
    public void Exec(string sql = "")
    {
      if (!IsError)
      {
        if (string.IsNullOrWhiteSpace(sql))
        {
          IsError = true;
          IdError = "DatabaseQuery.Exec.E.10";
          ErrorInfo = IdError;
        }
      }

      if (!IsError)
      {
        ClearData();

        try
        {
          Query.CommandText = sql.Trim();

          if (CmdType == DatabaseCommandType.Procedure) { Query.Prepare(); }

          RowAmount = Query.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
          IsError = true;
          IdError = "DatabaseQuery.Exec.E.20";
          ErrorInfo = ex.Message;

          ClearPool();
        }
        finally
        {
          Query.Parameters.Clear();
        }
      }
    }
    //END Exec


    public string Ssp(string s = "") //string (var) for sql prepare
    {
      if (!string.IsNullOrWhiteSpace(s)) { return s.Replace("'", "''"); }
      else { return ""; }
    }


    //BEGIN AddParameter
    public void AddParameter(DatabaseParameterType pType = DatabaseParameterType.Text, 
                             string pName = "", object pValue = null)
    {
      try
      {
        byte[] fileData = null; //byte[] fileData = { }; new byte[0]

        if (pType == DatabaseParameterType.File)
        {
          string fileName = pValue.ToString();

          if (System.IO.File.Exists(fileName))
          {
            using (System.IO.FileStream fs = new System.IO.FileStream(fileName,
                   System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
              fileData = new byte[fs.Length];
              fs.Read(fileData, 0, fileData.Length);
            }
          }
        }

        SqlParameter p = new SqlParameter();
        p.ParameterName = pName;

        if (pType == DatabaseParameterType.Text)
        {
          p.Value = pValue;
          p.SqlDbType = SqlDbType.NText;
        }
        else if (pType == DatabaseParameterType.Number)
        {
          p.Value = pValue;
          p.SqlDbType = SqlDbType.Decimal;
        }
        else if (pType == DatabaseParameterType.File)
        {
          p.Value = fileData;
          p.SqlDbType = SqlDbType.Image;
          //p.SqlDbType = SqlDbType.VarBinary;
          //p.Size = fileData.Length;
        }
        else if (pType == DatabaseParameterType.Table)
        {
          p.Value = pValue;
          p.SqlDbType = SqlDbType.Structured;
        }

        Query.Parameters.Add(p);
      }
      catch(Exception ex)
      {
        IsError = true;
        IdError = "DatabaseQuery.AddParameter.E.10";
        ErrorInfo = ex.Message;
      }
    }
    //END AddParameter


    public void SaveToFile(string fileName = "", string colName = "", int rowNumber = 0)
    {
      try
      {
        byte[] fileData = (byte[])Table.Rows[rowNumber][colName];
        System.IO.File.WriteAllBytes(fileName, fileData);
        //using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate))
        //{ fs.Write(fileData, 0, fileData.Length); }
      }
      catch(Exception ex)
      {
        IsError = true;
        IdError = "DatabaseQuery.SaveToFile.E.10";
        ErrorInfo = ex.Message;
      }
    }

  }




}

