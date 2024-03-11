using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class FirestoreManager
{
    FirebaseFirestore db;
    bool isInitialized = false;

    public FirestoreManager()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                return;
            }

            // Initialize Firestore
            db = FirebaseFirestore.DefaultInstance;
            isInitialized = true;
        });
        

    }

    private void AddDataToFirestore(string collectionName, Dictionary<string, object> data)
    {
        if(!isInitialized) return;
        
        
        // Add data to Firestore
        db.Collection(collectionName).AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to add data to Firestore: {task.Exception}");
                return;
            }

            Debug.Log("Data added successfully!");
        });
    }

    
    public IEnumerator RetrieveDataFromFirestore(string collectionName, Action<List<Dictionary<string, object>>> callback)
    {
        if (!isInitialized)
        {
            callback?.Invoke(null);
            yield break;
        }

        List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

        var task = db.Collection(collectionName).GetSnapshotAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError($"Failed to retrieve data from Firestore: {task.Exception}");
            callback?.Invoke(null);
            yield break;
        }

        foreach (var document in task.Result.Documents)
        {
            data.Add(document.ToDictionary());
        }

        callback?.Invoke(data);
    }



    public void AddUser(string name, int xp, int nightsSurvived)
    {
        var user = new Dictionary<string, object>
        {
            { "name", name },
            { "xp", xp },
            { "nightsSurvived", nightsSurvived }
        };

        AddDataToFirestore("users", user);
    }
    
    
    
    
   
}