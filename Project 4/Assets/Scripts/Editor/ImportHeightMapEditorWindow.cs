using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImportHeightMapEditorWindow : EditorWindow {

	[MenuItem("Terrain/Import Height Map")]
	public static ImportHeightMapEditorWindow ShowWindow() {
		return GetWindow<ImportHeightMapEditorWindow>();
	}

	private Texture2D image;
	private string terrainName = "New Terrain";

	private int width = 100;
	private int length = 100;
	private int maxHeight = 50;


	private float defaultLabelWidth;
	private bool usableDimensions;
	private bool squareDimensions;

	private Vector2 scrollPosition;

	public ImportHeightMapEditorWindow() {
		defaultLabelWidth = EditorGUIUtility.labelWidth;
		titleContent = new GUIContent("Create Terrain");
	}

	public void OnGUI() {
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		EditorGUIUtility.labelWidth = 100;

		terrainName = EditorGUILayout.TextField("Terrain Name", terrainName);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Image");
		image = (Texture2D) EditorGUILayout.ObjectField(image, typeof(Texture2D), false);
		EditorGUILayout.EndHorizontal();

		if (image != null) {
			EditorGUILayout.LabelField("Height Map Dimensions: (" + image.width + " × " + image.height + ")");
			usableDimensions = (image.width - 1) % 2 == 0 && (image.height - 1) % 2 == 0;
			squareDimensions = image.width == image.height;
			if (!usableDimensions)
				EditorGUILayout.HelpBox("Height map dimensions should be 1 more than a power of 2. For example, (513 × 513) instead of (512 × 512)", MessageType.Error);
			else if (!squareDimensions)
				EditorGUILayout.HelpBox("The height map should be square (width should squal the height of the texture)", MessageType.Error);
		}

		GUILayout.Space(60);


		GUI.skin.label.fontStyle = FontStyle.Bold;
		GUILayout.Label(new GUIContent("Terrain Dimensions", "The dimensions of the terrain in world units in your scene."));
		GUI.skin.label.fontStyle = FontStyle.Normal;

		EditorGUIUtility.labelWidth = 50;
		EditorGUILayout.BeginHorizontal();
		width = EditorGUILayout.IntField("Width", width);
		length = EditorGUILayout.IntField("Length", length);
		EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 100;

		maxHeight = EditorGUILayout.IntField("Max Height", maxHeight);

		GUILayout.Space(20);

		if (GUILayout.Button("Create Terrain"))
			CreateTerrain();

		EditorGUIUtility.labelWidth = defaultLabelWidth;
		EditorGUILayout.EndScrollView();
	}

	private void CreateTerrain() {
		if (image == null) {
			EditorUtility.DisplayDialog("Missing Height Map", "Failed to create terrain: You must supply a height map image.", "OK");
			return;
		}
		else if (!usableDimensions || !squareDimensions) {
			EditorUtility.DisplayDialog("Unusable Terrain Texture Dimensions", "Unable to create terrain: check the dimensions of your height map image.\n\nThe width and height must be equal, and they must be 1 more than a power of two. (For example, (129 × 129), (257 × 257), etc.)", "OK");
			return;
		}

		float[,] heights = new float[0, 0];
		Color[] heightMapPixels = new Color[0];
		try {
			heights = new float[image.height, image.width];
			heightMapPixels = image.GetPixels();
		} catch {
			EditorUtility.DisplayDialog("Error", "Failed to retrieve texture data. Make sure Read/Write is enabled in the Advanced Import Settings of the image file.", "OK");
			return;
		}


		try {

			TerrainData td = new TerrainData();
			td.size = new Vector3(width, maxHeight, length);
			td.heightmapResolution = image.width - 1; //Power of 2
			td.baseMapResolution = 2 * (image.width - 1); //A larger power of 2
			td.SetDetailResolution(td.baseMapResolution, 16);


			for (int y = 0; y < image.height; y++)
				for (int x = 0; x < image.width; x++)
					heights[y, x] = heightMapPixels[x + y * image.width].grayscale;

			td.SetHeights(0, 0, heights); //We can use SetHeights (Without DelayLOD because this is a newly created terrain)

			if (System.IO.File.Exists("Assets/" + terrainName + ".asset"))
				if (!EditorUtility.DisplayDialog("File Already Exists", "The file at Assets/" + terrainName + ".asset already exists.\n\nWould you like to replace it?", "Yes, Overwrite", "No, Don't Replace"))
					return;


			AssetDatabase.CreateAsset(td, "Assets/" + terrainName + ".asset");
			AssetDatabase.SaveAssets();
			EditorUtility.DisplayDialog("Success!", "Successfully created new Terrain Data at \nAssets/" + terrainName + ".asset.\n\nYou may drag the Terrain Data in a scene to create the Terrain Game Object now!", "OK");
		} catch (System.Exception e) {
			EditorUtility.DisplayDialog("Error", "Unable to create terrain.\nError details:\n\n" + e.Message, "OK");
		}
	}
}
