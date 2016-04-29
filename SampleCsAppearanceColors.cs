using System.Drawing;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("69dc574e-98a0-42b2-8e6d-6101d7ed5273")]
  public class SampleCsAppearanceColors : Command
  {
    public override string EnglishName
    {
      get { return "SampleCsAppearanceColors"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Viewport colors
      PrintColor("Background", AppearanceSettings.ViewportBackgroundColor);
      PrintColor("Major grid line", AppearanceSettings.GridThickLineColor);
      PrintColor("Minor grid line", AppearanceSettings.GridThinLineColor);
      PrintColor("X-axis line", AppearanceSettings.GridXAxisLineColor);
      PrintColor("Y-axis line", AppearanceSettings.GridYAxisLineColor);
      PrintColor("Z-axis line", AppearanceSettings.GridZAxisLineColor);
      PrintColor("World axis icon X", AppearanceSettings.WorldCoordIconXAxisColor);
      PrintColor("World axis icon Y", AppearanceSettings.WorldCoordIconYAxisColor);
      PrintColor("World axis icon Z", AppearanceSettings.WorldCoordIconZAxisColor);
      PrintColor("Layout", AppearanceSettings.PageviewPaperColor);

      // Object display
      PrintColor("Selected objects", AppearanceSettings.SelectedObjectColor);
      PrintColor("Locked objects", AppearanceSettings.LockedObjectColor);
      PrintColor("New layer", AppearanceSettings.DefaultLayerColor);

      // Interface objects
      PrintColor("Feedback", AppearanceSettings.CrosshairColor);
      PrintColor("Tracking lines", AppearanceSettings.CrosshairColor);
      PrintColor("Crosshair", AppearanceSettings.CrosshairColor);

      // Layer dialog box
      PrintColor("Layout settings background", AppearanceSettings.CurrentLayerBackgroundColor);

      // General
      PrintColor("Window color 1", AppearanceSettings.GetPaintColor(PaintColor.NormalStart));
      PrintColor("Window color 2", AppearanceSettings.GetPaintColor(PaintColor.NormalEnd));
      PrintColor("Window color 3", AppearanceSettings.GetPaintColor(PaintColor.HotStart));
      PrintColor("Window border", AppearanceSettings.GetPaintColor(PaintColor.NormalBorder));
      PrintColor("Window text", AppearanceSettings.GetPaintColor(PaintColor.TextEnabled));
      PrintNoColor("Active viewport title");
      PrintNoColor("Inactive viewport title");

      // Widget colors
      PrintNoColor("U-axis");
      PrintNoColor("V-axis");
      PrintNoColor("W-axis");

      return Result.Success;
    }

    protected void PrintColor(string label, Color color)
    {
      RhinoApp.WriteLine("{0} - {1}", label, color.Name);
    }

    protected void PrintNoColor(string label)
    {
      RhinoApp.WriteLine("{0} - not available", label);
    }

  }
}
