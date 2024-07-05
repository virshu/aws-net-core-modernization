using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SecretsManager;
using Constructs;
using System.Net.Http;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SSM;
// ReSharper disable ObjectCreationAsStatement

namespace NorthwindCdk;

public class NorthwindCdkStack : Stack
{
    internal  NorthwindCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // Create VPC
        Vpc vpc = new(this, "LabVpc", new VpcProps());

        // Generate password and store it in Secrets Manager
        Secret dbPassword = new(this, "LabDbPassword", new SecretProps
        {
            SecretName = "DatabasePassword",
            GenerateSecretString = new SecretStringGenerator
            {
                ExcludePunctuation = true
            }
        });
        const string dbUser = "adminuser";

        // Database security group
        SecurityGroup sg = new(this, "NorthwindDatabaseSecurityGroup", new SecurityGroupProps
        {
            Vpc = vpc,

            SecurityGroupName = "Northwind-DB-SG",
            AllowAllOutbound = false
        });

#pragma warning disable S1075
        string externalIp =  new HttpClient().GetStringAsync("http://icanhazip.com").Result.Replace("\n", "/32");
#pragma warning restore S1075
        sg.AddIngressRule(Peer.Ipv4(externalIp), Port.Tcp(1433)); // SQL Server

        // RDS SQL Server instance
        DatabaseInstance sqlServer = new(this, "NorthwindSQLServer", new DatabaseInstanceProps
        {
            Vpc = vpc,

            InstanceIdentifier = "northwind-sqlserver",

            // SQL Server Express
            Engine = DatabaseInstanceEngine.SqlServerEx(new SqlServerExInstanceEngineProps { Version = SqlServerEngineVersion.VER_16 }),

            Credentials = Credentials.FromPassword(
                username: dbUser,
                password: dbPassword.SecretValue),

            // t3.small
            InstanceType = Amazon.CDK.AWS.EC2.InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),

            SecurityGroups = new ISecurityGroup[] { sg },
            MultiAz = false,

            // public subnet
            VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC },

            DeletionProtection = false, // you need to be able to delete database
            DeleteAutomatedBackups = true,
            BackupRetention = Duration.Days(0),
            RemovalPolicy = RemovalPolicy.DESTROY // you need to be able to delete database
        });

        // Assemble database connection string and store it in Systems Manager Parameter Store
        StringParameter connectionString = new(this, "NorthwindDatabaseConnectionString", new StringParameterProps
        {
            ParameterName = "/Northwind/ConnectionStrings/NorthwindDatabase",
            Description = "SQL Server connection string",
            StringValue =
                $"Server={sqlServer.InstanceEndpoint.Hostname},1433;Integrated Security=false;User ID={dbUser};Password={dbPassword.SecretValue.UnsafeUnwrap()};Initial Catalog=NorthwindTraders;"
        });

        // Display connection string as output
        new CfnOutput(this, "SQLServerConnectionString", new CfnOutputProps
        {
            Value = connectionString.StringValue
        });
    }
}
