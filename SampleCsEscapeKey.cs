using System;
using Rhino;
using Rhino.Commands;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("9c581028-0837-4f26-a7a0-4465d2975ad9")]
  public class SampleCsEscapeKey : Command
  {
    bool _escapeKeyPressed;

    public SampleCsEscapeKey()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsEscapeKey"; }
    }

    void RhinoApp_EscapeKeyPressed(object sender, EventArgs e)
    {
      _escapeKeyPressed = true;
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      _escapeKeyPressed = false;

      // Add our escape key event handler
      RhinoApp.EscapeKeyPressed += new EventHandler(RhinoApp_EscapeKeyPressed);

      for (int i = 0; i < 10000; i++)
      {
        // Pauses to keep Windows message pump alive
        RhinoApp.Wait();

        // Check out cancel flag
        if (_escapeKeyPressed)
          break;

        RhinoApp.WriteLine(string.Format("Hello {0}", i));
      }

      // Remove it when finished
      RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;

      return Result.Success;
    }
  }
}
