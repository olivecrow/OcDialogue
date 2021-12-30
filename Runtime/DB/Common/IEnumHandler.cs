using System;

namespace OcDialogue.DB
{
    public interface IEnumHandler
    {
        Type GetEnumType(string fieldName);
        string[] GetEnumNames(string fieldName);
    }
}