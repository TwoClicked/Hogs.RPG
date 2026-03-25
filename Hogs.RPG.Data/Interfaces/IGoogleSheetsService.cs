using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Data.Interfaces
{
    public interface IGoogleSheetsService
    {

        Task<IList<IList<object>>> ReadRangeAsync(string sheet, string range);
        Task AppendRowAsync(string sheet, IList<Object> row);
        Task UpdateRangeAsync(string sheet, string range, IList<IList<object>> values);


        Task ClearRangeAsync(string sheetName, string range);
        Task WriteRangeAsync(string sheetName, string startCell, IList<IList<object>> data);
    }
}
