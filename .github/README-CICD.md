# CI/CD Pipeline Documentation

This document describes the CI/CD pipeline configuration for the Task Management System.

## Pipeline Overview

The CI/CD pipeline is implemented using GitHub Actions and consists of four main jobs:

1. **Build and Test** - Builds the application and runs unit/integration tests
2. **Docker Build** - Builds and pushes Docker images to Amazon ECR
3. **Deploy** - Deploys the application to Amazon ECS
4. **Security Scan** - Runs security vulnerability scans

## Workflow Triggers

The pipeline is triggered on:
- Push to `main` or `develop` branches
- Pull requests to `main` branch

## Jobs Description

### 1. Build and Test Job

- **Environment**: Ubuntu Latest
- **Services**: MySQL 8.0 for integration testing
- **Steps**:
  - Checkout code
  - Setup .NET 9.0 SDK
  - Cache NuGet dependencies
  - Restore dependencies
  - Build solution in Release configuration
  - Run unit tests with coverage collection
  - Wait for MySQL service to be ready
  - Run integration tests against MySQL
  - Upload test results and coverage reports

**Service Configuration**:
```yaml
services:
  mysql:
    image: mysql:8.0
    env:
      MYSQL_ROOT_PASSWORD: test_password
      MYSQL_DATABASE: TasksTestDb
    ports:
      - 3306:3306
    options: >-
      --health-cmd="mysqladmin ping -h localhost"
      --health-interval=10s
      --health-timeout=5s
      --health-retries=3
```

### 2. Docker Build Job

- **Condition**: Only runs on `main` branch pushes after successful build
- **Environment**: Ubuntu Latest with AWS permissions
- **Steps**:
  - Configure AWS credentials using OIDC
  - Login to Amazon ECR
  - Setup Docker Buildx for multi-platform builds
  - Extract metadata for tagging
  - Build and push Docker image with caching
  - Support for linux/amd64 platform

**Image Tagging Strategy**:
- `latest` - for main branch
- `{branch}-{sha}` - for feature branches
- `sha-{commit}` - unique identifier for each commit

### 3. Deploy Job

- **Condition**: Only runs after successful Docker build on `main` branch
- **Environment**: Ubuntu Latest with AWS permissions
- **Steps**:
  - Configure AWS credentials
  - Download current ECS task definition
  - Update task definition with new image
  - Deploy to ECS service with stability wait

**ECS Configuration**:
- **Cluster**: `task-management-cluster`
- **Service**: `task-management-service`
- **Container**: `task-management-app`

### 4. Security Scan Job

- **Condition**: Always runs after build (even if build fails)
- **Environment**: Ubuntu Latest
- **Steps**:
  - Checkout code
  - Run Trivy vulnerability scanner on filesystem
  - Upload results to GitHub Security tab as SARIF

## Required Secrets

Configure the following secrets in your GitHub repository:

### AWS Secrets (Recommended: OIDC)
```
AWS_ROLE_TO_ASSUME=arn:aws:iam::ACCOUNT:role/GitHubActionsRole
```

### Alternative: Access Keys (Less Secure)
```
AWS_ACCESS_KEY_ID=your_access_key
AWS_SECRET_ACCESS_KEY=your_secret_key
```

## AWS Infrastructure Requirements

### 1. IAM Role for OIDC (Recommended)

Create an IAM role with the following trust policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::ACCOUNT:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com",
          "token.actions.githubusercontent.com:sub": "repo:YOUR_GITHUB_USER/task-management-system:ref:refs/heads/main"
        }
      }
    }
  ]
}
```

### 2. IAM Permissions

The role/user needs the following permissions:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload",
        "ecr:PutImage"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "ecs:DescribeTaskDefinition",
        "ecs:RegisterTaskDefinition",
        "ecs:UpdateService",
        "ecs:DescribeServices"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "iam:PassRole"
      ],
      "Resource": "arn:aws:iam::ACCOUNT:role/ecsTaskExecutionRole"
    }
  ]
}
```

### 3. ECR Repository

Create an ECR repository:

```bash
aws ecr create-repository --repository-name task-management-system --region us-east-1
```

### 4. ECS Resources

- **Cluster**: `task-management-cluster`
- **Service**: `task-management-service`
- **Task Definition**: `task-management-system`

## Local Development

### Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK
- Git

### Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd task-management-system
```

2. Start the development environment:
```bash
docker-compose up -d mysql
```

3. Run the application:
```bash
dotnet run --project src/BlazorWasm.Server
```

4. Access the application at `http://localhost:5270`

### Testing

Run unit tests:
```bash
dotnet test
```

Run integration tests (requires MySQL):
```bash
docker-compose up -d mysql
dotnet test --filter "Category=Integration"
```

## Monitoring and Logging

### Health Checks

The application exposes a health check endpoint at `/healthz` that verifies:
- Database connectivity
- Application health

### Logging

- **Structured Logging**: Uses Serilog with JSON output
- **Log Aggregation**: Logs are collected by AWS CloudWatch (when deployed to ECS)
- **Request Logging**: All HTTP requests are logged with duration

### Metrics

- **Application Metrics**: Custom metrics for task operations
- **Infrastructure Metrics**: CPU, memory, and network metrics from ECS

## Troubleshooting

### Common Issues

1. **Build Failures**
   - Check .NET SDK version compatibility
   - Verify package references in `.csproj` files
   - Review build logs in GitHub Actions

2. **Test Failures**
   - Ensure MySQL service is healthy before running integration tests
   - Check database connection strings
   - Verify test data setup

3. **Docker Build Issues**
   - Check Dockerfile syntax
   - Verify base image availability
   - Review build context and file paths

4. **Deployment Failures**
   - Verify AWS credentials and permissions
   - Check ECS cluster and service status
   - Review task definition configuration

### Logs and Debugging

- **GitHub Actions Logs**: Available in the Actions tab of your repository
- **Application Logs**: Available in AWS CloudWatch Logs
- **Container Logs**: Use `docker logs` for local debugging

## Security Considerations

1. **Secrets Management**: Use GitHub Secrets for sensitive data
2. **Image Scanning**: Trivy scans for vulnerabilities in dependencies
3. **Access Control**: Limit AWS IAM permissions to minimum required
4. **Network Security**: Use VPC and security groups in AWS
5. **Container Security**: Run containers as non-root user when possible

## Performance Optimization

1. **Build Caching**: Docker layer caching reduces build times
2. **Dependency Caching**: NuGet packages are cached between builds
3. **Multi-stage Builds**: Separate build and runtime environments
4. **Resource Limits**: Configure appropriate CPU and memory limits

## Maintenance

### Regular Tasks

1. **Dependency Updates**: Regularly update NuGet packages and Docker images
2. **Security Patches**: Apply security updates promptly
3. **Log Rotation**: Configure log retention policies
4. **Backup**: Regular database backups (if using persistent storage)

### Monitoring

1. **Pipeline Health**: Monitor build success rates and duration
2. **Application Health**: Monitor health check endpoints
3. **Resource Usage**: Monitor CPU, memory, and storage usage
4. **Error Rates**: Monitor application error rates and response times
