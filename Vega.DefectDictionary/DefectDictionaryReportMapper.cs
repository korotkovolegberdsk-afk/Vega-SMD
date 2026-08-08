using Vega.DefectDictionary.Models;
using Vega.Report.Models;

namespace Vega.DefectDictionary;

public static class DefectDictionaryReportMapper
{
    public static DefectReportItem ToReportItem(DefectDefinition definition) => new()
    {
        Code = definition.Code, EnglishName = definition.EnglishName, RussianName = definition.RussianName,
        TypicalCause = definition.TypicalCause, TypicalSolution = definition.TypicalSolution
    };
}