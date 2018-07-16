using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;   
using QlikView.Qvx.QvxLibrary;

//QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Init()");
//throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Please provide WHERE clause");

namespace QlikAWSS3Connector
{
    internal class QlikAWSS3ConnectorConnection : QvxConnection
    {
        public override void Init()
        {
            QvxLog.SetLogLevels(false, true);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Init()");
            
            var bucketsListFields = new QvxField[]
                {
                    new QvxField("BucketName", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("CreationDate", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII)
                };

            var objectFields = new QvxField[]
                {
                    new QvxField("BucketName", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("ETag", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("Key", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("LastModified", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.INTEGER),
                    new QvxField("OwnerName", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("OwnerId", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("Size", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.INTEGER),
                    new QvxField("StorageClass", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII)
                };

            var dummyFields = new QvxField[]
            {
                    new QvxField("DummyField", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII)
            };
            

            MTables = new List<QvxTable>
                {
                    new QvxTable
                        {
                            TableName = "ListBuckets",                           
                            Fields = bucketsListFields
                        },
                    new QvxTable
                        {
                            TableName = "BucketObjects",
                            Fields = objectFields
                        },
                    new QvxTable
                        {
                            TableName = "DownloadObject",
                            Fields = dummyFields
                        }
                    //new QvxTable
                    //    {
                    //        TableName = "UploadObject",
                    //        Fields = dummyFields
                    //    },
                    //new QvxTable
                    //    {
                    //        TableName = "DeleteLocalObject",
                    //        Fields = dummyFields
                    //    }
                };
        }

        public override QvxDataTable ExtractQuery(string line, List<QvxTable> qvxTables)
        {
            string tableName = Parsers.GetTableName(line, MTables);
            IDictionary<string, string> fields = null;

            if (tableName != "listbuckets") {
                fields = Parsers.GetWhereFields(line, tableName);
            }
            QvxDataTable returnTable = null;


            switch (tableName)
            {
                case "listbuckets":
                    QvxDataTable a = StorageOperations.ListBuckets(FindTable("ListBuckets", MTables), MParameters);                    
                    returnTable = a;
                    break;
                case "bucketobjects":
                    QvxDataTable a1 = StorageOperations.ListBucketObjects(FindTable("BucketObjects", MTables), fields, MParameters);                    
                    returnTable = a1;
                    break;
                case "downloadobject":
                    QvxDataTable downloadObj = StorageOperations.DownloadObject(FindTable("DownloadObject", MTables), fields, MParameters);
                    returnTable = downloadObj;
                    break;
                case "uploadobject":
                    // TBA
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Not implemented yet :(");
                    //break;
                case "deletelocalobject":
                    // TBA
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Not implemented yet :(");
                    //break;
                default:
                    throw new QvxPleaseSendReplyException(QvxResult.QVX_UNKNOWN_COMMAND, "Please provide WHERE clause");
                    
            }

            return returnTable;
        }
    }
}
