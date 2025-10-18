variable "resource_group_name" {
  description = "Name of the Azure resource group"
  type        = string
  default     = "oopsai-resource-group"
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "Southeast Asia"
}

variable "app_service_plan_name" {
  description = "Name of the App Service Plan"
  type        = string
  default     = "oopsai-app-service-plan"
}

variable "app_insights_name" {
  description = "Name of the Application Insights instance"
  type        = string
  default     = "oopsai-app-insights"
}

variable "web_app_name" {
  description = "Name of the Web App"
  type        = string
  default     = "oopsai-web-app"
}

variable "web_app_name_suffix" {
  description = "Suffix to append to web app name for uniqueness"
  type        = string
  default     = ""
}