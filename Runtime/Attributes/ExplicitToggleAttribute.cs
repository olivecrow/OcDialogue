using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OcDialogue
{
    public class ExplicitToggleAttribute : Attribute
    {
        public string trueLabel;
        public string falseLabel;
        public ExplicitToggleAttribute(string trueLabel = "true", string falseLabel = "false")
        {
            this.trueLabel = trueLabel;
            this.falseLabel = falseLabel;
        }
    }
}
