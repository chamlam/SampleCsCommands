using System;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace SampleCsCommands
{
  #region GetReferencePoint class
  /// <summary>
  /// GetReferencePoint class
  /// </summary>
  internal class GetReferencePoint : GetPoint
  {
    private Point3d m_base_point;
    private Circle m_circle;

    public GetReferencePoint(Point3d basePoint)
    {
      m_base_point = basePoint;
      AcceptNumber(true, false);
      SetBasePoint(m_base_point, true);
      DrawLineFromPoint(m_base_point, true);
      ConstrainToConstructionPlane(true);
      MouseMove += new EventHandler<GetPointMouseEventArgs>(FirstRotationPoint_MouseMove);
      DynamicDraw += new EventHandler<GetPointDrawEventArgs>(FirstRotationPoint_DynamicDraw);
    }

    void FirstRotationPoint_DynamicDraw(object sender, GetPointDrawEventArgs e)
    {
      if (m_circle.IsValid)
      {
        System.Drawing.Color color = Rhino.ApplicationSettings.AppearanceSettings.TrackingColor;
        e.Display.DrawCircle(m_circle, color);
      }
    }

    private void FirstRotationPoint_MouseMove(object sender, GetPointMouseEventArgs e)
    {
      m_circle = new Circle(e.Viewport.ConstructionPlane(), m_base_point, e.Point.DistanceTo(m_base_point));
    }
  }
  #endregion


  #region GetRotationTransform class
  /// <summary>
  /// GetRotationTransform
  /// </summary>
  internal class GetRotationTransform : GetTransform
  {
    private Plane m_plane;
    private Point3d m_base_point;
    private Point3d m_ref_point;
    private double m_angle;
    private Arc m_arc;

    public GetRotationTransform(Plane plane, Point3d basePoint, Point3d refPoint)
    {
      m_angle = RhinoMath.UnsetValue;
      m_plane = plane;
      m_base_point = basePoint;
      m_ref_point = refPoint;
      AcceptNumber(true, false);
      SetBasePoint(m_base_point, true);
      Constrain(m_plane, false);
      ConstrainDistanceFromBasePoint(m_base_point.DistanceTo(m_ref_point));
      DynamicDraw += new EventHandler<GetPointDrawEventArgs>(GetSecondRotationPoint_DynamicDraw);
    }

    public double Angle
    {
      get
      {
        return m_angle;
      }
    }

    public override Transform CalculateTransform(RhinoViewport viewport, Point3d point)
    {
      if (CalculatePlaneAngle(point))
        return Transform.Rotation(m_angle, m_plane.Normal, m_base_point);
      return Transform.Identity;      
    }

    void GetSecondRotationPoint_DynamicDraw(object sender, GetPointDrawEventArgs e)
    {
      if (Transform.IsValid && m_arc.IsValid)
      {
        System.Drawing.Color color = Rhino.ApplicationSettings.AppearanceSettings.DefaultObjectColor;
        e.Display.DrawArc(m_arc, color);

        Vector3d v0 = m_arc.StartPoint - m_arc.Center;
        v0 *= 1.5;
        e.Display.DrawLine(m_arc.Center, m_arc.Center + v0, color);

        v0 = m_arc.EndPoint - m_arc.Center;
        v0 *= 1.5;
        Vector3d v1 = (e.CurrentPoint - m_arc.Center);
        if (v1.SquareLength > v0.SquareLength)
          v0 = v1;
        e.Display.DrawLine(m_arc.Center, m_arc.Center + v0, color);

        e.Display.DrawPoint(m_arc.StartPoint, color);
        e.Display.DrawPoint(m_arc.Center, color);
      }
    }

    private bool CalculatePlaneAngle(Point3d point)
    {
      if (System.Math.Abs(m_plane.ValueAt(point)) > 0.000001)
        return false;

      if (m_base_point.DistanceTo(point) <= RhinoMath.ZeroTolerance)
        return false;

      Vector3d v = point - m_base_point;
      v.Unitize();

      Vector3d zerov = m_ref_point - m_base_point;
      double r = zerov.Length;
      zerov.Unitize();
      
      double dot = zerov * v;
      if (!RhinoMath.IsValidDouble(dot)) 
        dot = 1.0;
      else if (dot > 1.0) 
        dot = 1.0;
      else if (dot < -1.0) 
        dot = -1.0;

      double test_angle = Math.Acos(dot);
      Vector3d zaxis = Vector3d.CrossProduct(m_plane.XAxis, m_plane.YAxis);
      zaxis.Unitize();

      v = Vector3d.CrossProduct(zaxis, zerov);
      v.Unitize();

      Plane yplane = new Plane(m_base_point, v);
      if (yplane.ValueAt(point) < 0.0)
        test_angle = 2.0 * Math.PI - test_angle;

      m_angle = test_angle;

      Plane arc_plane = new Plane(m_plane.Origin, zerov, v);
      m_arc = new Arc(arc_plane, r, m_angle);

      return (m_arc.IsValid);
    }
  }
  #endregion


  #region SampleCsRotate command
  /// <summary>
  /// SampleCsRotate command
  /// </summary>
  [System.Runtime.InteropServices.Guid("01d510e7-b682-4076-b2a7-48eb027096f7")]
  public class SampleCsRotate : TransformCommand 
  {
    public SampleCsRotate()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsRotate"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Locals
      Plane plane;
      Point3d base_point = Point3d.Unset;
      Point3d ref_point = Point3d.Unset;
      GetResult res = GetResult.Nothing;
      Result rc = Result.Nothing;


      // Select objects to rotate
      TransformObjectList list = new TransformObjectList();
      rc = SelectObjects("Select objects to rotate", list);
      if (rc != Result.Success)
        return rc;

      GetPoint gp = new GetPoint();
      gp.SetCommandPrompt("Center of rotation");
      gp.Get();
      if (gp.CommandResult() != Result.Success)
        return gp.CommandResult();

      RhinoView view = gp.View();
      if (null == view)
        return Result.Failure;

      base_point = gp.Point();
      plane = view.ActiveViewport.ConstructionPlane();
      plane.Origin = base_point;


      // Angle or first reference point
      GetReferencePoint gr = new GetReferencePoint(base_point);
      gr.SetCommandPrompt("Angle or first reference point");
      res = gr.Get();
      if (res == GetResult.Point)
      {
        view = gr.View();
        rc = (null != view) ? Result.Success : Result.Failure;
        if (rc == Result.Success)
        {
          plane = view.ActiveViewport.ConstructionPlane();
          plane.Origin = base_point;
          ref_point = plane.ClosestPoint(gr.Point());
          if (base_point.DistanceTo(ref_point) <= RhinoMath.ZeroTolerance)
            rc = Result.Nothing;
        }
        if (rc != Result.Success)
          return rc;
      }
      else if (res == GetResult.Number)
      {
        Transform xform = Transform.Rotation(Rhino.RhinoMath.ToRadians(gr.Number()), plane.Normal, base_point);
        rc = (xform.IsValid) ? Result.Success : Result.Failure;
        if (rc == Result.Success)
        {
          TransformObjects(list, xform, false, false);
          doc.Views.Redraw();
        }
        return rc;
      }
      else
      {
        return Result.Cancel;
      }


      // Second reference point
      GetRotationTransform gx = new GetRotationTransform(plane, base_point, ref_point);
      gx.SetCommandPrompt("Second reference point");
      gx.AddTransformObjects(list);
      res = gx.GetXform();
      if (res == GetResult.Point)
      {
        view = gx.View();
        rc = (null != view) ? Result.Success : Result.Failure;
        if (rc == Result.Success)
        {
          Transform xform = gx.CalculateTransform(view.ActiveViewport, gx.Point());
          rc = (xform.IsValid) ? Result.Success : Result.Failure;
          if (rc == Result.Success)
          {
            TransformObjects(list, xform, false, false);
            doc.Views.Redraw();
          }
        }
      }
      else if (res == GetResult.Number)
      {
        Transform xform = Transform.Rotation(Rhino.RhinoMath.ToRadians(gx.Number()), plane.Normal, base_point);
        rc = (xform.IsValid) ? Result.Success : Result.Failure;
        if (rc == Result.Success)
        {
          TransformObjects(list, xform, false, false);
          doc.Views.Redraw();
        }
      }
      else
      {
        rc = Result.Cancel;
      }

      return rc;
    }
  }
  #endregion
}
