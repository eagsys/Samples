using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AgsysServer
{

  public class DataAccessModel
  {
    public bool IsError = false;
    public string IdError = "";
    public string ErrorInfo = "";

    public string IdData = "";
    public List<DataAccessItemModel> Items = new List<DataAccessItemModel>();

    public DataAccessModel() { }
  }

  public class DataAccessItemModel
  {
    public string IdUser = "";
    public string UserName = "";

    public bool IsRead = false;
    public bool IsUpdate = false;
    public bool IsDelete = false;

    public bool IsSelect = false;

    public DataAccessItemModel() { }
  }


  partial class Data : IDisposable
  {

    //BEGIN Access
    public void Access(NameValueCollection clientData = null)
    {
      AccessModel = new DataAccessModel();
      List<DataAccessItemModel> items = new List<DataAccessItemModel>();

      if (clientData == null)
      {
        IsError = true;
        IdError = "Data.Access.E.100";
        ErrorInfo = IdError;
      }

      if (!IsError)
      {
        Id = clientData["IdData"];
        if (clientData["IsProcess"] == "y") { IsProcess = true; }

        if (Id == "0")
        {
          IsError = true;
          IdError = "Data.Access.E.200";
          ErrorInfo = IdError;
        }
      }

      //BEGIN isProcess
      if (!IsError)
      {
        if (IsProcess)
        {
          System.Data.DataTable dataTable = new System.Data.DataTable();
          dataTable.Columns.Add("f_iduser", typeof(int));
          dataTable.Columns.Add("f_owner", typeof(char));
          dataTable.Columns.Add("f_read", typeof(char));
          dataTable.Columns.Add("f_update", typeof(char));
          dataTable.Columns.Add("f_delete", typeof(char));

          //BEGIN add owner
          System.Data.DataRow dataRowOwner = dataTable.NewRow();
          dataRowOwner["f_iduser"] = IdUser;
          dataRowOwner["f_owner"] = 'y';
          dataRowOwner["f_read"] = 'y';
          dataRowOwner["f_update"] = 'y';
          dataRowOwner["f_delete"] = 'y';
          dataTable.Rows.Add(dataRowOwner);
          //END add owner

          //BEGIN add *
          string read_0 = clientData["read_0"]; if (read_0 != "y") { read_0 = "n"; }
          string update_0 = clientData["update_0"]; if (update_0 != "y") { update_0 = "n"; }
          string delete_0 = clientData["delete_0"]; if (delete_0 != "y") { delete_0 = "n"; }

          if ((update_0 == "y") || (delete_0 == "y")) { read_0 = "y"; }

          System.Data.DataRow dataRowAll = dataTable.NewRow();
          dataRowAll["f_iduser"] = 0;
          dataRowAll["f_owner"] = 'n';
          dataRowAll["f_read"] = read_0;
          dataRowAll["f_update"] = update_0;
          dataRowAll["f_delete"] = delete_0;
          dataTable.Rows.Add(dataRowAll);
          //END add *


          //BEGIN Get data for users
          int qUser = db.NewQuery();

          db.Query[qUser].AddParameter(DatabaseParameterType.Number, "@p_iddata", Id);
          db.Query[qUser].AddParameter(DatabaseParameterType.Number, "@p_iduser", IdUser);
          db.Query[qUser].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
          db.Query[qUser].CmdType = DatabaseCommandType.Procedure;
          db.Query[qUser].Open("dbo.pr_data_access_iduser");

          if (db.Query[qUser].IsError)
          {
            IsError = true;
            IdError = "Data.Access.E.300";
            ErrorInfo = db.Query[qUser].ErrorInfo;
          }

          if (!IsError)
          {
            for (int i = 0; i < db.Query[qUser].RowAmount; i++)
            {
              string f_iduser = db.Query[qUser].Table.Rows[i]["f_id"].ToString();
              string f_read = clientData["read_" + f_iduser]; if (f_read != "y") { f_read = "n"; }
              string f_update = clientData["update_" + f_iduser]; if (f_update != "y") { f_update = "n"; }
              string f_delete = clientData["delete_" + f_iduser]; if (f_delete != "y") { f_delete = "n"; }

              if ((f_update == "y") || (f_delete == "y")) { f_read = "y"; }

              System.Data.DataRow dataRow = dataTable.NewRow();
              dataRow["f_iduser"] = f_iduser;
              dataRow["f_owner"] = 'n';
              dataRow["f_read"] = f_read;
              dataRow["f_update"] = f_update;
              dataRow["f_delete"] = f_delete;
              dataTable.Rows.Add(dataRow);
            }
          }
          //END Get data for users


          //BEGIN Process
          if (!IsError)
          {
            bool isError = true;

            int qProcess = db.NewQuery();

            db.Query[qProcess].AddParameter(DatabaseParameterType.Number, "@p_iddata", Id);
            db.Query[qProcess].AddParameter(DatabaseParameterType.Table, "@p_table", dataTable);
            db.Query[qProcess].CmdType = DatabaseCommandType.Procedure;
            db.Query[qProcess].Open("dbo.pr_data_access_process");

            if (!db.Query[qProcess].IsError)
            {
              if (db.Query[qProcess].RowAmount == 1)
              {
                if (db.Query[qProcess].Table.Rows[0]["f_iserror"].ToString() == "n") { isError = false; }
              }
            }

            if (isError) { IsError = true; IdError = "Data.Access.E.400"; ErrorInfo = IdError; }
          }
          //END Process
        }
      }
      //END IsProcess


      //BEGIN Вывести данные из data_access
      if (!IsError)
      {
        int qList = db.NewQuery();

        db.Query[qList].AddParameter(DatabaseParameterType.Number, "@p_iddata", Id);
        db.Query[qList].AddParameter(DatabaseParameterType.Number, "@p_iduser", IdUser);
        db.Query[qList].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
        db.Query[qList].CmdType = DatabaseCommandType.Procedure;
        db.Query[qList].Open("dbo.pr_data_access_list");

        if (db.Query[qList].IsError)
        {
          IsError = true;
          IdError = "Data.Access.E.500";
          ErrorInfo = db.Query[qList].ErrorInfo;
        }

        if (!IsError)
        {
          bool isSelect = false;

          for (int i = 0; i < db.Query[qList].RowAmount; i++)
          {
            string f_iduser = db.Query[qList].Table.Rows[i]["f_id"].ToString();
            string f_name = db.Query[qList].Table.Rows[i]["f_name"].ToString();

            bool isRead = false; if (db.Query[qList].Table.Rows[i]["f_read"].ToString() == "y") { isRead = true; }
            bool isUpdate = false; if (db.Query[qList].Table.Rows[i]["f_update"].ToString() == "y") { isUpdate = true; }
            bool isDelete = false; if (db.Query[qList].Table.Rows[i]["f_delete"].ToString() == "y") { isDelete = true; }

            if (IsHtmlEncode) { f_name = HtmlEncode(f_name); }

            items.Add(new DataAccessItemModel { IdUser = f_iduser, UserName = f_name,
                                                IsRead = isRead, IsUpdate = isUpdate, IsDelete = isDelete, IsSelect = isSelect });

            if (isSelect) { isSelect = false; } else { isSelect = true; }
          }
        }
      }
      //END Вывести данные из data_access


      //BEGIN Установить AccessModel
      AccessModel.IsError = IsError;
      AccessModel.IdError = IdError;
      AccessModel.ErrorInfo = ErrorInfo;

      AccessModel.IdData = Id;
      AccessModel.Items = items;
      //END Установить AccessModel
    }
    //END Access

  }
}