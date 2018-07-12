# Qlik (View and Sense) Google Cloud Storage Custom Connector

#### Description

Custom connector that allows interaction with `Google Cloud Storage` via Qlik Script

#### Instalation
------
* Download zip file from releases here (**TBA**)
* Copy the folder from the zip file to `C:\Program Files\Common Files\QlikTech\Custom Data`
(if the path do not exist - make sure to create it)
* Close QV (or QS Desktop) and open it again

#### Usage (QV)
------
* Open the `Script Editor`
* From the list with connectors (*image to be added*) pick `QlikGoogleCloudConnector.exe`
* Press `Connect` button
* Paste the path or browse to the credentials JSON file
* Press `Ok` 

#### Query Examples
------

**TBA**

#### Methods
------

##### ListBuckets
  * Description - list all available buckets for specific project
  * Required params - none. The `ProjectID` is read from the credentials JSON file

##### ListObjects
  * Description - list all available objects for a specific buckets name
  * Required params
    * *BucketName*

##### DownloadObject
  * Description - download file to local location
  * Required params
    * *BucketName* 
    * *ObjectName*
    * *LocalPath*

##### UploadObject (TBA)
  * Description - upload local file to bucket
  * Required params
    * *LocalPath*
    * *BucketName* 
    * *???ObjectName???*

##### DeleteLocalObject (TBA)
  * Description - delete local file
  * Required params
    * *LocalPath*