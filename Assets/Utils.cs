using System;
using System.Collections.Generic;

public static class Utils
{
    public static void Shuffle<T>(this IList<T> list, Random random = null)  
    {  
        var n = list.Count;  
        while (n > 1) {  
            n--;
            var k = random?.Next(n + 1) ?? UnityEngine.Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }      
}