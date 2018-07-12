using QlikView.Qvx.QvxLibrary;
using System;
using System.Windows.Interop;

namespace QlikAWSS3Connector
{
    internal class QlikAWSS3ConnectorServer : QvxServer
    {
        public override QvxConnection CreateConnection()
        {

            return new QlikAWSS3ConnectorConnection();
        }

        public override string CreateConnectionString()
        {
            var connectionConfig = CreateLoginWindowHelper();
            connectionConfig.ShowDialog();

            string connectionString = null;
            if (connectionConfig.DialogResult.Equals(true))
            {
                connectionString = String.Format("access-key={0};secret-key-encrypted={1};aws-region={2}", 
                    connectionConfig.GetAccessKey(), 
                    connectionConfig.GetSecretKey(),
                    connectionConfig.GetAWSRegion());
            }

            return connectionString;
        }

        private ConnectionConfig CreateLoginWindowHelper()
        {
            var config = new ConnectionConfig();
            var wih = new WindowInteropHelper(config);
            wih.Owner = MParentWindow;

            return config;
        }
    }
}

