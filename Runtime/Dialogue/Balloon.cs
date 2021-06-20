using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OcDialogue
{
    public class Balloon : ScriptableObject
    {
        public enum Type
        {
            Entry,
            Dialogue,
            Choice
        }

        [ReadOnly] public string GUID;
        [ReadOnly] public Type type;

        [ShowIf("type", Type.Dialogue)] [ValueDropdown("GetNPCList")]
        public NPC actor;

        [HideIf("type", Type.Entry), TextArea]
        public string text;

        [Sirenix.OdinInspector.ReadOnly] public Vector2 position;
        [HideIf("type", Type.Entry)] public bool useChecker;

        // [ShowIf("useChecker"), HideLabel, BoxGroup("Checker")]
        // public DataChecker checker;
        //
        // [HideIf("type", BalloonType.Entry)] public bool useSetter;
        //
        //
        // [ShowIf("useSetter"), HideLabel, BoxGroup("Setter")]
        // public DataSetter setter;
        
        /// <summary> actor필드에서 NPC이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<NPC> GetNPCList()
        {
            var list = new ValueDropdownList<NPC>();
            foreach (var npc in NPCDatabase.Instance.NPCs)
            {
                list.Add(npc.NPCName, npc);
            }
        
            return list;
        }
    }
}
