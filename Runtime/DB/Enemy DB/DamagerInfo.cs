using System;

namespace OcDialogue.DB.Enemy_DB
{
    [Serializable]
    public class DamagerInfo
    {
        public DamagerTag tag;
        public BattleStat stat;
        public float weight;
    }
}