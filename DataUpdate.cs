using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AgsysServer
{
  //BEGIN DataUpdateModel
  public class DataUpdateModel
  {
    public bool IsError = false;
    public string IdError = "";
    public string ErrorInfo = "";

    public bool IsProcess = false;

    public string IdData = "";
    public string TypeName = "";

    public List<DataColModel> ColList = null;

    public DataUpdateModel() { }
  }
  //END DataUpdateModel

  partial class Data : IDisposable
  {
    //BEGIN Update
    public void Update(NameValueCollection clientData = null)
    {
      IsModeList = false;
      IsModeAdd = false;
      IsModeUpdate = true;
      IsModeDelete = false;

      if (clientData != null) { ClientData = clientData; }

      if (ClientData == null)
      {
        IsError = true;
        IdError = "Data.Update.E.100";
        ErrorInfo = IdError;
      }
      else
      {
        if (ClientData["IsProcess"] == "y") { IsProcess = true; }
        Id = ClientData["IdData"];
      }

      //BEGIN Check
      if (!IsError)
      {
        bool isError = true;

        int atNum = 3; //Проверить доступ на чтение
        if (IsProcess) { atNum = 4; } //Если процесс, то проверить доступ на обновление

        int q1 = db.NewQuery();

        db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_iddata", Id);
        db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_iduser", IdUser);
        db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_access", atNum);
        db.Query[q1].CmdType = DatabaseCommandType.Procedure;
        db.Query[q1].Open("dbo.pr_data_check");

        if (!db.Query[q1].IsError)
        {
          if (db.Query[q1].RowAmount == 1)
          {
            string f_iserror = db.Query[q1].Table.Rows[0]["f_iserror"].ToString();
            string f_idtype = db.Query[q1].Table.Rows[0]["f_idtype"].ToString();
            string f_typename = db.Query[q1].Table.Rows[0]["f_typename"].ToString();
            string f_typeview = db.Query[q1].Table.Rows[0]["f_typeview"].ToString();

            if (f_iserror == "n")
            {
              isError = false;

              IdType = f_idtype;
              TypeName = f_typename;

              if (!string.IsNullOrWhiteSpace(f_typeview)) { IsView = true; View = f_typeview; }
            }
          }
        }

        if (isError)
        {
          IsError = true;
          IdError = "Data.Update.E.200";
          ErrorInfo = IdError;
        }
      }
      //END Check


      //BEGIN ColList
      if (!IsError)
      {
        GetColList();

        if (ColList.Count < 1)
        {
          IsError = true;
          IdError = "Data.Update.E.300";
          ErrorInfo = IdError;
        }
      }
      //END ColList


      //BEGIN IsProcess
      if (IsProcess)
      {
        if (!IsError)
        {
          bool isLock = false; //Для блокировки записи по Id

          //BEGIN try
          try
          {
            //BEGIN Пытаемся заблокировать запись
            if (!DataLock.Add(Id))
            {
              IsError = true;
              IdError = "Data.Update.E.500";
              ErrorInfo = IdError;
            }
            else { isLock = true; }
            //END Пытаемся заблокировать запись

            //BEGIN Data Lock
            if (!IsError)
            {
              if (!GetCurrentData())
              {
                IsError = true;
                IdError = "Data.Update.E.600";
                ErrorInfo = IdError;
              }
              else
              {
                if (!CheckView(CurrentData))
                {
                  IsError = true;
                  IdError = "Data.Update.E.650";
                  ErrorInfo = IdError;
                }
              }

              //BEGIN Preparing to update data
              if (!IsError)
              {
                SetEditData();

                if (!CheckEditData())
                {
                  IsError = true;
                  IdError = "Data.Update.E.700";
                  ErrorInfo = IdError;
                }
                //BEGIN Check Step
                else
                {
                  if (!CheckStep())
                  {
                    IsError = true;
                    IdError = "Data.Update.E.800";
                    ErrorInfo = IdError;
                  }
                }
                //END Check Step
              }
              //END Preparing to update data

              //BEGIN Update Data
              if (!IsError)
              {
                ................................
              }
              //END Update Data
            }
            //END Data Lock
          }
          //END try
          //BEGIN catch finally
          catch(Exception ex)
          {
            IsError = true;
            IdError = "Data.Update.E.1000";
            ErrorInfo = ex.Message;
          }
          finally { if (isLock) { DataLock.Delete(Id); } }
          //END catch finally
        }
      }
      //END IsProcess

      //BEGIN Show
      if ((IsError) || (!IsProcess))
      {
        if (!IsProcess)
        {
          GetCurrentData(true);

          if (!CheckView(EditData))
          {
            for (int i = 0; i < ColList.Count; i++) { EditData[ColList[i].Id] = "xxx"; }
          }
        }

        SetSendData();
      } 
      //END Show

      //BEGIN UpdateModel
      UpdateModel = new DataUpdateModel();

      UpdateModel.IsError = IsError;
      UpdateModel.IdError = IdError;
      UpdateModel.ErrorInfo = ErrorInfo;

      UpdateModel.IsProcess = IsProcess;

      UpdateModel.IdData = Id;

      if (IsHtmlEncode) { UpdateModel.TypeName = HtmlEncode(TypeName); }
      else { UpdateModel.TypeName = TypeName; }

      UpdateModel.ColList = ColList;
      //END UpdateModel
    }
    //END Update


    //BEGIN CheckMaxUpdate
    private bool CheckMaxUpdate(string idCol = "0", string maxUpdate = "0")
    {
      bool r = false;

      int q1 = db.NewQuery();

      db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_iddata", Id);
      db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_idcol", idCol);
      db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_maxupdate", maxUpdate);
      db.Query[q1].CmdType = DatabaseCommandType.Procedure;
      db.Query[q1].Open("dbo.pr_data_maxupdate");

      if (!db.Query[q1].IsError)
      {
        if (db.Query[q1].RowAmount == 1)
        {
          if (db.Query[q1].Table.Rows[0]["f_iserror"].ToString() == "n") { r = true; }
        }
      }

      return r;
    }
    //END CheckMaxUpdate

  }
}
