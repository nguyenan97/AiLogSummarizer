# CI/CD Deployment Guide

## Introduction to CI/CD

### What is CI/CD?

CI/CD stands for Continuous Integration and Continuous Deployment (or Delivery). It's a methodology that automates the process of integrating code changes from multiple contributors and deploying them to production environments.

- **Continuous Integration (CI)**: Developers frequently merge code changes into a central repository. Each merge triggers automated builds and tests to detect issues early.
- **Continuous Deployment (CD)**: Automatically deploys code changes to production after passing CI tests, ensuring fast and reliable releases.

### Why is CI/CD Useful?

- **Faster Releases**: Automates repetitive tasks, reducing manual errors and speeding up deployment.
- **Improved Quality**: Automated testing catches bugs before they reach production.
- **Better Collaboration**: Teams can work on features simultaneously without conflicts.
- **Reliability**: Consistent deployment processes reduce downtime and human errors.

### How GitHub Actions Fits In

GitHub Actions is a CI/CD platform integrated into GitHub. It allows you to automate workflows directly from your repository. Workflows are defined in YAML files stored in the `.github/workflows/` directory. Actions can be triggered by events like pushes, pull requests, or manually.

## Prerequisites

Before setting up the deployment pipeline, ensure you have:

1. A GitHub repository with your .NET 9 Web API project.
2. An Azure subscription with a provisioned Web App (using Terraform as mentioned).
3. Azure CLI installed locally (for testing, optional).
4. Basic knowledge of Azure Portal and GitHub.

## Step-by-Step Configuration

### 1. Enable GitHub Actions

GitHub Actions is enabled by default for public repositories. For private repositories:

1. Go to your repository on GitHub.
2. Click on the "Settings" tab.
3. Scroll down to "Actions" in the left sidebar.
4. Under "Actions permissions", select "Allow all actions and reusable workflows".

### 2. Set Up Azure Credentials

To deploy to Azure, you need to configure authentication. We'll use a Service Principal for secure, automated access.

#### Create a Service Principal in Azure

1. Log in to the [Azure Portal](https://portal.azure.com/).
2. Open the Azure Cloud Shell (terminal icon in the top-right) or use Azure CLI locally.
3. Run the following command to create a Service Principal:

   ```
   az ad sp create-for-rbac --name "GitHubActionsSP" --role contributor --scopes /subscriptions/<YOUR_SUBSCRIPTION_ID>/resourceGroups/<YOUR_RESOURCE_GROUP> --sdk-auth
   ```

   Replace `<YOUR_SUBSCRIPTION_ID>` with your Azure subscription ID and `<YOUR_RESOURCE_GROUP>` with the resource group containing your Web App.

4. The command will output JSON credentials. Copy this output - you'll need it for GitHub secrets.

#### Obtain Azure Details

You'll need the following information:

- **Subscription ID**: Found in Azure Portal > Subscriptions > Your Subscription > Overview > Subscription ID.
- **Tenant ID**: From the Service Principal creation output (or Azure Portal > Azure Active Directory > Properties > Tenant ID).
- **Client ID**: From the Service Principal creation output.
- **Client Secret**: From the Service Principal creation output.
- **Resource Group Name**: The name of your resource group (e.g., from Terraform outputs).
- **Web App Name**: The name of your Azure Web App (e.g., from Terraform outputs).

### 3. Configure GitHub Secrets

1. Go to your GitHub repository.
2. Click on "Settings" > "Secrets and variables" > "Actions".
3. Click "New repository secret" and add the following:

   - `AZURE_CREDENTIALS`: Paste the entire JSON output from the Service Principal creation.
   - `AZURE_RESOURCE_GROUP`: Your Azure resource group name.
   - `AZURE_WEBAPP_NAME`: Your Azure Web App name.

   These secrets will be used by the GitHub Actions workflow to authenticate with Azure.

### 4. Deploy the Workflow File

The deployment workflow is defined in `.github/workflows/deploy.yml`. This file should already be in your repository. If not, create it with the following content:

```yaml
name: Deploy to Azure Web App

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Deployment environment'
        required: true
        default: 'production'
        type: choice
        options:
        - production
        - staging

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore ./src/WebApi/WebApi.csproj

    - name: Build project
      run: dotnet build ./src/WebApi/WebApi.csproj --configuration Release --no-restore

    - name: Publish project
      run: dotnet publish ./src/WebApi/WebApi.csproj --configuration Release --output ./publish --no-build

    - name: Login to Azure
      uses: azure/login@v2
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}

    - name: Deploy to Azure Web App
      run: |
        az webapp deployment source config-zip --resource-group ${{ secrets.AZURE_RESOURCE_GROUP }} --name ${{ secrets.AZURE_WEBAPP_NAME }} --src ./publish
```

## Triggering Manual Deployment

1. Go to your GitHub repository.
2. Click on the "Actions" tab.
3. Select the "Deploy to Azure Web App" workflow from the left sidebar.
4. Click "Run workflow" on the right.
5. Choose the environment (production or staging) and click "Run workflow".

The workflow will start, and you can monitor its progress in real-time.

## Monitoring Deployment

1. In the Actions tab, click on the running workflow.
2. Each step will show its status (queued, in progress, success, or failure).
3. Click on individual steps to view logs and troubleshoot issues.
4. If deployment succeeds, your Web App will be updated automatically.

## Troubleshooting

- **Authentication Issues**: Double-check your Azure credentials and secrets.
- **Build Failures**: Ensure your project builds locally with `dotnet build`.
- **Deployment Errors**: Check Azure Web App logs in the Azure Portal.
- **Permissions**: Ensure the Service Principal has contributor access to the resource group.

## Best Practices

- Regularly rotate your Service Principal credentials.
- Use environment-specific secrets for different deployment targets.
- Test deployments in staging before production.
- Monitor workflow runs and set up notifications for failures.