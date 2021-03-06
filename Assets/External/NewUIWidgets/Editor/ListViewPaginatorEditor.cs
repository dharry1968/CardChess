﻿namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// ListViewPaginator editor.
	/// </summary>
	[CustomEditor(typeof(ListViewPaginator), true)]
	[CanEditMultipleObjects]
	public class ListViewPaginatorEditor : Editor
	{
		Dictionary<string, SerializedProperty> Properties = new Dictionary<string, SerializedProperty>();

		string[] properties = new string[]
		{
			"ListView",
			"perPage",
			"DefaultPage",
			"ActivePage",
			"PrevPage",
			"NextPage",
			"FastDragDistance",
			"FastDragTime",
			"currentPage",
			"ForcedPosition",
			"Animation",
			"OnPageSelect",
		};

		/// <summary>
		/// Init.
		/// </summary>
		protected virtual void OnEnable()
		{
			Properties.Clear();

			Array.ForEach(properties, x => Properties.Add(x, serializedObject.FindProperty(x)));
		}

		/// <summary>
		/// Draw inspector GUI.
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(Properties["ListView"], true);
			EditorGUILayout.PropertyField(Properties["perPage"], true);

			EditorGUILayout.PropertyField(Properties["DefaultPage"], true);
			EditorGUILayout.PropertyField(Properties["ActivePage"], true);
			EditorGUILayout.PropertyField(Properties["PrevPage"], true);
			EditorGUILayout.PropertyField(Properties["NextPage"], true);

			EditorGUILayout.PropertyField(Properties["FastDragDistance"], true);
			EditorGUILayout.PropertyField(Properties["FastDragTime"], true);

			EditorGUILayout.PropertyField(Properties["currentPage"], true);
			EditorGUILayout.PropertyField(Properties["ForcedPosition"], true);

			EditorGUILayout.PropertyField(Properties["Animation"], true);

			EditorGUILayout.PropertyField(Properties["OnPageSelect"], true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}