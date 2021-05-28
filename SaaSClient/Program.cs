using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SaaSClient
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        static async Task MainAsync(string[] args)
        {
            const string baseAddress = "https://saas.tachosys.com/api/";

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = new TimeSpan(0, 2, 0)
            };
            DateTime authenticationExpiry;

            // Get Authentication token
            const string tokenUri = "token";
            const string username = "james@prosysdev.com";
            const string password = "MrForgetful";

            HttpResponseMessage response = await client.PostAsJsonAsync(tokenUri, new LoginModel
            {
                Username = username,
                Password = password
            });
            if (response.IsSuccessStatusCode) {
                var token = await response.Content.ReadAsAsync<TokenModel>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                authenticationExpiry = DateTime.UtcNow.AddMinutes(token.ExpiresIn);
                Console.WriteLine("Authentication successful.");
            }
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Unable to authenticate with server."));

            // Upload file
            const string uploadUri = "file/upload";
            const string filename = @"C:\Users\Public\Documents\Tachosys\digiConnect\Archive\Drivers\TESTING EXTRA GENTWO 12345678910\C_20190710_1004_E_TESTING_DB18312147040300.ddd";
            //const string filename = @"C:\Users\Public\Documents\Tachosys\digiConnect\Archive\Vehicles\G2VDO\M_20190131_1452_G2VDO_GEN2VDO1234567890.ddd";

            var form = new MultipartFormDataContent();
            var content = new ByteArrayContent(File.ReadAllBytes(filename));
            form.Add(content, "File", Path.GetFileName(filename));

            response = await client.PostAsync(uploadUri, form);
            if (response.IsSuccessStatusCode)
                Console.WriteLine("File upload successful.");
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "File upload failed."));

            // File Parse result
            const string parseUri = "file/parse";
            response = await client.GetAsync(parseUri);
            if (response.IsSuccessStatusCode) {
                var parseResults = await response.Content.ReadAsAsync<FileParseResponse>();
                Console.WriteLine("Parse: {0}", JsonConvert.SerializeObject(parseResults, Formatting.Indented));
            }
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to parse uploaded file."));

            // Get Filename
            const string filenameUri = "file/filename";

            response = await client.PostAsJsonAsync(filenameUri, new FilenameRequest
            {
                DownloadDate = DateTime.UtcNow,
                NamingConvention = NamingConventions.StandardEuropean
            });
            if (response.IsSuccessStatusCode)
                Console.WriteLine("Filename: {0}", response.Content.ReadAsAsync<FilenameResponse>().Result.Filename);
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to get filename."));

            // Get Summary
            const string summaryUri = "file/summary";

            response = await client.GetAsync(summaryUri);
            if (response.IsSuccessStatusCode) {
                var summary = await response.Content.ReadAsAsync<FileSummaryResponse>();
                Console.WriteLine("Summary: {0}", JsonConvert.SerializeObject(summary, Formatting.Indented));
            }
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to get file summary."));

            // Get File section
            const string sectionUri = "file/section";

            response = await client.PostAsJsonAsync(sectionUri, new FileSectionRequest
            {
                FileID = "0520",
                RecordType = "",
                FileOccurrence = "0"
            });
            if (response.IsSuccessStatusCode) {
                object section = await response.Content.ReadAsAsync<object>();
                Console.WriteLine("File Section: {0}", JsonConvert.SerializeObject(section, Formatting.Indented));
            }
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to get file section."));

            // Showing upload of ActivityInfo, Places, and SpecificCondition - analysis can be run directly against previously uploaded DriverCard
            // These calls would be used when you have got your own ActivityInfo, Places, and SpecificCondition tables.
            // The example below gets these sections from the uploaded DriverCard.

            // Get ActivityInfo, PlacesRecords, and SpecificConditions from uploaded card file
            ActivityDailyRecord[] activityCollection;
            SpecificConditionRecord[] specificConditionRecords;
            PlacesRecord[] placesRecords;
            response = await client.PostAsJsonAsync(sectionUri, new FileSectionRequest { FileID = "0504", FileOccurrence = "0" });
            if (response.IsSuccessStatusCode)
                activityCollection = await response.Content.ReadAsAsync<ActivityDailyRecord[]>();
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to get file section."));
            response = await client.PostAsJsonAsync(sectionUri, new FileSectionRequest { FileID = "0506", FileOccurrence = "0" });
            if (response.IsSuccessStatusCode)
                placesRecords = await response.Content.ReadAsAsync<PlacesRecord[]>();
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to get file section."));
            response = await client.PostAsJsonAsync(sectionUri, new FileSectionRequest { FileID = "0522", FileOccurrence = "0" });
            if (response.IsSuccessStatusCode)
                specificConditionRecords = await response.Content.ReadAsAsync<SpecificConditionRecord[]>();
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Failed to get file section."));

            List<ActivityChangeInfo> activity = new List<ActivityChangeInfo>();
            foreach (ActivityDailyRecord record in activityCollection) {
                foreach (ActivityChangeInfo change in record.ActivityChangeInfos)
                    activity.Add(change);
            }

            // Upload ActivityInfo, PlacesRecords, and SpecificConditions
            const string activityInfoUri = "analyse/data/activityChangeInfo";
            const string placesRecordsUri = "analyse/data/placesRecords";
            const string specificConditionsUri = "analyse/data/specificConditionRecords";

            response = await client.PostAsJsonAsync(activityInfoUri, activity);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(string.Format("Error: {0:d} {1} {2}", response.StatusCode, response.ReasonPhrase, "Failed to upload activityChangeInfo."));
            response = await client.PostAsJsonAsync(placesRecordsUri, placesRecords);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(string.Format("Error: {0:d} {1} {2}", response.StatusCode, response.ReasonPhrase, "Failed to upload placesRecords."));
            response = await client.PostAsJsonAsync(specificConditionsUri, specificConditionRecords);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException(string.Format("Error: {0:d} {1} {2}", response.StatusCode, response.ReasonPhrase, "Failed to upload specificConditionRecords."));

            // Get Driver Card analysis
            const string analyseUri = "analyse";

            response = await client.PostAsJsonAsync(analyseUri, new AnalyseRequest
            {
                POAasBreak = false,
                MissingManualEntry = false,
                HomeNation = 0x15,
                Language = "en-GB",
                TimeZone = "GMT Standard Time"
            });
            if (response.IsSuccessStatusCode) {
                List<AnalysisItem> list = await response.Content.ReadAsAsync<List<AnalysisItem>>();
                foreach (AnalysisItem item in list) {
                    Console.WriteLine($"Type    : {item.Type}");
                    Console.WriteLine($"SubType : {item.SubType}");
                    Console.WriteLine($"Inf Code: {item.Infringement_Code}");
                    Console.WriteLine($"Inf Level: {item.Infringement_Level}");
                    Console.WriteLine($"DateTime: {item.DateTime}");
                    Console.WriteLine($"Message : {item.Message}");
                    Console.WriteLine();
                }
            }
            else
                throw new ApplicationException(string.Format("Error: {0:d} {1} - {2}", response.StatusCode, response.ReasonPhrase, "Unable to perform analysis."));

        }
    }
}
