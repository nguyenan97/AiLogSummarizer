# Locals for common values
locals {
  project_name = "oopsai"
  tags = {
    Project     = "OopsAI"
    Environment = "Development"
    ManagedBy   = "Terraform"
  }
}

# Resource Group
resource "azurerm_resource_group" "oopsai" {
  name     = var.resource_group_name
  location = var.location

  tags = local.tags
}

# App Service Plan
resource "azurerm_service_plan" "oopsai" {
  name                = var.app_service_plan_name
  resource_group_name = azurerm_resource_group.oopsai.name
  location            = azurerm_resource_group.oopsai.location
  os_type             = "Windows"
  sku_name            = "P0v3"

  tags = local.tags
}

# Application Insights
resource "azurerm_application_insights" "oopsai" {
  name                = var.app_insights_name
  location            = azurerm_resource_group.oopsai.location
  resource_group_name = azurerm_resource_group.oopsai.name
  application_type    = "web"

  tags = local.tags
}

# Web App
resource "azurerm_windows_web_app" "oopsai" {
  name                = "${var.web_app_name}${var.web_app_name_suffix}"
  resource_group_name = azurerm_resource_group.oopsai.name
  location            = azurerm_resource_group.oopsai.location
  service_plan_id     = azurerm_service_plan.oopsai.id

  site_config {
    application_stack {
      current_stack = "dotnet"
      dotnet_version = "v9.0"
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.oopsai.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.oopsai.connection_string
  }

  tags = local.tags
}