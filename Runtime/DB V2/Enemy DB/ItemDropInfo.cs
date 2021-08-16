using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OcDialogue
{
    [Serializable]
    public struct ItemDropInfo
    {
        [SerializeField] ItemBase item;
        [Range(0f, 1f)]public float chance;

        /// <summary> 주사위를 굴려서 드랍 가능한 상태인지 확인하고, 드랍이 가능하면 out으로 아이템의 카피를 반환함. </summary>
        public bool TryDrop(out ItemBase dropItem)
        {
            dropItem = null;
            if (Random.value > chance) return false;
            
            dropItem = item.GetCopy();
            return true;
        }
    }
}
