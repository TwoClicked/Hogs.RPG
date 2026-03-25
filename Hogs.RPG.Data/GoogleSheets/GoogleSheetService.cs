using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Hogs.RPG.Data.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hogs.RPG.Data.GoogleSheets
{
    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;

        public GoogleSheetsService(string credentialsPath, string spreadsheetId)
        {
            GoogleCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential
                    .FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Hogs RPG Bot"
            });

            _spreadsheetId = spreadsheetId;
        }

        public async Task<IList<IList<object>>> ReadRangeAsync(string sheetName, string range)
        {
            var fullRange = $"{sheetName}!{range}";

            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, fullRange);
            var response = await request.ExecuteAsync();

            return response.Values ?? new List<IList<object>>();
        }

        public async Task AppendRowAsync(string sheetName, IList<object> row)
        {
            var range = $"{sheetName}!A1";

            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { row }
            };

            var appendRequest = _sheetsService.Spreadsheets.Values.Append(
                valueRange,
                _spreadsheetId,
                range
            );

            appendRequest.ValueInputOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

            await appendRequest.ExecuteAsync();
        }

        public async Task UpdateRangeAsync(string sheetName, string range, IList<IList<object>> values)
        {
            var fullRange = $"{sheetName}!{range}";

            var body = new ValueRange
            {
                Values = values
            };

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(
                body,
                _spreadsheetId,
                fullRange
            );

            updateRequest.ValueInputOption =
                SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            await updateRequest.ExecuteAsync();
        }

        public async Task WriteRangeAsync(string sheetName, string startCell, IList<IList<object>> data)
        {
            var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
            {
                Values = data
            };

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(
                valueRange,
                _spreadsheetId,
                $"{sheetName}!{startCell}"
            );

            updateRequest.ValueInputOption =
                Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            await updateRequest.ExecuteAsync();
        }

        public async Task ClearRangeAsync(string sheetName, string range)
        {
            var clearRequest = new Google.Apis.Sheets.v4.Data.ClearValuesRequest();

            await _sheetsService.Spreadsheets.Values.Clear(
                clearRequest,
                _spreadsheetId,
                $"{sheetName}!{range}"
            ).ExecuteAsync();
        }
    }
}