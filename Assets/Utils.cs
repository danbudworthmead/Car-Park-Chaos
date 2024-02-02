using System.Collections.Generic;

public static class Utils
{
    public static void Shuffle<T>(this IList<T> list)  
    {  
        var n = list.Count;  
        while (n > 1) {  
            n--;  
            var k = UnityEngine.Random.Range(0, n+1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }      
}