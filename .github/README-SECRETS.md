# GitHub Secrets Configuration

This document outlines the required GitHub repository secrets for the CI/CD pipeline.

## Required Secrets

### AWS Configuration

Configure the following secrets in your GitHub repository:

1. **AWS_ACCESS_KEY_ID**
   - Description: AWS access key for ECR authentication
   - Value: Your AWS IAM user access key ID
   - Usage: Used for AWS CLI authentication in GitHub Actions

2. **AWS_SECRET_ACCESS_KEY**
   - Description: AWS secret access key for ECR authentication
   - Value: Your AWS IAM user secret access key
   - Usage: Used for AWS CLI authentication in GitHub Actions

3. **AWS_REGION**
   - Description: AWS region where ECR repository is located
   - Value: `us-east-1` (or your preferred region)
   - Usage: Specifies the AWS region for ECR operations

4. **ECR_REPOSITORY**
   - Description: ECR repository name for Docker images
   - Value: `task-management-system`
   - Usage: Target repository for Docker image pushes

## Setting Up Secrets

### Step 1: Create AWS IAM User

1. Go to AWS Console → IAM → Users
2. Create a new user: `github-actions-user`
3. Attach policy: `AmazonEC2ContainerRegistryPowerUser`
4. Generate access keys for programmatic access
5. Save the Access Key ID and Secret Access Key

### Step 2: Create ECR Repository

```bash
aws ecr create-repository --repository-name task-management-system --region us-east-1
```

### Step 3: Configure GitHub Secrets

1. Go to your GitHub repository
2. Navigate to Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Add each secret with the values from AWS

### Required IAM Permissions

The IAM user needs the following permissions:

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "ecr:BatchCheckLayerAvailability",
                "ecr:GetDownloadUrlForLayer",
                "ecr:BatchGetImage",
                "ecr:GetAuthorizationToken"
            ],
            "Resource": "*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "ecr:InitiateLayerUpload",
                "ecr:UploadLayerPart",
                "ecr:CompleteLayerUpload",
                "ecr:PutImage"
            ],
            "Resource": "arn:aws:ecr:*:*:repository/task-management-system"
        }
    ]
}
```

## Security Best Practices

1. **Principle of Least Privilege**: Only grant necessary permissions
2. **Rotate Keys Regularly**: Update access keys every 90 days
3. **Monitor Usage**: Enable CloudTrail for API call monitoring
4. **Use Environment-Specific Secrets**: Different keys for staging/production

## Troubleshooting

### Common Issues

1. **Authentication Failed**
   - Verify access key ID and secret are correct
   - Check IAM user has required permissions
   - Ensure AWS region matches ECR repository region

2. **Repository Not Found**
   - Verify ECR repository name matches secret value
   - Check repository exists in specified region
   - Ensure IAM user has access to the repository

3. **Permission Denied**
   - Review IAM policy permissions
   - Check resource ARNs in policy statements
   - Verify repository-specific permissions

### Testing Authentication

Test AWS authentication locally:

```bash
aws sts get-caller-identity --region us-east-1
aws ecr describe-repositories --region us-east-1
```

## Additional Configuration

### Branch Protection Rules

Configure branch protection for `main`:

1. Require status checks to pass
2. Require branches to be up to date
3. Include administrators in restrictions
4. Require review from code owners

### Environment Secrets

For production deployments, consider using GitHub Environments:

1. Create `production` environment
2. Add environment-specific secrets
3. Configure deployment protection rules
4. Require manual approval for production

## Monitoring

Set up monitoring for CI/CD pipeline:

1. **GitHub Actions Usage**: Monitor workflow run minutes
2. **AWS Costs**: Track ECR storage and transfer costs
3. **Success Rate**: Monitor deployment success metrics
4. **Performance**: Track build and deployment times
