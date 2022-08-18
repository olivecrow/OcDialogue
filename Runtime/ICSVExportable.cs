using System.Collections.Generic;

namespace OcDialogue
{
    public interface ICSVExportable
    {
        public string[] GetLocalizationCSVHeader();
        public IEnumerable<LocalizationCSVRow> GetLocalizationCSVLines();
    }
}