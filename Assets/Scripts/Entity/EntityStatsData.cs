using UnityEngine;

namespace Entity
{
    [CreateAssetMenu(menuName = "Entity/Stats")]
    public class EntityStatsData : ScriptableObject
    {
        public int id;
        public bool isBot;

        public float hp;
        public float stamina;
        
        public bool isAlive;
        public bool isExhausted;
        
        public Vector2 position;
        public Vector2 velocity;
        public float angle;
        public float collisionRadius;
    }
}