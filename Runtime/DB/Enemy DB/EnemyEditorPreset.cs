using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class EnemyEditorPreset : MonoBehaviour
    {
        public List<OverrideEnemy> OverrideEnemies;

        [Button]
        void ToDefault()
        {
            var list = new List<OverrideEnemy>();
            foreach (var enemy in EnemyDatabase.Instance.Enemies)
            {
                var oE = new OverrideEnemy(enemy);
                list.Add(oE);
            }

            OverrideEnemies = list;
        }

        [Serializable]
        public class OverrideEnemy
        {
            public Enemy Enemy;
            public int KillCount;

            public OverrideEnemy(Enemy enemy)
            {
                Enemy = enemy;
            }
        }
    }
}
