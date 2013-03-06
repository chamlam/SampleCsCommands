using System;
using Rhino;
using Rhino.Commands;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("5c16e399-76ea-4361-824f-aa049e0211fc")]
  public class SampleCsCurveGetter : Command
  {
    public SampleCsCurveGetter()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsCurveGetter"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
      go.SetCommandPrompt("Select curves");
      go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      go.GetMultiple(1, 0);
      if (go.CommandResult() != Result.Success)
        return go.CommandResult();

      for (int i = 0; i < go.ObjectCount; i++)
      {
        Rhino.Geometry.Curve curve = go.Object(i).Curve();
        if (null == curve)
          return Result.Failure;

        if (null != curve as Rhino.Geometry.LineCurve)
          RhinoApp.WriteLine("Curve {0} is a line.", i);
        else if (null != curve as Rhino.Geometry.ArcCurve)
        {
          if (curve.IsClosed)
            RhinoApp.WriteLine("Curve {0} is a circle.", i);
          else
            RhinoApp.WriteLine("Curve {0} is an arc.", i);
        }
        else if (null != curve as Rhino.Geometry.PolylineCurve)
          RhinoApp.WriteLine("Curve {0} is a polyline.", i);
        else if (null != curve as Rhino.Geometry.PolyCurve)
          RhinoApp.WriteLine("Curve {0} is a polycurve.", i);
        else if (null != curve as Rhino.Geometry.NurbsCurve)
        {
          if (curve.IsEllipse())
          {
            if (curve.IsClosed)
              RhinoApp.WriteLine("Curve {0} is an ellipse.", i);
            else
              RhinoApp.WriteLine("Curve {0} is an elliptical arc.", i);
          }
          else
            RhinoApp.WriteLine("Curve {0} is a NURBS curve.", i);
        }
        else
          RhinoApp.WriteLine("Curve {0} is an unknown type.", i);
      }

      return Result.Success;
    }
  }
}
