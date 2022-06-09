using System;
using UnityEditor;
using UnityEngine;

namespace DensityField{
	public enum ConnectionPointType { In, Out }

	public class ConnectionPoint
	{
	    public Rect rect;

	    public ConnectionPointType type;

	    public EditorNode node;

	    public GUIStyle style;

	    public Action<ConnectionPoint> OnClickConnectionPoint;

	    public ConnectionPoint(EditorNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
	    {
	        this.node = node;
	        this.type = type;
	        this.style = style;
	        this.OnClickConnectionPoint = OnClickConnectionPoint;
	        rect = new Rect(0, 0, 10f, 20f);
	    }

	    public void Draw()
	    {
	        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

	        switch (type)
	        {
	            case ConnectionPointType.In:
	            rect.x = node.rect.x - rect.width + 8f;
	            break;

	            case ConnectionPointType.Out:
	            rect.x = node.rect.x + node.rect.width - 8f;
	            break;
	        }

	        if (GUI.Button(rect, "", style))
	        {
	            if (OnClickConnectionPoint != null)
	            {
	                OnClickConnectionPoint(this);
	            }
	        }
	    }
	}
}
