using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyDB
{
    [Serializable]
    public class ItemDropInfo
    {
        public enum ProbabilityMethod
        {
            Constant,
            MoreIsHard,
            MoreIsEasy
        }

        public ItemBase item;

        [VerticalGroup("Probability"), HideLabel]
        public ProbabilityMethod method;

        [VerticalGroup("Probability"), HideLabel] [Range(0f, 1f)]
        public float probability = 1;

        public Vector2Int minMaxCount = new Vector2Int(0, 1);
        public int CountResult { get; set; }

        public bool TryDrop(out int result)
        {
            CalcResult();
            result = CountResult;
            return CountResult > 0;
        }
        
        [VerticalGroup("B"), Button]
        public void CalcResult()
        {
            if (probability == 0)
            {
                CountResult = 0;
                return;
            }

            var fail = Random.value > probability;
            if (fail)
            {
                CountResult = 0;
                return;
            }
            
            switch (method)
            {
                case ProbabilityMethod.Constant:
                {
                    CountResult = Random.Range(minMaxCount.x, minMaxCount.y + 1);
                    break;
                }
                case ProbabilityMethod.MoreIsHard:
                {
                    var count = minMaxCount.y - minMaxCount.x + 1;
                    var index = -1;
                    var maxChance = -1f;
                    for (int i = minMaxCount.x; i <= minMaxCount.y; i++)
                    {
                        var chance = Random.Range(0, count + (count - i) * count * (1.02f - probability));
                        if (chance > maxChance)
                        {
                            maxChance = chance;
                            index = i;
                        }
                    }

                    CountResult = index;
                    break;
                }
                case ProbabilityMethod.MoreIsEasy:
                {
                    var count = minMaxCount.y - minMaxCount.x + 1;
                    var index = -1;
                    var maxChance = -1f;
                    for (int i = minMaxCount.x; i <= minMaxCount.y; i++)
                    {
                        var chance = Random.Range(0, count + (count - i) * count * (1.02f - probability));
                        if (chance > maxChance)
                        {
                            maxChance = chance;
                            index = i;
                        }
                    }

                    CountResult = index;
                    break;
                }
            }
        }
        
        
#if UNITY_EDITOR
        [VerticalGroup("B"), Button]
        public void Calc10000()
        {
            Dictionary<int, int> counter = new Dictionary<int, int>();
            for (int i = 0; i < 10000; i++)
            {
                CalcResult();
                if (!counter.ContainsKey(CountResult)) counter[CountResult] = 0;
                counter[CountResult]++;
            }

            foreach (var kv in counter.OrderBy(x => x.Key))
            {
                Debug.Log($"count : {kv.Key} | result : {(float)kv.Value / 10000}");
            }
        }
#endif
    }
}
