using Sirenix.OdinInspector;

namespace OcDialogue
{
    public interface IDialogueActorDB
    {
        ValueDropdownList<OcNPC> GetOdinDropDown();
    }
}