using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class RuntimeEnemyData
    {
        public List<Enemy> Enemies;

        public RuntimeEnemyData(IEnumerable<Enemy> original)
        {
            var list = new List<Enemy>();
            foreach (var enemy in original)
            {
                var copy = enemy.GetCopy();
                list.Add(copy);
            }

            Enemies = list;
        }
    }
}
