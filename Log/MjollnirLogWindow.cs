using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MjollnirLogWindow : EditorWindow
{

    private SerializedObject test;

    bool All            = false;
    bool All_pressed    = false;

    bool Chunks         = false;
    bool Gaps           = false;
    bool Intersections  = false;



    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    Texture tex;

    public int toolbarInt = 0;
    public string[] toolbarStrings = new string[] { "Toolbar1", "Toolbar2", "Toolbar3" };


    [MenuItem("Window/MjollnirLog")]
    public static void ShowWindow()
    {
        MjollnirLogWindow window = (MjollnirLogWindow)EditorWindow.GetWindow(typeof(MjollnirLogWindow));
        window.Show();
    }

    private void OnGUI()
    {

        toolbarInt = GUI.Toolbar(new Rect(25, 25, 250, 30), toolbarInt, toolbarStrings);

         EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        All             = GUILayout.Toggle(All,             "Show All",       "Button");
        AllLogic();

        Chunks          = GUILayout.Toggle(Chunks,          "Show Chunks",    "Button");
        Gaps            = GUILayout.Toggle(Gaps,            "Show Gaps",      "Button");
        Intersections   = GUILayout.Toggle(Intersections,   "Show Xcross",    "Button");




        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Text Field", myString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();

        if (!tex)
        {
            Debug.LogError("No texture found, please assign a texture on the inspector");
        }

        if (GUILayout.Button(tex))
        {
            Debug.Log("Clicked the image");
        }
        if (GUILayout.Button("I am a regular Automatic Layout Button"))
        {
            Debug.Log("Clicked Button");
        }

        myBool = GUILayout.Toggle(myBool, "Toggle me !", "Button");
    }

    private void AllLogic()
    {
        if (All != All_pressed)
        {
            Chunks = All;
            Gaps = All;
            Intersections = All;

            All_pressed = All;
        }

        if (Chunks == Gaps && Gaps == Intersections)
        {
            All = Chunks;
        }
    }

}
