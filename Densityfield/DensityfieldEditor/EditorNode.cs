using System;
using UnityEditor;
using UnityEngine;

namespace DensityField{

	public enum NodeType{
		Generator,
		Operator,
		Output
	}

	public enum NodeGenerator{
		Billow,
		Checker,
		Const,
		Cylinder,
		Perlin,
		RidgedMultifractal,
		Spheres,
		Voronoi
	}

	public enum NodeOperators{
		Abs,
		Add,
		Blend,
		Cache,
		Clamp,
		Curve,
		Displace,
		Exponent,
		Invert,
		Max,
		Min,
		Multiply,
		Power,
		Rotate,
		Scale,
		ScaleBias,
		Select,
		Subtract,
		Terrace,
		Translate,
		Turbulence
	}


	public class EditorNode
	{

		public Rect rect;
	    public string title;


		public bool isDragged;
		public bool isSelected;


	    public NodeType type;

		public GUIStyle style;

		private GUIStyle styleOperator;
		private GUIStyle styleGenerator;

	    public EditorNode(Vector2 position, NodeType _type)
	    {
			title = "YOLO";
			if(_type == NodeType.Generator){
				rect = new Rect(position.x, position.y, 150, 250);
			}else if(_type == NodeType.Operator){
				rect = new Rect(position.x, position.y, 50, 100);
			}
			type = _type;

			styleOperator = new GUIStyle();
        	styleOperator.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        	styleOperator.border = new RectOffset(12, 12, 12, 12);

			styleGenerator = new GUIStyle();
        	styleGenerator.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        	styleGenerator.border = new RectOffset(12, 12, 12, 12);
	    }

	    public void Drag(Vector2 delta)
	    {
	        rect.position += delta;
	    }

	    public void Draw()
	    {
	        GUI.Box(rect, title, style);
	    }

		public bool ProcessEvents(Event e)
	    {
	        switch (e.type)
	        {
	            case EventType.MouseDown:
	                if (e.button == 0)
	                {
	                    if (rect.Contains(e.mousePosition))
	                    {
	                        isDragged = true;
	                        GUI.changed = true;
	                        isSelected = true;
	                      // style = selectedNodeStyle;
	                    }
	                    else
	                    {
	                        GUI.changed = true;
	                        isSelected = false;
	                        //style = defaultNodeStyle;
	                    }
	                }

	                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
	                {
	                    //ProcessContextMenu();
	                    e.Use();
	                }
	                break;

	            case EventType.MouseUp:
	                isDragged = false;
	                break;

	            case EventType.MouseDrag:
	                if (e.button == 0 && isDragged)
	                {
	                    Drag(e.delta);
	                    e.Use();
	                    return true;
	                }
	                break;
	        }

	        return false;
	    }
	}
}
