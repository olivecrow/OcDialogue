using System.Collections.Generic;

namespace OcDialogue
{
    public interface ICSVExportable
    {
        /// <summary>
        /// 번역을 위한 CSV테이블의 헤더를 반환함. LocalizationCSVRow에 맞게 길이가 6이어야함.
        /// </summary>
        public string[] GetLocalizationCSVHeader();
        
        /// <summary>
        /// 번역을 위한 CSV테이블의 내용을 반환함. additional1...4, key, korean 순서로 출력됨.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LocalizationCSVRow> GetLocalizationCSVLines();
    }
}