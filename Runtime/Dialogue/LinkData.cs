using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    [Serializable]
    public class LinkData
    {
        public string from;
        public string to;
        public LinkData(){}

        public LinkData(string from, string to)
        {
            this.@from = from;
            this.to = to;
        }
    }
}
