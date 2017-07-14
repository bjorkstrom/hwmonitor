using Newtonsoft.Json;
using System.IO;

// disable warning "Field `XXX' is never assigned to" for
// our JSON deserialization object fields
#pragma warning disable 0649
class Cloud /* cloud.json */
{
    public string apiHost;
    public string deviceName;
    public string password;
    public string awsAccessKeyID;
    public string awsSecretAccessKey;

    /*
     * optinal elemetns
     */

    /* allow to override protocol used to communicate with API host */
    public string protocol = "https";
    /* allows to override AWS region we are using */
    public string awsRegion = Amazon.RegionEndpoint.EUCentral1.SystemName;
    /* allows to disable sending logs to Kinesis stream */
    public bool logToKinesis = true;
    /* allows to override the the Kinesis stream name where the device log is sent */
    public string kinesisLogStream = "logs";
}

#pragma warning restore

///
/// Provides a simple API for accessing various 'factory' settings
/// for the device, such as device name and cloud access settings.
///
/// E.g. device settings that we are not stored in the cloud.
///
public class CloudConfig
{
    static internal Cloud _Cloud = null;
    static Cloud Cloud
    {
        get
        {
            if (_Cloud == null)
            {
                _Cloud = LoadCloudJson();
            }

            return _Cloud;
        }
    }

    /* make it possible for the test to override config file used */
    internal delegate string PersistentGetter();
    static internal PersistentGetter PersistentPath = _PersistentPath;
    static string _PersistentPath()
    {
        return @"C:\Users\brab\AppData\LocalLow\Brab\Hagring\cloud.json";
    }

    static Cloud LoadCloudJson()
    {
        using (var reader = new StreamReader(PersistentPath()))
        {
            return JsonConvert.DeserializeObject<Cloud>(reader.ReadToEnd());
        }
    }

    ///
    /// The public API.
    /// Each Cloud setting is accessable as a static property.
    ///
    /// For example, to access kinesis stream name use CloudConfig.KinesisLogStream
    ///
    public static string Name { get { return CloudConfig.Cloud.deviceName; } }
    public static string CloudProtocol { get { return CloudConfig.Cloud.protocol; } }
    public static string CloudHost { get { return CloudConfig.Cloud.apiHost; } }
    public static string CloudPassword { get { return CloudConfig.Cloud.password; } }
    public static string AWSAccessKeyID { get { return CloudConfig.Cloud.awsAccessKeyID; } }
    public static string AWSSecretAccessKey { get { return CloudConfig.Cloud.awsSecretAccessKey; } }
    public static string AWSRegion { get { return CloudConfig.Cloud.awsRegion; } }
    public static bool LogToKinesis { get { return CloudConfig.Cloud.logToKinesis; } }
    public static string KinesisLogStream { get { return CloudConfig.Cloud.kinesisLogStream; } }
}

