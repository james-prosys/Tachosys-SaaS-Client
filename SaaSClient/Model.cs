using System;
using System.Collections;

namespace SaaSClient
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenModel
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class FileParseResponse
    {
        public bool IsFileParseSuccessful { get; set; }
        public bool IsVerifySignaturesSuccessful { get; set; }
        public string FileParseException { get; set; }
        public VerifySignatureResult VerifySignatureResult { get; set; }
    }

    public class VerifySignatureResult
    {
        public string Result { get; set; }
        public string Exception { get; set; }
        public SectionResult[] SectionResults { get; set; }
    }

    public class SectionResult
    {
        public string fileID { get; set; }
        public int fileOccurrence { get; set; }
        public bool success { get; set; }
    }

    public class FilenameRequest
    {
        public DateTime DownloadDate { get; set; }

        public int NamingConvention { get; set; }

    }

    public class FilenameResponse
    {
        public string Filename { get; set; }
    }

    public class NamingConventions
    {
        public const int StandardEuropean = 0;
        public const int France = 1;
        public const int Spain = 2;
    }

    public enum Types
    {
        Unknown = 0,
        DriverCard = 1,
        WorkshopCard = 2,
        VehicleUnit = 3
    }

    public class FileSummaryResponse
    {
        public Types Types { get; set; }
        public int Generation { get; set; }
        public DriverCardSummary DriverCardSummary { get; set; }
        public VehicleUnitSummary VehicleUnitSummary { get; set; }
        public WorkshopCardSummary WorkshopCardSummary { get; set; }
    }

    public class FileSectionRequest
    {
        public string FileID { get; set; }
        public string RecordType { get; set; }
        public string FileOccurrence { get; set; }
    }

    public class AnalyseRequest
    {
        public bool POAasBreak { get; set; }
        public bool MissingManualEntry { get; set; }
        public byte HomeNation { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }
    }

    public class AnalysisItem
    {
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Infringement_Code { get; set; }
        public string Infringement_Level { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public object[] Parameters { get; set; }
    }

    public class DriverCardSummary
    {
        public string CardHolderFirstNames { get; set; }
        public string CardHolderSurname { get; set; }
        public DateF CardHolderBirthDate { get; set; }
        public TimeReal CardValidityBegin { get; set; }
        public TimeReal CardExpiryDate { get; set; }
        public string CardIssuingMemberStateAlpha { get; set; }
        public CardNumber CardNumber { get; set; }
        public string DrivingLicenceNumber { get; set; }
        public ActivitySummary ActivitySummary { get; set; }
    }

    public class VehicleUnitSummary
    {
        public string VehicleIdentificationNumber { get; set; }
        public string VehicleRegistrationNationAlpha { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public ActivitySummary ActivitySummary { get; set; }
        public VehicleUnitIdenficationSummary VehicleUnitIdenficationSummary { get; set; }
    }

    public class WorkshopCardSummary
    {
        public string CardHolderFirstNames { get; set; }
        public string CardHolderSurname { get; set; }
        public TimeReal CardValidityBegin { get; set; }
        public TimeReal CardExpiryDate { get; set; }
        public string CardIssuingMemberStateAlpha { get; set; }
        public CardNumber CardNumber { get; set; }
        public string WorkshopName { get; set; }
        public string WorkshopAddress { get; set; }
    }

    public class ActivitySummary
    {
        public TimeReal StartDate { get; set; }
        public TimeReal EndDate { get; set; }
    }

    public class VehicleUnitIdenficationSummary
    {
        public string vuManufacturerName { get; set; }
        public string vuPartNumber { get; set; }
        public string vuSoftwareVersion { get; set; }
    }

    public class ActivityDailyRecord
    {
        public int activityPreviousRecordLength { get; set; }
        public int activityRecordLength { get; set; }
        public TimeReal activityRecordDate { get; set; }
        public string activityDailyPresenceCounter { get; set; }
        public int activityDayDistance { get; set; }

        public ActivityChangeInfo[] ActivityChangeInfos { get; set; }
    }

    public class ActivityChangeInfo
    {
        public byte slot { get; set; }
        public byte drivingStatus { get; set; }
        public byte cardStatus { get; set; }
        public byte activity { get; set; }
        public TimeReal time { get; set; }
    }

    public struct PlacesRecord
    {

        public TimeReal entryTime;
        public byte entryTypeDailyWorkPeriod;
        public byte dailyWorkPeriodCountry;
        public byte dailyWorkPeriodRegion;
        public uint vehicleOdometerValue;

    }

    public struct SpecificConditionRecord
    {
        public TimeReal entryTime;
        public byte specificConditionType;
    }

    public struct DateF
    {

        public int Year;
        public int Month;
        public int Day;

        public override string ToString()
        {
            return string.Format("{0:0000}-{1:00}-{2:00}", Year, Month, Day);
        }
    }

    public struct TimeReal
    {
        public UInt32 Ticks;

        public static implicit operator DateTime(TimeReal t)
        {
            UInt32 seconds = t.Ticks;
            UInt32 minutes = seconds / 60;
            UInt32 remainder = seconds % 60;
            DateTime ret = new DateTime(1970, 1, 1);
            ret = ret.AddMinutes(minutes);
            ret = ret.AddSeconds(remainder);
            return ret;
        }

        public DateTime ToDateTime()
        {
            return Convert.ToDateTime(this);
        }

        public override string ToString()
        {
            if (Ticks == 0 | Ticks == UInt32.MaxValue) {
                return "";
            }
            else {
                return this.ToDateTime().ToString();
            }
        }
        public string ToString(string format)
        {
            if (Ticks == 0 | Ticks == UInt32.MaxValue) {
                return "";
            }
            else {
                return this.ToDateTime().ToString(format);
            }
        }
    }

    public struct CardNumber
    {

        public string driverIdentification;
        public string ownerIdentification;
        public string cardConsecutiveIndex;
        public string cardReplacementIndex;
        public string cardRenewalIndex;

    }
}


