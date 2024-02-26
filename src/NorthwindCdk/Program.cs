using Amazon.CDK;
using NorthwindCdk;

App app = new();

// ReSharper disable once ObjectCreationAsStatement
new NorthwindCdkStack(app, "NorthwindCdkStack");

app.Synth();
