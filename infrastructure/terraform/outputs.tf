output "web_app_url" {
  description = "URL of the deployed Web App"
  value       = "https://${azurerm_windows_web_app.oopsai.default_hostname}"
}

output "app_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = azurerm_application_insights.oopsai.instrumentation_key
  sensitive   = true
}

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.oopsai.name
}