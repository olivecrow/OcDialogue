using System;
using System.Linq;
using UnityEditor.Search;

namespace OcDialogue
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DataSearchAttribute : Attribute
    {
        public DataSearchAttribute(){}
    }
}