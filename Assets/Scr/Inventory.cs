using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Inventory 
{
    public List<Item> items;
}

[System.Serializable]
public class Item
{

}

[System.Serializable]
public class Weapon : Item
{

}

[System.Serializable]
public class Clothing : Item
{

}

[System.Serializable]
public class Consumables : Item
{

}