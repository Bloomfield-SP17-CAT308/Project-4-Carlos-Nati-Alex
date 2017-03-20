using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

public class StandardItems {
	[XmlArray("Items")]
	[XmlArrayItem("Item", typeof(Item))]
	public List<Item> items;

	private static StandardItems instance;
	public static List<Item> Items {
		get { return instance.items; }
	}

	public static void LoadStandardItems() {
		string xmlText = File.ReadAllText("Assets/Standard Items.xml");
		int index = xmlText.IndexOf('<');
		if (index > 0)
			xmlText = xmlText.Substring(index);

		XmlSerializer serializer = new XmlSerializer(typeof(StandardItems), new XmlRootAttribute("StandardItems"));
		StringReader reader = new StringReader(xmlText);
		instance = (serializer.Deserialize(reader) as StandardItems);
		reader.Close();

		/*Debug.Log("There are " + Items.Count + " items in the Item Database! (Standard Items.xml)");
		for (int i = 0; i < Items.Count; i++)
			Debug.Log(Items[i].ToString());*/
	}
}