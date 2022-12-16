using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;

using Starpelly;

namespace Bread2Unity
{
    public class Bread2Unity : EditorWindow
    {
        Object source;
        Object json;
        Object animation;
        public const string editorFolderName = "bread2unity";

        [MenuItem("Tools/bread2unity")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<Bread2Unity>("bread2unity");
        }

        public void OnGUI()
        {
            Texture logo = (Texture)AssetDatabase.LoadAssetAtPath($"Assets/Editor/{editorFolderName}/logo.png", typeof(Texture));
            GUILayout.Box(logo, new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(60) });
            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Source texture:", EditorStyles.boldLabel);
            source = EditorGUILayout.ObjectField(source, typeof(Texture2D), false, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("JSON made from BCCAD:", EditorStyles.boldLabel);
            json = EditorGUILayout.ObjectField(json, typeof(TextAsset), false, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Animator:", EditorStyles.boldLabel);
            animation = EditorGUILayout.ObjectField(animation, typeof(Animation), false, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUIStyle desc = EditorStyles.label;
            desc.wordWrap = true;
            desc.fontStyle = FontStyle.BoldAndItalic;

            GUILayout.Box("bread2unity is a tool built with the purpose of converting RH Megamix and Fever animations to unity. And to generally speed up development by a lot." +
                "\nCreated by Starpelly, edited by VirusGames.", desc);

            GUILayout.Space(120);

            if (GUILayout.Button("Create JSON from BCCAD"))
            {
                string path = EditorUtility.OpenFilePanel("Open BCCAD File", null, "bccad");
                if (path.Length != 0)
                {
                    var fileContent = File.ReadAllBytes(path);
                    BCCAD.JSONFromBCCAD(path);
                }
            }

            if (GUILayout.Button("Slice texture"))
            {
                if (json != null)
                {
                    BCCAD.SliceTexture((TextAsset)json, (Texture2D)source);
                }
            }

            if (GUILayout.Button("Compile Sprites"))
            {
                if (json != null)
                {
                    BCCAD.CompileSprites((TextAsset)json);
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bread Download", GUILayout.Height(40)))
            {
                Application.OpenURL("https://github.com/rhmodding/bread");
            }
            GUILayout.EndHorizontal();
        }
    }
}