using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
/*
namespace DensityField {
	public class DensityfieldEditor : EditorWindow
	{
	    private List<EditorNode> nodes;
	    private List<Connection> connections;

	    private ConnectionPoint selectedInPoint;
	    private ConnectionPoint selectedOutPoint;

	    private Vector2 offset;
	    private Vector2 drag;

		private static Texture2D background;
		private static Texture2D texture;


		float zoomScale = 1.0f;
		Vector2 vanishingPoint = new Vector2(0,21);


		[MenuItem("Window/Mjollnir/Densityfield Editor")]
	    private static void OpenWindow()
	    {
	        DensityfieldEditor window = GetWindow<DensityfieldEditor>();
	        window.titleContent = new GUIContent("Densityfield ");
	    }

	    private void OnGUI()
	    {

			Zoom();
			GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), Resources.Load<Texture2D>("Densityfield/background"), ScaleMode.StretchToFill);

			//DrawGrid(20, 0.2f, Color.gray);
			//DrawGrid(100, 0.4f, Color.gray);

	        DrawNodes();

	        ProcessEvents(Event.current);
			ProcessNodeEvents(Event.current);

	        if (GUI.changed) Repaint();
	    }


		private void Zoom(){
		    Matrix4x4 oldMatrix = GUI.matrix;

		    //Scale my gui matrix
		    Matrix4x4 Translation = Matrix4x4.TRS(vanishingPoint,Quaternion.identity,Vector3.one);
		    Matrix4x4 Scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
		    GUI.matrix = Translation*Scale*Translation.inverse;

		    GUI.matrix = oldMatrix;

		    vanishingPoint = EditorGUILayout.Vector2Field("vanishing point",vanishingPoint);
		    zoomScale = EditorGUILayout.Slider("zoom",zoomScale,1.0f/25.0f,2.0f);
		}

	    private void DrawNodes()
	    {
			if (nodes != null)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					nodes[i].Draw();
				}
			}
	    }

		private void ProcessContextMenu(Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add Operator"), false, () => OnClickAddNode(mousePosition, NodeType.Operator));
			genericMenu.AddItem(new GUIContent("Add Generator"), false, () => OnClickAddNode(mousePosition, NodeType.Generator));
			genericMenu.ShowAsContext();
		}

		private void OnClickAddNode(Vector2 _mousePosition, NodeType _type)
		{
			if (nodes == null)
			{
				nodes = new List<EditorNode>();
			}

			nodes.Add(new EditorNode(_mousePosition, _type));
		}

		private void ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 1)
					{
						ProcessContextMenu(e.mousePosition);
					}
					break;
			}
		}

		private void ProcessNodeEvents(Event e)
		{
			if (nodes != null)
			{
				for (int i = nodes.Count - 1; i >= 0; i--)
				{
					bool guiChanged = nodes[i].ProcessEvents(e);

					if (guiChanged)
					{
						GUI.changed = true;
					}
				}
			}
		}
	}
}
*/
