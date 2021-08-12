using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(TextMeshProAlignment))]
public class TextMeshProAlignmentEditor : Editor
{
	bool working = false;
	Dictionary<string, TextMeshProAlignmentSettings> lookup;
	int total;
	int item;

	public override bool RequiresConstantRepaint()
	{
		return working;
	}

	public override void OnInspectorGUI()
	{
		TextMeshProAlignment alignment = (TextMeshProAlignment)target;

		if (GUILayout.Button("Force reserialise all assets"))
		{
			if (!working)
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(
					LoopGUIDs(
						alignment,
						AssetDatabase.FindAssets("t:prefab", null),
						(guid) => alignment.ReserialiseAsset(guid)));
			}
		}

		if (GUILayout.Button("Load Text Mesh Pro alignment"))
		{
			if (!working)
			{
				alignment.settings = new List<TextMeshProAlignmentSettings>();
				EditorCoroutineUtility.StartCoroutineOwnerless(
					LoopGUIDs(
						alignment,
						AssetDatabase.FindAssets("t:prefab", null),
						(guid) => alignment.LoadTextMeshProAlignment(guid)));
			}
		}

		if (GUILayout.Button("Check Text Mesh Pro alignment"))
		{
			if (!working)
			{
				lookup = alignment.GetSettingsLookup();
				HashSet<string> guids = alignment.GetTextMeshProGUIDs();
				guids.IntersectWith(alignment.GetGUIDs());
				EditorCoroutineUtility.StartCoroutineOwnerless(
					LoopGUIDs(
						alignment,
						new List<string>(guids).ToArray(),
						(guid) => alignment.ResetSettings(guid, lookup)));
			}
		}

		if (working)
		{
			Rect rect = GUILayoutUtility.GetRect(18.0f, 18.0f, "TextField");
			EditorGUI.ProgressBar(
				rect,
				(float)item / (float)total,
				$"{item} / {total}");
		}

		DrawDefaultInspector();
	}

	public delegate string ProcessGUID(string guid);

	IEnumerator LoopGUIDs(TextMeshProAlignment alignment, string[] guids, ProcessGUID processGUID)
	{
		working = true;
		total = guids.Length;
		item = 0;
		foreach (var guid in guids)
		{
			item++;
			string message = processGUID(guid);
			Debug.Log(message);
			yield return null;
		}
		EditorUtility.SetDirty(alignment);
		working = false;
	}
}
