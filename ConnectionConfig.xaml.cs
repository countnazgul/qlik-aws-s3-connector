using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace QlikAWSS3Connector
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class ConnectionConfig : Window
    {
        public ConnectionConfig()
        {
            InitializeComponent();
        }

        public string GetAccessKey()
        {
            return accessKey.Text;
        }

        public string GetAWSRegion()
        {
            try
            {
                return awsRegion.SelectedValue.ToString();
            } catch(Exception e)
            {
                return System.String.Empty;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            accessKey.Focus();
        }

        public string GetSecretKey()
        {
            if (secretKey.Password.Length > 0)
            {
                return EncryptDecrypt.EncryptString(secretKey.Password);
            } else
            {
                return System.String.Empty;
            }
        }

        public string GetSecretKeyPlain()
        {
            if (secretKey.Password.Length > 0)
            {
                return secretKey.Password;
            }
            else
            {
                return System.String.Empty;
            }
        }

        public string GetPassword1()
        {
            string test = EncryptDecrypt.EncryptString(secretKey.Password);
            MessageBox.Show(test);
            string test1 = EncryptDecrypt.DecryptString(test);
            MessageBox.Show(test1);
            return EncryptDecrypt.EncryptString(secretKey.Password);
        }

        private void okBbutton_Click(object sender, RoutedEventArgs e)
        {
            if( GetAccessKey().Length == 0 )
            {
                MessageBox.Show("Access Key can't be empty");                
            }

            if (GetSecretKey().Length == 0)
            {
                MessageBox.Show("Secret Key can't be empty");
            }

            if (GetAWSRegion().Length == 0)
            {
                MessageBox.Show("AWS Region is not selected");
            }

            if (GetAccessKey().Length != 0 && GetSecretKey().Length != 0 && GetAWSRegion().Length != 0) {
                DialogResult = true;
                Close();
            }
        }

        private void testBbutton_Click(object sender, RoutedEventArgs e)
        {
            RegionEndpoint region = null;
            AmazonS3Client s3Client = null;
            string accessKey = null;
            string secretKey = null;
            string awsRegion = null;


            if (GetAccessKey().Length == 0)
            {
                MessageBox.Show("Access Key can't be empty");                
            } else
            {
                accessKey = GetAccessKey();
            }

            if (GetSecretKeyPlain().Length == 0)
            {
                MessageBox.Show("Secret Key can't be empty");
            } else
            {
                secretKey = GetSecretKeyPlain();
            }

            if (GetAWSRegion().Length == 0)
            {
                MessageBox.Show("AWS Region is not selected");
            }
            else
            {
                awsRegion = GetAWSRegion();
            }

            if (GetAccessKey().Length != 0 && GetSecretKey().Length != 0 && GetAWSRegion().Length != 0)
            {
                try
                {
                    region = RegionEndpoint.GetBySystemName(awsRegion);
                    s3Client = new AmazonS3Client(accessKey, secretKey, region);

                    ListBucketsResponse response = s3Client.ListBuckets();

                    int length = 0;
                    foreach (S3Bucket bucket in response.Buckets)
                    {
                        length++;
                    }

                    MessageBox.Show(String.Format("{0} bucket(s) available", length));
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

            private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(/*comboBox1.SelectedValue.ToString()*/);
        }
    }
}
