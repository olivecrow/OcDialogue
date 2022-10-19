using System;

namespace OcDialogue
{
    [Serializable]
    public struct LocalizationCSVRow
    {
        public string key;
        public string korean;
        
        public string additional1;
        public string additional2;
        public string additional3;
        public string additional4;

        public LocalizationCSVRow(string key, string korean, string additional1, string additional2, string additional3, string additional4)
        {
            this.key = key;
            this.korean = korean;
            this.additional1 = additional1;
            this.additional2 = additional2;
            this.additional3 = additional3;
            this.additional4 = additional4;
        }
    }
}