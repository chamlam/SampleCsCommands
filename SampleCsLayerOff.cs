﻿using System.Windows.Forms;
using Rhino;
using Rhino.Commands;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("ee0d1c9c-7139-47b8-9deb-4e8e196c5ec8")]
  public class SampleCsLayerOff : Command
  {
    public override string EnglishName
    {
      get { return "SampleCsLayerOff"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var layer_index = doc.Layers.CurrentLayerIndex;
      var current_state = false;
      var res = Rhino.UI.Dialogs.ShowSelectLayerDialog(ref layer_index, "Select Layer", false, false, ref current_state);
      if (res != DialogResult.OK)
        return Result.Cancel;

      if (layer_index < 0 || layer_index >= doc.Layers.Count)
        return Result.Failure;

      if (layer_index == doc.Layers.CurrentLayerIndex)
        return Result.Nothing; // Cannot hide the current layer

      var layer = doc.Layers[layer_index];
      layer.IsVisible = false;
      layer.SetPersistentVisibility(false);
      layer.CommitChanges();

      doc.Views.Redraw();

      return Result.Success;
    }
  }
}
