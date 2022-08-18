using Sirenix.OdinInspector;

namespace OcDialogue
{
    public interface IDialogueActorDB
    {
        ValueDropdownList<OcData> GetNPCDropDown();
    }
}