using System;
using Rhino;
using Rhino.Commands;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("87d8544e-c7de-47a2-a671-cdb69a382ba9")]
  public class SampleCsOrientPerpendicularToCurve : Command
  {
    public SampleCsOrientPerpendicularToCurve()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsOrientPerpendicularToCurve"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Select objects to orient
      Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
      go.SetCommandPrompt("Select objects to orient");
      go.SubObjectSelect = false;
      go.GroupSelect = true;
      go.GetMultiple(1, 0);
      if (go.CommandResult() != Rhino.Commands.Result.Success)
        return go.CommandResult();

      // Point to orient from
      Rhino.Input.Custom.GetPoint gp = new Rhino.Input.Custom.GetPoint();
      gp.SetCommandPrompt("Point to orient from");
      gp.Get();
      if (gp.CommandResult() != Rhino.Commands.Result.Success)
        return gp.CommandResult();

      // Define source plane
      Rhino.Display.RhinoView view = gp.View();
      if (view == null)
      {
        view = doc.Views.ActiveView;
        if (view == null)
          return Rhino.Commands.Result.Failure;
      }
      Rhino.Geometry.Plane plane = view.ActiveViewport.ConstructionPlane();
      plane.Origin = gp.Point();

      // Curve to orient on
      Rhino.Input.Custom.GetObject gc = new Rhino.Input.Custom.GetObject();
      gc.SetCommandPrompt("Curve to orient on");
      gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gc.EnablePreSelect(false, true);
      gc.DeselectAllBeforePostSelect = false;
      gc.Get();
      if (gc.CommandResult() != Rhino.Commands.Result.Success)
        return gc.CommandResult();

      Rhino.DocObjects.ObjRef objref = gc.Object(0);
      // get selected curve object
      Rhino.DocObjects.RhinoObject obj = objref.Object();
      if (obj == null)
        return Rhino.Commands.Result.Failure;
      // get selected curve
      Rhino.Geometry.Curve curve = objref.Curve();
      if (curve == null)
        return Rhino.Commands.Result.Failure;
      // Unselect curve
      obj.Select(false);

      // Point on surface to orient to
      GetOrientPerpendicularPoint gx = new GetOrientPerpendicularPoint(curve, plane, go.Object(0).Object());
      gx.SetCommandPrompt("New base point on curve");
      gx.Get();
      if (gx.CommandResult() != Rhino.Commands.Result.Success)
        return gx.CommandResult();

      Rhino.Geometry.Transform xform = new Rhino.Geometry.Transform(1);
      if (gx.CalculateTransform(gx.View().ActiveViewport, gx.Point(), ref xform))
      {
        doc.Objects.Transform(go.Object(0).Object(), xform, true);
        doc.Views.Redraw();
      }

      return Result.Success;
    }
  }

  /// <summary>
  /// GetOrientPerpendicularPoint class
  /// </summary>
  class GetOrientPerpendicularPoint : Rhino.Input.Custom.GetPoint
  {
    Rhino.Geometry.Curve _curve;
    Rhino.Geometry.Plane _plane;
    Rhino.DocObjects.RhinoObject _obj;
    Rhino.Geometry.Transform _xform;
    bool _draw;

    public GetOrientPerpendicularPoint(Rhino.Geometry.Curve curve, Rhino.Geometry.Plane plane, Rhino.DocObjects.RhinoObject obj)
    {
      _curve = curve;
      _plane = plane;
      _obj = obj;

      _xform = new Rhino.Geometry.Transform(1);
      _draw = false;

      this.Constrain(_curve, false);
      this.MouseMove += new EventHandler<Rhino.Input.Custom.GetPointMouseEventArgs>(RhOrientPerpPoint_MouseMove);
      this.DynamicDraw += new EventHandler<Rhino.Input.Custom.GetPointDrawEventArgs>(RhOrientPerpPoint_DynamicDraw);
    }

    public bool CalculateTransform(Rhino.Display.RhinoViewport vp, Rhino.Geometry.Point3d pt, ref Rhino.Geometry.Transform xform)
    {
      bool rc = false;
      if (null != _curve)
      {
        double t;
        if (_curve.ClosestPoint(pt, out t))
        {
          Rhino.Geometry.Plane frame;
          if (_curve.PerpendicularFrameAt(t, out frame))
          {
            xform = Rhino.Geometry.Transform.PlaneToPlane(_plane, frame);
            rc = xform.IsValid;
          }
        }
      }
      return rc;
    }

    void RhOrientPerpPoint_MouseMove(object sender, Rhino.Input.Custom.GetPointMouseEventArgs e)
    {
      _draw = CalculateTransform(e.Viewport, e.Point, ref _xform);
    }

    void RhOrientPerpPoint_DynamicDraw(object sender, Rhino.Input.Custom.GetPointDrawEventArgs e)
    {
      if (_draw)
      {
        if (null != _obj)
          e.Display.DrawObject(_obj, _xform);
      }
    }
  }
}
