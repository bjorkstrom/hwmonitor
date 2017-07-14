using Amazon;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using System.IO;
using System.Text;

public class KinesisLog
{
    /*
     * A delegate of this type sends record
     * to the Kinesis stream
     */
    public delegate void Send(Record record);

    AmazonKinesisClient KinesisClient;

    string StreamName;
    string PartitionKey;

    KinesisLog(string AWSAccessKeyId, string AWSSecretAccessKey, string AWSRegion, string Stream, string Partition)
    {
        KinesisClient = new AmazonKinesisClient(AWSAccessKeyId, AWSSecretAccessKey,
                                                RegionEndpoint.GetBySystemName(AWSRegion));
        StreamName = Stream;
        PartitionKey = Partition + "/hwmetrics";
    }

    public void LogRecord(Record record)
    {
        byte[] dataAsBytes = Encoding.UTF8.GetBytes(record.ToJson());

        using (MemoryStream memoryStream = new MemoryStream(dataAsBytes))
        {
            PutRecordRequest requestRecord = new PutRecordRequest();
            requestRecord.StreamName = StreamName;
            requestRecord.PartitionKey = PartitionKey;
            requestRecord.Data = memoryStream;
            PutRecordResponse responseRecord = KinesisClient.PutRecord(requestRecord);
        }
    }

    public static Send Create()
    {
        if (!CloudConfig.LogToKinesis)
        {
            /*
             * logging to Kinesis is disabled,
             * return a delegate that does nothing
             */
            return delegate(Record record) { /* NOP */ };
        }

        var Logger = new KinesisLog(CloudConfig.AWSAccessKeyID, CloudConfig.AWSSecretAccessKey, CloudConfig.AWSRegion,
                                    CloudConfig.KinesisLogStream, CloudConfig.Name);
        return Logger.LogRecord;
    }
}
