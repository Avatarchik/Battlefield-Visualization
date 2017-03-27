using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Treeview;
using UnityEngine;
using Assets.Treeview.Rendering.Gui;
using Assets.CustomTreeview;

public class Treeview_DataRecorder : ITreeviewSourceDecoder<Treeview_DataModel>
{

    private readonly IGuiLayout _guiLayout;
    private Texture2D toogleVisible;
    private Texture2D toogleNotVisible;

    public Treeview_DataRecorder(IGuiLayout guiLayout)
    {
        if (guiLayout == null)
        {
            throw new ArgumentNullException("guiLayout");
        }

        this._guiLayout = guiLayout;

        this.toogleVisible = (Texture2D)Resources.Load("Images/Treeview/agg-treeview-VISIBLE", typeof(Texture2D));
        this.toogleNotVisible = (Texture2D)Resources.Load("Images/Treeview/agg-NOT_VISIBLE", typeof(Texture2D));
    }

    public Func<Treeview_DataModel, IEnumerable> Children
    {
        get { return this.ChildrenFunction; }
    }

    public Action<Treeview_DataModel, RenderDisplayContext> RenderDisplay
    {
        get
        {
            return this.RenderDisplayFunction;
        }
    }

    private IEnumerable ChildrenFunction(Treeview_DataModel item)
    {
        return item.Children;
    }

    private void RenderDisplayFunction(Treeview_DataModel item, RenderDisplayContext context)
    {
        if (item == null)
        {
            return;
        }

        var text = item.Text;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleLeft;
        style.fixedHeight = 16f;
        style.normal.textColor = context.IsSelected ? ContentRenderingColours.SelectedText : ContentRenderingColours.NormalText;

        GUIStyle styleToggle = new GUIStyle(GUI.skin.toggle);
        style.fixedHeight = 16f;
        styleToggle.border.top = 0;
        styleToggle.border.bottom = 0;
        styleToggle.normal.textColor = context.IsSelected ? ContentRenderingColours.SelectedText : ContentRenderingColours.NormalText;
        style.alignment = TextAnchor.MiddleLeft;

        bool beforeChange = item.IsAggregated;

        this._guiLayout.BeginHorizontal();

        //if (item.IsUnit)
        //item.IsAggregated = GUILayout.Toggle(item.IsAggregated, "", styleToggle, new GUILayoutOption[] { GUILayout.MaxWidth(10.0f) });

        if (item.IsAggregated)
        {
            if (GUILayout.Button(this.toogleNotVisible, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
            {
                item.IsAggregated = false;
            }
        }
        else
        {
            if (GUILayout.Button(this.toogleVisible, GUIStyle.none, GUILayout.Width(15), GUILayout.Height(15)))
            {
                item.IsAggregated = true;
            }
        }

        this._guiLayout.Label(" " + text, style);
        this._guiLayout.EndHorizontal();

        // item toggle handling
        if (item.IsAggregated != beforeChange)
        {
            item.IsSelfAggregated = item.IsAggregated;

            if (item.IsUnit)
            {
                HideChildren_Rec(item, !item.IsAggregated);
            }
            else
            {
                HideMember(item, !item.IsAggregated);
            }

            if (!item.IsSelfAggregated)
            {
                UntoggleFathers_Rec(item);
            }
        }
    }

    private void HideChildren_Rec(Treeview_DataModel father, bool isVisible)
    {
        foreach (var child in father.Children)
        {
            if (child.IsSelfAggregated)
            {
                child.IsAggregated = true;
                continue;
            }
            else if (!child.IsSelfAggregated)
            {
                child.IsAggregated = !isVisible;
            }

            if (child.IsUnit)
            {
                HideChildren_Rec(child, isVisible);
            }
            else
            {
                HideMember(child, isVisible);
            }
        }
    }

    private void HideMember(Treeview_DataModel member, bool isVisible)
    {
        foreach (Transform grandchild in member.GameObject.transform)
            foreach (Transform grandgrandchild in grandchild.transform)
            {
                if (grandgrandchild.GetComponent<Renderer>().enabled != isVisible)
                    grandgrandchild.GetComponent<Renderer>().enabled = isVisible;
                else
                    break;
            }
    }

    private void UntoggleFathers_Rec(Treeview_DataModel son)
    {
        if (son.Father == null)
        {
            return;
        }   
        if (!son.Father.IsAggregated && son.Father.IsSelfAggregated)
            return;

        son.Father.IsAggregated = false;
        son.Father.IsSelfAggregated = false;
        UntoggleFathers_Rec(son.Father);
    }
}
