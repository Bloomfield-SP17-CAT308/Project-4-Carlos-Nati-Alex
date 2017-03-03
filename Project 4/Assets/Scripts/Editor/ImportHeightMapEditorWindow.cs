using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImportHeightMapEditorWindow : EditorWindow {

	[MenuItem("Project 4/Import Terrain Height Map")]
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
		titleContent = new GUIContent("Import Height Map");
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
				EditorGUILayout.HelpBox("Height map dimensions should be one more than a power of 2. For example, (513 × 513), NOT (512 × 512)", MessageType.Error);
			else if (!squareDimensions)
				EditorGUILayout.HelpBox("The height map should be square (width should squal the height of the texture)", MessageType.Error);
		}

		GUILayout.Space(60);

		EditorGUIUtility.labelWidth = 50;
		EditorGUILayout.BeginHorizontal();
		width = EditorGUILayout.IntField("Width", width);
		length = EditorGUILayout.IntField("Length", length);
		EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 100;


		maxHeight = EditorGUILayout.IntField("Max Height", maxHeight);

		if (GUILayout.Button("Create Terrain"))
			CreateTerrain();

		EditorGUIUtility.labelWidth = defaultLabelWidth;
		EditorGUILayout.EndScrollView();
	}

	private void CreateTerrain() {
		if (!usableDimensions || !squareDimensions)
			ShowNotification(new GUIContent("Unable to create terrain: check the dimensions of your height map!"));
		TerrainData td = new TerrainData();
		td.size = new Vector3(width, maxHeight, length);
		td.heightmapResolution = image.width - 1; //Power of 2
		td.baseMapResolution = 2 * (image.width - 1); //A larger power of 2
		td.SetDetailResolution(td.baseMapResolution, 16);

		float[,] heights = new float[image.height, image.width];
		Color[] heightMapPixels = image.GetPixels();

		for (int y = 0; y < image.height; y++)
			for (int x = 0; x < image.width; x++)
				heights[y, x] = heightMapPixels[x + y * image.width].grayscale;

		td.SetHeights(0, 0, heights); //We can use SetHeights (Without DelayLOD because this is a newly created terrain)

		AssetDatabase.CreateAsset(td, "Assets/" + terrainName + ".asset");
		AssetDatabase.SaveAssets();
	}
}
