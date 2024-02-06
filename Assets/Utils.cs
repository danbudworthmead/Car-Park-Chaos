using System.Collections.Generic;
using _Game.Car_Park;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public static class Utils
{
    public static void Shuffle<T>(this IList<T> list, Random random = null)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random?.Next(n + 1) ?? UnityEngine.Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    [MenuItem("Utils/Duplicate space finder")]
    public static void FindDuplicateSpaces()
    {
        var spaces = GameObject.FindObjectsOfType<CarParkSpace>();
        var duplicates = new List<CarParkSpace>();
        for (var i = 0; i < spaces.Length; i++)
        {
            for (var j = i + 1; j < spaces.Length; j++)
            {
                // distance check
                if (Vector3.Distance(spaces[i].transform.position, spaces[j].transform.position) < 0.1f)
                {
                    duplicates.Add(spaces[i]);
                    duplicates.Add(spaces[j]);
                }
            }
        }
        
        // log the duplicates
        if (duplicates.Count > 0)
        {
            Debug.Log($"Found duplicate spaces: {duplicates.Count}");
            foreach (var duplicate in duplicates)
            {
                Debug.Log(duplicate.name);
            }
        }
        else
        {
            Debug.Log("No duplicate spaces found");
        }
    }
    
    [MenuItem("Utils/Name spaces")]
    public static void NameSpaces()
    {
        var spaces = GameObject.FindObjectsOfType<CarParkSpace>();
        for (var i = 0; i < spaces.Length; i++)
        {
            spaces[i].name = $"Space ({i})";
        }
    }
}