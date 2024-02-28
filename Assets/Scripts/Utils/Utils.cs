using System.Collections;
using UnityEngine;

namespace Utils
{
    public static class Utils
    {
        
        public static void DestroyWithChildren(GameObject obj)
        {
            foreach (Transform child in obj.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            GameObject.Destroy(obj);
        }
        
        
        public static IEnumerator PerformFunctionAfterDelay(float delay, System.Action function)
        {
            yield return new WaitForSeconds(delay);
            function();
        }
        
    }
}