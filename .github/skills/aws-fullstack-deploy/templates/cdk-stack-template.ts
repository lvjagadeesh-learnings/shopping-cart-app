import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as ecr from 'aws-cdk-lib/aws-ecr';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import * as acm from 'aws-cdk-lib/aws-certificatemanager';

// Placeholder stack: replace repo names, image tags, health-check paths, and
// sizing (cpu/memory/desiredCount) with real values before deploying.
export class FullStackAppStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const vpc = new ec2.Vpc(this, 'AppVpc', {
      maxAzs: 2,
      natGateways: 1, // set to 0 for dev/staging to cut cost; 2 for prod HA
    });

    const cluster = new ecs.Cluster(this, 'AppCluster', { vpc });

    const backendRepo = ecr.Repository.fromRepositoryName(this, 'BackendRepo', 'app-backend');
    const frontendRepo = ecr.Repository.fromRepositoryName(this, 'FrontendRepo', 'app-frontend');

    const dbSecret = secretsmanager.Secret.fromSecretNameV2(this, 'DbSecret', 'app/db-connection-string');

    const backendTaskDef = new ecs.FargateTaskDefinition(this, 'BackendTaskDef', {
      cpu: 256,
      memoryLimitMiB: 512,
    });
    backendTaskDef.addContainer('BackendContainer', {
      image: ecs.ContainerImage.fromEcrRepository(backendRepo, process.env.IMAGE_TAG ?? 'latest'),
      portMappings: [{ containerPort: 8080 }],
      logging: ecs.LogDrivers.awsLogs({
        streamPrefix: 'backend',
        logGroup: new logs.LogGroup(this, 'BackendLogGroup', { retention: logs.RetentionDays.ONE_MONTH }),
      }),
      secrets: {
        DB_CONNECTION_STRING: ecs.Secret.fromSecretsManager(dbSecret),
      },
    });

    const backendService = new ecs.FargateService(this, 'BackendService', {
      cluster,
      taskDefinition: backendTaskDef,
      desiredCount: 2,
    });

    const frontendTaskDef = new ecs.FargateTaskDefinition(this, 'FrontendTaskDef', {
      cpu: 256,
      memoryLimitMiB: 512,
    });
    frontendTaskDef.addContainer('FrontendContainer', {
      image: ecs.ContainerImage.fromEcrRepository(frontendRepo, process.env.IMAGE_TAG ?? 'latest'),
      portMappings: [{ containerPort: 8080 }],
      logging: ecs.LogDrivers.awsLogs({
        streamPrefix: 'frontend',
        logGroup: new logs.LogGroup(this, 'FrontendLogGroup', { retention: logs.RetentionDays.ONE_MONTH }),
      }),
    });

    const frontendService = new ecs.FargateService(this, 'FrontendService', {
      cluster,
      taskDefinition: frontendTaskDef,
      desiredCount: 2,
    });

    const alb = new elbv2.ApplicationLoadBalancer(this, 'AppAlb', { vpc, internetFacing: true });

    // Replace with a real ACM certificate ARN/domain before deploying to production.
    const certificate = acm.Certificate.fromCertificateArn(this, 'Cert', process.env.ACM_CERT_ARN ?? '');

    const httpsListener = alb.addListener('HttpsListener', {
      port: 443,
      certificates: [certificate],
    });

    alb.addListener('HttpListener', { port: 80 }).addAction('Redirect', {
      action: elbv2.ListenerAction.redirect({ protocol: 'HTTPS', port: '443' }),
    });

    httpsListener.addTargets('BackendTarget', {
      priority: 10,
      conditions: [elbv2.ListenerCondition.pathPatterns(['/api/*'])],
      port: 8080,
      targets: [backendService],
      healthCheck: { path: '/health' },
    });

    httpsListener.addTargets('FrontendTarget', {
      port: 8080,
      targets: [frontendService],
      healthCheck: { path: '/' },
    });

    new cdk.CfnOutput(this, 'AlbDnsName', { value: alb.loadBalancerDnsName });
  }
}
