using System;
using Rhino;
using Rhino.Commands;

namespace SampleCsCommands
{
  /// <summary>
  /// Escape key event handler helper class
  /// </summary>
  class EscapeKeyEventHandler : IDisposable
  {
    private bool _escapeKeyPressed = false;

    public EscapeKeyEventHandler(string message)
    {
      Rhino.RhinoApp.EscapeKeyPressed += new EventHandler(RhinoApp_EscapeKeyPressed);
      Rhino.RhinoApp.WriteLine(message);
    }

    public bool EscapeKeyPressed
    {
      get
      {
        Rhino.RhinoApp.Wait(); // "pumps" the Rhino message queue
        return _escapeKeyPressed;
      }
    }

    private void RhinoApp_EscapeKeyPressed(object sender, EventArgs e)
    {
      _escapeKeyPressed = true;
    }

    public void Dispose()
    {
      Rhino.RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;
    }
  }

  [System.Runtime.InteropServices.Guid("ba3fb034-00ab-48da-9d69-c495b7d4f90d")]
  public class SampleCsEscapeKey2 : Command
  {
    private Random _random;

    public SampleCsEscapeKey2()
    {
      _random = new Random();
    }

    public override string EnglishName
    {
      get { return "SampleCsEscapeKey2"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      EscapeKeyEventHandler handler = new EscapeKeyEventHandler("Press <Esc> to cancel");

      for (int i = 0; i < 1000; i++)
      {
        if (handler.EscapeKeyPressed)
        {
          Rhino.RhinoApp.WriteLine("Command canceled");
          break;
        }

        double x = (double)_random.Next(0, 100);
        double y = (double)_random.Next(0, 100);
        double z = (double)_random.Next(0, 100);
        Rhino.Geometry.Point3d pt = new Rhino.Geometry.Point3d(x, y, z);
        doc.Objects.AddPoint(pt);
        doc.Views.Redraw();
      }

      return Result.Success;
    }
  }
}
