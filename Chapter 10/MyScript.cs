using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScript : MonoBehaviour
{
	class Item
	{
		public static int NumberOfInstances = 0;
		
		public string name = "Unnamed Item";
		public int worth = 1;
		public bool canBeSold = true;

		public Item(string name, int worth, bool canBeSold)
		{
			NumberOfInstances += 1;
			
			this.name = name;
			this.worth = worth;
			this.canBeSold = canBeSold;
		}

		public void LogInfo()
		{
			if (canBeSold)
				Debug.Log("This " + name + " can be sold for " + worth + " golden coins!");
			else
				Debug.Log("This " + name + " cannot be sold.");
		}

		public static void LogInstanceCount()
		{
			Debug.Log("Number of Item instances is: " + NumberOfInstances);
		}

	}
	
	// Start is called before the first frame update
	void Start()
	{
		Item.LogInstanceCount();

		Item item = new Item("Goblin Tooth",4,true);

		Item.LogInstanceCount();
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}