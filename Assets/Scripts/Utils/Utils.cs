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
        
    }
}