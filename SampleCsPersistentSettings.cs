using System;
using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("7e24bc25-2c10-434b-b3a1-170e66e6a9d6")]
  public class SampleCsPersistentSettings : Command
  {
    private const bool BOOL_DEFAULT = true;
    private const int INT_DEFAULT = 1;
    private const double DBL_DEFAULT = 3.14;
    private const int LIST_DEFAULT = 3;

    /// <summary>
    /// Public constructor
    /// </summary>
    public SampleCsPersistentSettings()
    {
    }

    /// <summary>
    /// EnglishName override
    /// </summary>
    public override string EnglishName
    {
      get { return "SampleCsPersistentSettings"; }
    }

    /// <summary>
    /// RunCommand override
    /// </summary>
    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Get persistent settings (from Registry)
      PersistentSettings settings = this.Settings;
      bool bool_value = settings.GetBool("BoolValue", BOOL_DEFAULT);
      int int_value = settings.GetInteger("IntValue", INT_DEFAULT);
      double dbl_value = settings.GetDouble("DblValue", DBL_DEFAULT);
      int list_value = settings.GetInteger("ListValue", LIST_DEFAULT);

      GetPoint gp = new GetPoint();
      gp.SetCommandPrompt("GetPoint with options");

      Result rc = Result.Cancel;

      while (true)
      {
        gp.ClearCommandOptions();

        OptionToggle bool_option = new OptionToggle(bool_value, "Off", "On");
        OptionInteger int_option = new OptionInteger(int_value, 1, 99);
        OptionDouble dbl_option = new OptionDouble(dbl_value, 0, 99.9);
        string[] list_items = new string[5] { "Item0", "Item1", "Item2", "Item3", "Item4" };

        int bool_index = gp.AddOptionToggle("Boolean", ref bool_option);
        int int_index = gp.AddOptionInteger("Integer", ref int_option);
        int dbl_index = gp.AddOptionDouble("Double", ref dbl_option);
        int list_index = gp.AddOptionList("List", list_items, list_value);
        int reset_index = gp.AddOption("Reset");

        Rhino.Input.GetResult res = gp.Get();

        if (res == Rhino.Input.GetResult.Point)
        {
          doc.Objects.AddPoint(gp.Point());
          doc.Views.Redraw();
          rc = Result.Success;
        }
        else if (res == Rhino.Input.GetResult.Option)
        {
          CommandLineOption option = gp.Option();
          if (null != option)
          {
            if (option.Index == bool_index)
              bool_value = bool_option.CurrentValue;
            else if (option.Index == int_index)
              int_value = int_option.CurrentValue;
            else if (option.Index == dbl_index)
              dbl_value = dbl_option.CurrentValue;
            else if (option.Index == list_index)
              list_value = option.CurrentListOptionIndex;
            else if (option.Index == reset_index)
            {
              bool_value = BOOL_DEFAULT;
              int_value = INT_DEFAULT;
              dbl_value = DBL_DEFAULT;
              list_value = LIST_DEFAULT;
            }
          }
          continue;
        }

        break;
      }

      if (rc == Result.Success)
      {
        // Set persistent settings (to Registry)
        settings.SetBool("BoolValue", bool_value);
        settings.SetInteger("IntValue", int_value);
        settings.SetDouble("DblValue", dbl_value);
        settings.SetInteger("ListValue", list_value);
      }

      return rc;
    }
  }
}
