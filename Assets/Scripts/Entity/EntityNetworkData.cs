using Fusion;
using UnityEngine;

namespace Entity
{
    public struct EntityNetworkData : INetworkStruct
    {
        public int Id;
        public bool IsBot;

        public float Hp;
        public float Stamina;
        
        public bool IsAlive;
        public bool IsExhausted;


        public EntityNetworkData(int id, bool isBot, 
            float hp, float stamina, 
            bool isAlive, bool isExhausted)
        {
            Id = id;
            IsBot = isBot;
            Hp = hp;
            Stamina = stamina;
            IsAlive = isAlive;
            IsExhausted = isExhausted;
        }
    }
}