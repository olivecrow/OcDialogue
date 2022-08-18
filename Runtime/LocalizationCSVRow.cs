using System;

namespace OcDialogue
{
    [Serializable]
    public struct LocalizationCSVRow
    {
        public string key;
        public string id;
        public string korean;

        public string additional1;
        public string additional2;
        public string additional3;
        public string additional4;
    }
}