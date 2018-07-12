using System;

namespace QlikAWSS3Connector
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new QlikAWSS3ConnectorServer().Run(args[0], args[1]);
            }

            //new QvEventLogServer().CreateConnection();
        }
    }
}
