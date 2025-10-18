# OopsAI Terraform Infrastructure

This directory contains Terraform configuration files to provision Azure resources for the OopsAI project.

## Installing Terraform

To install Terraform, follow these simple steps:

1. Visit the [Terraform downloads page](https://www.terraform.io/downloads.html).
2. Download the appropriate version for your operating system (e.g., Linux, macOS, or Windows).
3. Extract the downloaded file and move the `terraform` binary to a directory in your system's PATH (e.g., `/usr/local/bin` on Linux/macOS or `C:\Program Files\Terraform` on Windows).

For detailed installation instructions, refer to the [official Terraform documentation](https://learn.hashicorp.com/tutorials/terraform/install-cli).

To check your local Terraform version, open a terminal and run:

```
terraform --version
```

Ensure you have Terraform version 1.0 or higher installed.

## Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) installed (version 1.0+)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- Azure subscription with appropriate permissions

## Setup

1. **Install Azure CLI:**
   Follow the instructions at https://docs.microsoft.com/en-us/cli/azure/install-azure-cli to install Azure CLI on your system.

2. **Login to Azure:**
    ```bash
    az login
    ```
    This will open a browser window for authentication. Alternatively, you can use service principal authentication if preferred.

3. **Verify your subscription:**
    ```bash
    az account show
    ```
    Ensure you are logged in to the correct subscription. If you have multiple subscriptions, you can set the active one with:
    ```bash
    az account set --subscription "Your-Subscription-ID"
    ```

4. **Navigate to the Terraform directory:**
    ```bash
    cd infrastructure/terraform
    ```

5. **Copy the example variables file:**
    ```bash
    cp terraform.tfvars.example terraform.tfvars
    ```
    Edit `terraform.tfvars` to customize values, especially the `web_app_name_suffix` for global uniqueness.

6. **Initialize Terraform:**
    ```bash
    terraform init
    ```
    This downloads the necessary provider plugins.

## Deployment

1. **Plan the deployment:**
    ```bash
    terraform plan
    ```
    Review the plan to ensure it matches your expectations. This step is optional but recommended.

2. **Apply the configuration:**
    ```bash
    terraform apply
    ```
    Type `yes` when prompted to confirm the deployment.

## Resources Created

- **Resource Group**: `oopsai-resource-group` in Southeast Asia region
- **App Service Plan**: `oopsai-app-service-plan` (Premium V3 P0V3 SKU, Windows OS)
- **Application Insights**: `oopsai-app-insights` for monitoring and telemetry
- **Web App**: `oopsai-web-app-{suffix}` (.NET 9 runtime stack, globally unique URL)

## Outputs

After deployment, Terraform will output:
- **Web App URL**: The HTTPS URL of the deployed web application
- **Application Insights Instrumentation Key**: Key for configuring application monitoring (marked as sensitive)
- **Resource Group Name**: Name of the created resource group

## Cleanup

To destroy all resources when no longer needed:
```bash
terraform destroy
```
Type `yes` when prompted to confirm destruction.

## Customization

You can customize the deployment by:
- Modifying variables in `terraform.tfvars`
- Overriding variables during apply: `terraform apply -var="resource_group_name=my-custom-rg"`
- Editing the configuration files directly for advanced customizations

## Security Notes

- No hardcoded secrets are used in the configuration
- Application Insights instrumentation key is marked as sensitive in outputs
- Ensure your Azure account has appropriate permissions for resource creation