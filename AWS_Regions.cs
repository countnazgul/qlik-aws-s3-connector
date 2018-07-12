using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QlikAWSS3Connector
{
    public static class CollectionData
    {
        public static Dictionary<string, string> GetChoices()
        {
            Dictionary<string, string> awsRegions = new Dictionary<string, string>();

            awsRegions.Add("ap-northeast-1", "Asia Pacific (Tokyo)");
            awsRegions.Add("ap-northeast-2", "Asia Pacific (Seoul)");
            awsRegions.Add("ap-south-1", "Asia Pacific (Mumbai)");
            awsRegions.Add("ap-southeast-1", "Asia Pacific (Singapore)");
            awsRegions.Add("ap-southeast-2", "Asia Pacific (Sydney)");
            awsRegions.Add("ca-central-1", "Canada (Central)");
            awsRegions.Add("eu-central-1", "EU Central (Frankfurt)");
            awsRegions.Add("eu-west-1", "EU West (Ireland)");
            awsRegions.Add("eu-west-2", "EU West (London)");
            awsRegions.Add("eu-west-3", "EU West (Paris)");
            awsRegions.Add("sa-east-1", "South America (Sao Paulo)");
            awsRegions.Add("us-east-1", "US East (Virginia)");
            awsRegions.Add("us-east-2", "US East (Ohio)");
            awsRegions.Add("us-west-1", "US West (N. California)");
            awsRegions.Add("us-west-2", "US West (Oregon)");
            awsRegions.Add("cn-north-1", "China (Beijing)");
            awsRegions.Add("cn-northwest-1", "China (Ningxia)");
            awsRegions.Add("us-gov-west-1", "US GovCloud West (Oregon)");



            return awsRegions;
        }
    }
}
