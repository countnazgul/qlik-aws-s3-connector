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

        public static QvxDataTable ListBuckets(QvxTable bucketsTable, IDictionary<string, string> fields)
        {
            bucketsTable.GetRows = ListBucketsRows(bucketsTable, fields);

            return new QvxDataTable(bucketsTable);
        }

        public static QvxTable.GetRowsHandler ListBucketsRows(QvxTable table, IDictionary<string, string> fields)
        {
            RegionEndpoint region = null;
            AmazonS3Client s3Client = null;

            string GCPProjectId = "";

            //try
            //{
            //    var JSONObj = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(jsonCredentials);
            //    GCPProjectId = JSONObj["project_id"];
            //}
            //catch (Exception ex)
            //{
            //    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
            //    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Error parsing the JSON credentials file.");
            //}
            List<QvxDataRow> rows = new List<QvxDataRow>();
            fields.TryGetValue("access-key", out string accessKey);
            fields.TryGetValue("secret-key-encrypted", out string secretKeyEncrypted);
            fields.TryGetValue("aws-region", out string awsRegion);

            string secretKey = EncryptDecrypt.DecryptString(secretKeyEncrypted);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, String.Format("access --> {0} secret --> {1} region --> {2}", accessKey, secretKey, awsRegion));

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

                //foreach (S3Bucket bucket in response.Buckets)
                //{
                //    Console.WriteLine("You own Bucket with name: {0}", bucket.BucketName);
                //}


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



        public static QvxDataTable ListBucketObjects(QvxTable bucketObjectsTable, IDictionary<string, string> fields, string jsonCredentials)
        {
            bucketObjectsTable.GetRows = ListBucketObjectsRows(fields, bucketObjectsTable, jsonCredentials);

            return new QvxDataTable(bucketObjectsTable);
        }

        public static QvxTable.GetRowsHandler ListBucketObjectsRows(IDictionary<string, string> fields, QvxTable table, string jsonCredentials)
        {
            return () =>
            {
                List<QvxDataRow> rows = new List<QvxDataRow>();
                RegionEndpoint region = null;
                AmazonS3Client s3Client = null;
                ListObjectsResponse response = null;

                //fields.TryGetValue("bucketname", out string bucketName);

                //if (String.IsNullOrEmpty(bucketName))
                //{
                //    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, String.Format("Missing required param: {0}", "BucketName"));
                //}

                try
                {
                    region = RegionEndpoint.GetBySystemName("eu-west-2");
                    s3Client = new AmazonS3Client("AKIAJJLWV4VLV6NVHAVQ", "+aFLxMPbjDmtul2qs1Uy5qtb9I66Q/bexaw+H8YW", region);
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = "qlik-snippets"
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
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Error getting the data from Google Storage");
                }

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, rows.Count.ToString());
                return rows;
            };
        }


        public static QvxDataTable DownloadObject(QvxTable downloadObjectTable, IDictionary<string, string> fields, string jsonCredentials)
        {
            RegionEndpoint region = null;
            AmazonS3Client s3Client = null;

            fields.TryGetValue("bucketname", out string bucketName);
            fields.TryGetValue("objectname", out string objectName);
            fields.TryGetValue("localpath", out string localPath);
            fields.TryGetValue("localpath", out string keyName);
          

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
                localPath = localPath ?? Path.GetFileName(objectName);

                region = RegionEndpoint.GetBySystemName("eu-west-2");
                s3Client = new AmazonS3Client("AKIAJJLWV4VLV6NVHAVQ", "+aFLxMPbjDmtul2qs1Uy5qtb9I66Q/bexaw+H8YW", region);

                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = s3Client.GetObject(request))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    string dest = Path.Combine(localPath);
                    if (!File.Exists(dest))
                    {
                        response.WriteResponseStreamToFile(dest);
                    }
                }

                downloadObjectTable.GetRows = DownloadObjectRows(downloadObjectTable);

            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, ex.Message);
                throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Error downloading object from Google Storage");
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
