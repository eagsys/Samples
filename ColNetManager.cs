using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AgsysServer
{
  //BEGIN Models for ColNet
  public class ColNetListModel
  {
    public bool IsError = false;
    public string IdError = "";
    public string ErrorInfo = "";

    public List<ColNetListItemModel> Items = null;

    public ColNetListModel() { }
  }

  public class ColNetListItemModel
  {
    public string Id = "";
    public string Name = "";
    public string Sort = "";

    public bool IsText = false;
    public bool IsNumber = false;
    public bool IsDate = false;

    public bool IsActive = false;
    public bool IsSelect = false;

    public ColNetListItemModel() { }
  }

  public class ColNetEditModel
  {
    public bool IsError = false;
    public string IdError = "";
    public string ErrorInfo = "";

    public string Id = "";
    public string Name = "";
    public string Info = "";
    public string Sort = "";
    public string Replace = "";

    public bool IsText = false;
    public bool IsNumber = false;
    public bool IsDate = false;

    public bool IsActive = false;
    public bool IsProcess = false;

    public ColNetEditModel() { }
  }
  //END Models for ColNet

  class ColNetManager : IDisposable
  {
    private bool _IsError = false;
    private string _IdError = "";
    private string _ErrorInfo = "";

    private Common common = null;
    private Database db = null;

    private string _DbConnectionString = "";
    private bool _IsInit = false;

    private string _Id = "";
    private string _IdGroup = "";
    private string _IdCtype = "";
    private string _Active = "";
    private string _Name = "";
    private string _Info = "";
    private string _Sort = "";
    private string _Replace = "";
    private string _DtAdd = "";

    private bool _IsProcess = false;
    private bool _IsHtmlEncode = true;

    private ColNetListModel _ListModel = null;
    private ColNetEditModel _EditModel = null;

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

    public string DbConnectionString
    {
      get { return _DbConnectionString; }
      set { _DbConnectionString = value; }
    }
    public bool IsInit
    {
      get { return _IsInit; }
      private set { _IsInit = value; }
    }

    public string Id
    {
      get { return _Id; }
      set
      {
        _Id = "0";
        if (common.IsStrNum(value, "i")) { if (int.Parse(value) > 0) { _Id = value; } }
      }
    }
    public string IdGroup
    {
      get { return _IdGroup; }
      set
      {
        _IdGroup = "0";
        if (common.IsStrNum(value, "i")) { if (int.Parse(value) > 0) { _IdGroup = value; } }
      }
    }
    public string IdCtype
    {
      get { return _IdCtype; }
      set
      {
        _IdCtype = "10";
        if (value == "20") { _IdCtype = "20"; }
        else if (value == "30") { _IdCtype = "30"; }
      }
    }
    public string Active
    {
      get { return _Active; }
      set { if (value == "y") { _Active = "y"; } else { _Active = "n"; } }
    }
    public string Name
    {
      get { return _Name; }
      set
      {
        _Name = "";
        if (!string.IsNullOrWhiteSpace(value)) { _Name = common.Leftt(value, 50); }
      }
    }
    public string Info
    {
      get { return _Info; }
      set
      {
        _Info = "";
        if (!string.IsNullOrWhiteSpace(value)) { _Info = common.Leftt(value, 4000); }
      }
    }
    public string Sort
    {
      get { return _Sort; }
      set
      {
        _Sort = "0";
        if (common.IsStrNum(value, "i")) { if (int.Parse(value) > 0) { _Sort = value; } }
      }
    }
    public string Replace
    {
      get { return _Replace; }
      set
      {
        _Replace = "";
        if (!string.IsNullOrWhiteSpace(value)) { _Replace = common.Leftt(value, 4000); }
      }
    }
    public string DtAdd
    {
      get { return _DtAdd; }
      private set { _DtAdd = value; }
    }

    public bool IsProcess
    {
      get { return _IsProcess; }
      set { _IsProcess = value; }
    }
    public bool IsHtmlEncode
    {
      get { return _IsHtmlEncode; }
      set { _IsHtmlEncode = value; }
    }

    public ColNetListModel ListModel
    {
      get { return _ListModel; }
      private set { _ListModel = value; }
    }
    public ColNetEditModel EditModel
    {
      get { return _EditModel; }
      private set { _EditModel = value; }
    }



    public ColNetManager()
    {
    }
    public ColNetManager(CurrentConfig cConfig = null)
    {
      Init(cConfig.DbConnectionString);

      IdGroup = cConfig.IdGroup;
    }
    ~ColNetManager()
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
      if (db != null) { db.Dispose(); }
    }

    public void Init(string dbConnectionString = "")
    {
      if (!IsInit)
      {
        if (!string.IsNullOrWhiteSpace(dbConnectionString)) { DbConnectionString = dbConnectionString; }

        common = new Common();
        db = new Database(DbConnectionString);

        ClearError();
        ClearData();

        IsInit = true;
      }
    }


    public void ClearError()
    {
      IsError = false;
      IdError = "";
      ErrorInfo = "";
    }

    public void ClearData()
    {
      Id = "";
      IdGroup = "";
      IdCtype = "";
      Active = "";
      Name = "";
      Info = "";
      Sort = "";
      Replace = "";
      DtAdd = "";

      IsProcess = false;
      IsHtmlEncode = true;

      ListModel = null;
      EditModel = null;
    }




    //BEGIN GetList
    public void GetList(NameValueCollection clientData = null)
    {
      ListModel = new ColNetListModel();
      List<ColNetListItemModel> items = new List<ColNetListItemModel>();

      int q1 = db.NewQuery();

      db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
      db.Query[q1].CmdType = DatabaseCommandType.Procedure;
      db.Query[q1].Open("pr_colnet_list");

      if (!db.Query[q1].IsError)
      {
        bool isSelect = false;

        for (int i = 0; i < db.Query[q1].RowAmount; i++)
        {
          string f_id = db.Query[q1].Table.Rows[i]["f_id"].ToString();
          string f_idctype = db.Query[q1].Table.Rows[i]["f_idctype"].ToString();
          string f_active = db.Query[q1].Table.Rows[i]["f_active"].ToString();
          string f_name = db.Query[q1].Table.Rows[i]["f_name"].ToString();
          string f_sort = db.Query[q1].Table.Rows[i]["f_sort"].ToString();

          bool isText = false; bool isNumber = false; bool isDate = false;

          if (f_idctype == "10") { isText = true; }
          else if (f_idctype == "20") { isNumber = true; }
          else if (f_idctype == "30") { isDate = true; }

          bool isActive = false; if (f_active == "y") { isActive = true; }

          if (IsHtmlEncode) { f_name = HtmlEncode(f_name); }

          items.Add(new ColNetListItemModel { Id = f_id, Name = f_name, Sort = f_sort,
                                              IsText = isText, IsNumber = isNumber, IsDate = isDate,
                                              IsActive = isActive, IsSelect = isSelect });

          if (isSelect) { isSelect = false; } else { isSelect = true; }
        }
      }
      else
      {
        IsError = true;
        IdError = "ColNetManager.GetList.E.100";
        ErrorInfo = db.Query[q1].ErrorInfo;
      }

      //BEGIN Set ListModel
      ListModel.IsError = IsError;
      ListModel.IdError = IdError;
      ListModel.ErrorInfo = ErrorInfo;

      ListModel.Items = items;
      //END Set ListModel
    }
    //END GetList


    //BEGIN Add
    public void Add(NameValueCollection clientData = null)
    {
      if (clientData != null)
      {
        if (clientData["IsProcess"] == "y") { IsProcess = true; }

        IdCtype = clientData["IdCtype"];
        Active = clientData["ColNetActive"];
        Name = clientData["ColNetName"];
        Info = clientData["ColNetInfo"];
        Sort = clientData["ColNetSort"];
        Replace = clientData["ColNetReplace"];
      }
      else
      {
        IsError = true;
        IdError = "ColNetManager.Add.E.100";
        ErrorInfo = IdError;
      }

      if (!IsError)
      {
        if (IsProcess)
        {
          if (string.IsNullOrWhiteSpace(Name))
          {
            IsError = true;
            IdError = "ColNetManager.Add.E.200";
            ErrorInfo = IdError;
          }
        }
      }

      //BEGIN IsProcess
      if (!IsError)
      {
        if (IsProcess)
        {
          int qAdd = db.NewQuery();

          db.Query[qAdd].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
          db.Query[qAdd].AddParameter(DatabaseParameterType.Number, "@p_idctype", IdCtype);
          db.Query[qAdd].AddParameter(DatabaseParameterType.Text, "@p_active", Active);
          db.Query[qAdd].AddParameter(DatabaseParameterType.Text, "@p_name", Name);
          db.Query[qAdd].AddParameter(DatabaseParameterType.Text, "@p_info", Info);
          db.Query[qAdd].AddParameter(DatabaseParameterType.Number, "@p_sort", Sort);
          db.Query[qAdd].AddParameter(DatabaseParameterType.Text, "@p_replace", Replace);
          db.Query[qAdd].CmdType = DatabaseCommandType.Procedure;
          db.Query[qAdd].Open("pr_colnet_add");

          if (db.Query[qAdd].IsError)
          {
            IsError = true;
            IdError = "ColNetManager.Add.E.300";
            ErrorInfo = db.Query[qAdd].ErrorInfo;
          }

          if (!IsError)
          {
            if (db.Query[qAdd].RowAmount != 1)
            {
              IsError = true;
              IdError = "ColNetManager.Add.E.400";
              ErrorInfo = IdError;
            }
          }

          if (!IsError)
          {
            string f_iserror = db.Query[qAdd].Table.Rows[0]["f_iserror"].ToString();
            string f_id = db.Query[qAdd].Table.Rows[0]["f_id"].ToString();

            if (f_iserror == "n") { Id = f_id; }
            else
            {
              IsError = true;
              IdError = "ColNetManager.Add.E.500";
              ErrorInfo = IdError;
            }
          }
        }
      }
      //END IsProcess

      SetEditModel();
    }
    //END Add


    //BEGIN Update
    public void Update(NameValueCollection clientData = null)
    {
      if (clientData == null)
      {
        IsError = true;
        IdError = "ColNetManager.Update.E.100";
        ErrorInfo = IdError;
      }

      if (!IsError)
      {
        Id = clientData["IdColNet"];

        if (Id == "0")
        {
          IsError = true;
          IdError = "ColNetManager.Update.E.200";
          ErrorInfo = IdError;
        }
      }

      if (!IsError)
      {
        if (clientData["IsProcess"] == "y")
        {
          IsProcess = true;

          IdCtype = clientData["IdCtype"];
          Active = clientData["ColNetActive"];
          Name = clientData["ColNetName"];
          Info = clientData["ColNetInfo"];
          Sort = clientData["ColNetSort"];
          Replace = clientData["ColNetReplace"];

          if (string.IsNullOrWhiteSpace(Name))
          {
            IsError = true;
            IdError = "ColNetManager.Update.E.300";
            ErrorInfo = IdError;
          }
        }
      }

      //BEGIN !IsError
      if (!IsError)
      {
        //BEGIN IsProcess
        if (IsProcess)
        {
          int qProcess = db.NewQuery();

          db.Query[qProcess].AddParameter(DatabaseParameterType.Number, "@p_id", Id);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Number, "@p_idctype", IdCtype);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Text, "@p_active", Active);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Text, "@p_name", Name);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Text, "@p_info", Info);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Number, "@p_sort", Sort);
          db.Query[qProcess].AddParameter(DatabaseParameterType.Text, "@p_replace", Replace);
          db.Query[qProcess].CmdType = DatabaseCommandType.Procedure;
          db.Query[qProcess].Open("pr_colnet_update");

          if (db.Query[qProcess].IsError)
          {
            IsError = true;
            IdError = "ColNetManager.Update.E.400";
            ErrorInfo = db.Query[qProcess].ErrorInfo;
          }

          if (!IsError)
          {
            if (db.Query[qProcess].RowAmount != 1)
            {
              IsError = true;
              IdError = "ColNetManager.Update.E.500";
              ErrorInfo = IdError;
            }
          }

          if (!IsError)
          {
            string f_iserror = db.Query[qProcess].Table.Rows[0]["f_iserror"].ToString();

            if (f_iserror == "n") { }
            else
            {
              IsError = true;
              IdError = "ColNetManager.Update.E.600";
              ErrorInfo = IdError;
            }
          }
        }
        //END isProcess



        //BEGIN Get data for show
        int qShow = db.NewQuery();

        db.Query[qShow].AddParameter(DatabaseParameterType.Number, "@p_id", Id);
        db.Query[qShow].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
        db.Query[qShow].CmdType = DatabaseCommandType.Procedure;
        db.Query[qShow].Open("pr_colnet_show");

        if (db.Query[qShow].IsError)
        {
          IsError = true;
          IdError = "ColNetManager.Update.E.700";
          ErrorInfo = db.Query[qShow].ErrorInfo;
        }

        if (!IsError)
        {
          if (db.Query[qShow].RowAmount != 1)
          {
            IsError = true;
            IdError = "ColNetManager.Update.E.800";
            ErrorInfo = IdError;
          }
        }

        if (!IsError)
        {
          IdCtype = db.Query[qShow].Table.Rows[0]["f_idctype"].ToString();
          Active = db.Query[qShow].Table.Rows[0]["f_active"].ToString();
          Name = db.Query[qShow].Table.Rows[0]["f_name"].ToString();
          Info = db.Query[qShow].Table.Rows[0]["f_info"].ToString();
          Sort = db.Query[qShow].Table.Rows[0]["f_sort"].ToString();
          Replace = db.Query[qShow].Table.Rows[0]["f_replace"].ToString();
        }
        //END Get data for show
      }
      //END !IsError

      SetEditModel();
    }
    //END Update


    //BEGIN Set EditModel
    private void SetEditModel()
    {
      EditModel = new ColNetEditModel();

      EditModel.IsError = IsError;
      EditModel.IdError = IdError;
      EditModel.ErrorInfo = ErrorInfo;

      EditModel.Id = Id;

      if (IsHtmlEncode)
      {
        EditModel.Name = HtmlEncode(Name);
        EditModel.Info = HtmlEncode(Info);
        EditModel.Replace = HtmlEncode(Replace);
      }
      else
      {
        EditModel.Name = Name;
        EditModel.Info = Info;
        EditModel.Replace = Replace;
      }

      if (Sort != "0") { EditModel.Sort = Sort; } else { EditModel.Sort = ""; }

      if (IdCtype == "10") { EditModel.IsText = true; }
      else if (IdCtype == "20") { EditModel.IsNumber = true; }
      else if (IdCtype == "30") { EditModel.IsDate = true; }

      if (Active == "y") { EditModel.IsActive = true; } else { EditModel.IsActive = false; }
      EditModel.IsProcess = IsProcess;
    }
    //END Set EditModel


    //BEGIN Delete
    public void Delete(NameValueCollection clientData = null)
    {
      if (clientData == null)
      {
        IsError = true;
        IdError = "ColNetManager.Delete.E.100";
        ErrorInfo = IdError;
      }

      if (!IsError)
      {
        Id = clientData["IdColNet"];

        int q1 = db.NewQuery();

        db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_id", Id);
        db.Query[q1].AddParameter(DatabaseParameterType.Number, "@p_idgroup", IdGroup);
        db.Query[q1].CmdType = DatabaseCommandType.Procedure;
        db.Query[q1].Open("pr_colnet_delete");

        if (db.Query[q1].IsError)
        {
          IsError = true;
          IdError = "ColManager.Delete.E.200";
          ErrorInfo = db.Query[q1].ErrorInfo;
        }
      }
    }
    //END Delete


    //BEGIN HtmlEncode
    private string HtmlEncode(string html = "")
    {
      if (!string.IsNullOrWhiteSpace(html)) { return System.Web.HttpUtility.HtmlEncode(html); }
      else { return ""; }
    }
    //END HtmlEncode
  }
}
