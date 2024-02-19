using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/Enemy", order = 1)]
public class Enemy : Entity
{

    public enum EnemyType
    {
        Melee,
        Ranged,
        Boss
    }

    public EnemyType enemyType = EnemyType.Melee;
    
    [Header("Item Properties")]
    [Tooltip("The amount of damage this enemy does")]
    public float damage;
    [Tooltip("The amount of health this enemy has")]
    public float health;
    [Tooltip("The amount of speed this enemy has")]
    public float speed;
    [Tooltip("The amount of range this enemy has")]
    public float range;
    
    private GameObject player;
    
    public void InstantiateEnemy(Vector3 location)
    {
        // Create a new Enemy object with the desired properties
        GameObject enemy = Instantiate(prefab, location, Quaternion.identity);
        var behaviour = enemy.GetComponent<EnemyBehaviour>();
        var health = enemy.GetComponent<Health>();
        
        behaviour.damage = (int)damage;
        health.maxHealth = (int)this.health;
        behaviour.speed = speed;
        
        //If other instanatiation is needed, do it here
        
    }

    public override void CopyOf(Entity entity)
    {
        throw new System.NotImplementedException();
    }
}



