using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using QlikView.Qvx.QvxLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace QlikAWSS3Connector
{
    class StorageOperations
    {

        public static QvxDataTable ListBuckets(QvxTable bucketsTable, IDictionary<string, string> MParameters)
        {
            bucketsTable.GetRows = ListBucketsRows(bucketsTable, MParameters);

            return new QvxDataTable(bucketsTable);
        }

        public static QvxTable.GetRowsHandler ListBucketsRows(QvxTable table, IDictionary<string, string> MParameters)
        {
            RegionEndpoint region = null;
            AmazonS3Client s3Client = null;

            string GCPProjectId = "";

            List<QvxDataRow> rows = new List<QvxDataRow>();
            MParameters.TryGetValue("access-key", out string accessKey);
            MParameters.TryGetValue("secret-key-encrypted", out string secretKeyEncrypted);
            MParameters.TryGetValue("aws-region", out string awsRegion);

            string secretKey = EncryptDecrypt.DecryptString(secretKeyEncrypted);

            return () =>
            {             
                try
                {
                    region = RegionEndpoint.GetBySystemName("eu-west-2");
                    s3Client = new AmazonS3Client(accessKey, secretKey, region);
                }
                catch (Exception ex)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Error getting the data from Google Storage");
                }

                ListBucketsResponse response = s3Client.ListBuckets();

                int length = 0;
                foreach (S3Bucket bucket in response.Buckets)
                {
                    length++;
                }

                if (length == 0)
                {
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("No buckets found for project {0}", GCPProjectId));
                }
                else
                {
                    foreach (S3Bucket bucket in response.Buckets)
                    {
                        var row = new QvxDataRow();
                        row[table.Fields[0]] = bucket.BucketName.ToString();
                        row[table.Fields[1]] = bucket.CreationDate.ToString();
                        rows.Add(row);
                    }
                }

                return rows;
            };
        }

        
        public static QvxDataTable ListBucketObjects(QvxTable bucketObjectsTable, IDictionary<string, string> fields, IDictionary<string, string> MParameters)
        {
            bucketObjectsTable.GetRows = ListBucketObjectsRows(fields, bucketObjectsTable, MParameters);

            return new QvxDataTable(bucketObjectsTable);
        }

        public static QvxTable.GetRowsHandler ListBucketObjectsRows(IDictionary<string, string> fields, QvxTable table, IDictionary<string, string> MParameters)
        {
            return () =>
            {
                List<QvxDataRow> rows = new List<QvxDataRow>();
                RegionEndpoint region = null;
                AmazonS3Client s3Client = null;
                ListObjectsResponse response = null;

                MParameters.TryGetValue("access-key", out string accessKey);
                MParameters.TryGetValue("secret-key-encrypted", out string secretKeyEncrypted);
                MParameters.TryGetValue("aws-region", out string awsRegion);

                string secretKey = EncryptDecrypt.DecryptString(secretKeyEncrypted);

                fields.TryGetValue("bucketname", out string bucketName);

                try
                {
                    region = RegionEndpoint.GetBySystemName(awsRegion);
                    s3Client = new AmazonS3Client(accessKey, secretKey, region);
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = bucketName
                    };

                    response = s3Client.ListObjects(request);

                    foreach (S3Object bucketObject in response.S3Objects)
                    {
                        var row = new QvxDataRow();
                        row[table.Fields[0]] = bucketObject.BucketName?.ToString() ?? "";
                        row[table.Fields[1]] = bucketObject.ETag?.ToString() ?? "";
                        row[table.Fields[2]] = bucketObject.Key?.ToString() ?? "";                        
                        row[table.Fields[3]] = bucketObject.LastModified.ToString();
                        row[table.Fields[4]] = bucketObject.Owner.DisplayName?.ToString() ?? "";
                        row[table.Fields[5]] = bucketObject.Owner.Id?.ToString() ?? "";
                        row[table.Fields[6]] = bucketObject.Size;
                        row[table.Fields[7]] = bucketObject.StorageClass.Value.ToString() ?? "";
                        rows.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Error getting the data from AWS S3");
                }

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, rows.Count.ToString());
                return rows;
            };
        }


        public static QvxDataTable DownloadObject(QvxTable downloadObjectTable, IDictionary<string, string> fields, IDictionary<string, string> MParameters)
        {
            RegionEndpoint region = null;
            AmazonS3Client s3Client = null;

            fields.TryGetValue("bucketname", out string bucketName);
            fields.TryGetValue("objectname", out string objectName);
            fields.TryGetValue("localpath", out string localPath);
            
            MParameters.TryGetValue("access-key", out string accessKey);
            MParameters.TryGetValue("secret-key-encrypted", out string secretKeyEncrypted);
            MParameters.TryGetValue("aws-region", out string awsRegion);

            string secretKey = EncryptDecrypt.DecryptString(secretKeyEncrypted);
            
            if (String.IsNullOrEmpty(bucketName))
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("Missing required param: {0}", "BucketName"));
            }

            if (String.IsNullOrEmpty(objectName))
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("Missing required param: {0}", "ObjectName"));
            }

            if (String.IsNullOrEmpty(localPath))
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("Missing required param: {0}", "LocalPath"));
            }


            try
            {
                region = RegionEndpoint.GetBySystemName(awsRegion);
                s3Client = new AmazonS3Client(accessKey, secretKey, region);

                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = objectName
                };

                using (GetObjectResponse response = s3Client.GetObject(request))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    string dest = localPath;
                    response.WriteResponseStreamToFile(dest);
                }

                downloadObjectTable.GetRows = DownloadObjectRows(downloadObjectTable);

            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Error downloading object from AWS S3: " + ex.Message);
            }
            return new QvxDataTable(downloadObjectTable);

        }

        public static QvxTable.GetRowsHandler DownloadObjectRows(QvxTable table)
        {
            return () =>
            {
                List<QvxDataRow> rows = new List<QvxDataRow>();

                var row = new QvxDataRow();
                row[table.Fields[0]] = "Dummy Value. This table can be dropped from the data model.";
                rows.Add(row);

                return rows;
            };
        }

    }
}
