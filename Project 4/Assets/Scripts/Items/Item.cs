using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

public class Item {
	protected int itemId;
	protected string name;
	protected int stackLimit;

	/// <summary>An array of 2 Lists. Each list contains integers.
	/// The first list, craftingRecipe[0], will contain the itemIds of the items needed in the recipe.
	/// The second list, craftingRecipe[1], will contain the quantity of each item needed in the recipe.</summary>
	protected List<int>[] craftingRecipe;

	protected Sprite itemSprite;
	protected string spriteFilePath;

	protected GameObject itemPrefab;
	protected string prefabFilePath;

	[XmlAttribute("ItemId")]
	public int ItemId {
		get { return itemId; }
		set { itemId = value; }
	}

	[XmlAttribute("Name")]
	public string Name {
		get { return name; }
		set { name = value; }
	}

	[XmlElement("StackLimit")]
	public int StackLimit {
		get { return stackLimit; }
		set { stackLimit = value; }
	}

	[XmlElement("CraftingRecipe")]
	public string XmlCraftingRecipe {
		get {
			string result = "";
			if (craftingRecipe[0].Count != craftingRecipe[1].Count)
				throw new Exception("The number of items in the Crafting Recipe for Item " + itemId + " do not match up.");

			for (int i = 0; i < craftingRecipe[0].Count; i++)
				result += "{Item " + craftingRecipe[0][i] + ", " + craftingRecipe[1][i] + "}, ";

			return result.Substring(result.Length - 2);
		}
		set {
			List<string> pairs = new List<string>();
			int i = 0;
			int start = -1, end = -1;
			while (i < value.Length) {
				if (value[i] == '{')
					start = i;
				else if (value[i] == '}')
					end = i;

				if (start != -1 && end != -1) {
					pairs.Add(value.Substring(start + 1, end - (start + 1)));
					start = end = -1;
				}
				i++;
			}

			craftingRecipe = new List<int>[2];
			craftingRecipe[0] = new List<int>();
			craftingRecipe[1] = new List<int>();

			string[] splitPair;
			for (int j = 0; j < pairs.Count; j++) {
				splitPair = pairs[j].Split(',');
				craftingRecipe[0].Add(int.Parse(splitPair[0].Substring(4)));
				craftingRecipe[1].Add(int.Parse(splitPair[1]));
			}
		}
	}

	public bool Craftable {
		get { return (craftingRecipe != null); }
	}

	public Sprite Sprite {
		get { return itemSprite; }
	}

	[XmlElement("ItemSprite")]
	public string SpriteFilePath {
		get { return spriteFilePath; }
		set {
			spriteFilePath = value;
			itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFilePath);
		}
	}

	public GameObject Prefab {
		get { return itemPrefab; }
	}

	[XmlElement("ItemPrefab")]
	public string PrefabFilePath {
		get { return prefabFilePath; }
		set {
			prefabFilePath = value;
			itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFilePath);
		}
	}

	public override string ToString() {
		string result = GetType().Name + ": {";
		result += "\nId: " + itemId;
		result += "\nName: " + name;
		result += "\nStack Limit: " + stackLimit;
		result += "\nSprite: " + spriteFilePath;
		result += "\nPrefab: " + prefabFilePath;
		result += "\nCraftable: " + Craftable;
		return result + "\n}";
	}
}