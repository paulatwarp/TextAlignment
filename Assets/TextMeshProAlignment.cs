using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

[System.Serializable]
public class TextMeshProAlignmentSettings
{
	public string guid;
	public string hierarchy;
	public TextAlignmentOptions alignment;
}

public class PreviewScene : System.IDisposable
{
	public string path;
	UnityEngine.SceneManagement.Scene preview;
	Transform root;
	GameObject gameObject;

	public PreviewScene(Canvas canvasPrefab)
	{
		preview = EditorSceneManager.NewPreviewScene();
		Debug.Assert(preview != null);
		Canvas canvas = PrefabUtility.InstantiatePrefab(canvasPrefab, preview) as Canvas;
		Debug.Assert(canvas != null);
		root = canvas.transform;
	}

	public GameObject LoadPrefab(string guid)
	{
		path = AssetDatabase.GUIDToAssetPath(guid);
		if (!path.StartsWith("Packages"))
		{
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			Debug.Assert(prefab != null);
			gameObject = PrefabUtility.InstantiatePrefab(prefab, root) as GameObject;
		}
		return gameObject;
	}

	public void Dispose()
	{
		EditorSceneManager.ClosePreviewScene(preview);
	}
}


[CreateAssetMenu()]
public class TextMeshProAlignment : ScriptableObject
{
	public Canvas canvas;
	public List<TextMeshProAlignmentSettings> settings;

	static string GetName(Transform transform)
	{
		string name = "";
		if (transform.parent != null)
		{
			name += $"{GetName(transform.parent)}/";
		}
		name += transform.name;
		return name;
	}

	public string LoadTextMeshProAlignment(string guid)
	{
		string result;
		using (var preview = new PreviewScene(canvas))
		{
			List<string> results = new List<string>();
			GameObject prefab = preview.LoadPrefab(guid);
			if (prefab != null)
			{
				foreach (var text in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
				{
					string hierarchy = GetName(text.transform);
					var entry = new TextMeshProAlignmentSettings();
					entry.guid = guid;
					entry.hierarchy = hierarchy;
					entry.alignment = text.alignment;
					PrefabUtility.RecordPrefabInstancePropertyModifications(text);
					PrefabUtility.ApplyObjectOverride(text, preview.path, InteractionMode.AutomatedAction);
					settings.Add(entry);
					results.Add($"TextMesh {preview.path} {hierarchy} alignment is {text.alignment}");
				}
				if (results.Count > 0)
				{
					result = string.Join("\n", results);
				}
				else
				{
					result = $"No TextMeshes on {preview.path}";
				}
			}
			else
			{
				result = $"{preview.path} skipped";
			}
		}
		return result;
	}

	public void GetPrefabsDepthFirst(TextMeshProUGUI text, HashSet<string> guids)
	{
		var path = AssetDatabase.GetAssetPath(text);
		Debug.Assert(path != null, $"Can't find asset path for {GetName(text.transform)}");
		var guid = AssetDatabase.AssetPathToGUID(path);
		Debug.Assert(guid != null, $"Can't find guid for path {path}");
		if (!guids.Contains(guid))
		{
			foreach (var child in text.GetComponentsInChildren<TextMeshProUGUI>(true))
			{
				if (child != text)
				{
					var source = PrefabUtility.GetCorrespondingObjectFromSource<TextMeshProUGUI>(child);
					if (source != null)
					{
						GetPrefabsDepthFirst(source, guids);
					}
				}
			}
			guids.Add(guid);
		}
	}

	public HashSet<string> GetTextMeshProGUIDs()
	{
		HashSet<string> guids = new HashSet<string>();
		foreach (var guid in AssetDatabase.FindAssets("t:prefab", null))
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			foreach (var text in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
			{
				var source = PrefabUtility.GetCorrespondingObjectFromSource<TextMeshProUGUI>(text);
				if (source != null)
				{
					GetPrefabsDepthFirst(source, guids);
				}
			}
			guids.Add(guid);
		}
		return guids;
	}

	public Dictionary<string, TextMeshProAlignmentSettings> GetSettingsLookup()
	{
		var lookup = new Dictionary<string, TextMeshProAlignmentSettings>();
		foreach (var entry in settings)
		{
			lookup.Add($"{entry.guid}:{entry.hierarchy}", entry);
		}
		return lookup;
	}

	public HashSet<string> GetGUIDs()
	{
		HashSet<string> guids = new HashSet<string>();
		foreach (var entry in settings)
		{
			guids.Add(entry.guid);
		}
		return guids;
	}

	public string ResetSettings(string guid, Dictionary<string, TextMeshProAlignmentSettings> lookup)
	{
		string result; 
		List<string> results = new List<string>();
		using (var preview = new PreviewScene(canvas))
		{
			GameObject prefab = preview.LoadPrefab(guid);
			if (prefab != null)
			{
				foreach (var text in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
				{
					string hierarchy = GetName(text.transform);
					string id = $"{guid}:{hierarchy}";
					if (lookup.ContainsKey(id))
					{
						TextMeshProAlignmentSettings original = lookup[id];
						if (text.alignment != original.alignment)
						{
							results.Add($"TextMesh {preview.path} {id} updating alignment from {text.alignment} to {original.alignment}");
							text.alignment = original.alignment;
						}
						else
						{
							results.Add($"TextMesh {preview.path} {id} already set");
						}
					}
					else
					{
						results.Add($"TextMesh {preview.path} {id} not found");
					}
				}
			}
			if (results.Count > 0)
			{
				result = string.Join("\n", results);
			}
			else
			{
				result = $"No TextMeshes on {preview.path}";
			}
		}
		return result;
	}

	public string ReserialiseAsset(string guid)
	{
		string result;
		using (var preview = new PreviewScene(canvas))
		{
			if (preview.LoadPrefab(guid) != null)
			{
				result = $"reserialised {preview.path}";
			}
			else
			{
				result = $"skipped {preview.path}";
			}
		}
		return result;
	}
}
