variable "project_name" {
  description = "Short prefix used in AWS resource names."
  type        = string
  default     = "innovera"

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{2,20}$", var.project_name))
    error_message = "project_name must be 3-21 lowercase letters, numbers, or hyphens, starting with a letter."
  }
}

variable "aws_region" {
  description = "AWS region for all resources."
  type        = string
  default     = "eu-west-1"
}

variable "availability_zone" {
  description = "Lightsail Availability Zone in aws_region."
  type        = string
  default     = "eu-west-1a"
}

variable "lightsail_blueprint_id" {
  description = "Lightsail instance operating system blueprint ID supported in the selected region."
  type        = string
  default     = "amazon_linux_2023"
}

variable "lightsail_bundle_id" {
  description = "Lightsail instance bundle ID (plan size), supported in the selected region."
  type        = string
  default     = "nano_3_0"
}

variable "lightsail_key_pair_name" {
  description = "Existing Lightsail SSH key pair name. Leave null to use the default key pair."
  type        = string
  default     = null
  nullable    = true
}

variable "admin_cidr" {
  description = "Single trusted IPv4 CIDR allowed to connect to the instance over SSH (port 22)."
  type        = string

  validation {
    condition     = can(cidrnetmask(var.admin_cidr)) && var.admin_cidr != "0.0.0.0/0"
    error_message = "admin_cidr must be a valid IPv4 CIDR other than 0.0.0.0/0."
  }
}

variable "lightsail_database_blueprint_id" {
  description = "Lightsail managed database PostgreSQL blueprint ID supported in the selected region."
  type        = string
  default     = "postgres_16"
}

variable "lightsail_database_bundle_id" {
  description = "Lightsail managed database bundle ID (plan size), supported in the selected region."
  type        = string
  default     = "micro_2_0"
}

variable "database_name" {
  description = "Initial PostgreSQL database name."
  type        = string
  default     = "innovera"
}

variable "database_username" {
  description = "Master PostgreSQL username."
  type        = string
  default     = "innovera_admin"
}

variable "database_password" {
  description = "Master PostgreSQL password. Supply securely using TF_VAR_database_password or a secret manager."
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.database_password) >= 8 && length(var.database_password) <= 64
    error_message = "database_password must be between 8 and 64 characters."
  }
}

variable "database_backup_window" {
  description = "Daily UTC backup window in HH:MM-HH:MM format."
  type        = string
  default     = "03:00-04:00"
}

variable "s3_bucket_name" {
  description = "Globally unique S3 bucket name."
  type        = string

  validation {
    condition     = length(var.s3_bucket_name) >= 3 && length(var.s3_bucket_name) <= 63
    error_message = "s3_bucket_name must be between 3 and 63 characters."
  }
}

variable "ses_domain" {
  description = "Email sending domain to verify with Amazon SES. DNS verification records must be published separately."
  type        = string
}

variable "monthly_budget_limit" {
  description = "Monthly AWS cost budget limit in USD."
  type        = string
  default     = "50"

  validation {
    condition     = can(regex("^[0-9]+([.][0-9]{1,2})?$", var.monthly_budget_limit)) && tonumber(var.monthly_budget_limit) > 0
    error_message = "monthly_budget_limit must be a positive amount in USD, with up to two decimal places."
  }
}

variable "budget_alert_threshold" {
  description = "Actual monthly spend percentage at which to send a budget alert."
  type        = number
  default     = 80

  validation {
    condition     = var.budget_alert_threshold > 0 && var.budget_alert_threshold <= 100
    error_message = "budget_alert_threshold must be greater than 0 and at most 100."
  }
}

variable "budget_notification_email" {
  description = "Email address that receives AWS budget notifications."
  type        = string
}

variable "tags" {
  description = "Tags applied to supported AWS resources."
  type        = map(string)
  default = {
    Project   = "Innovera"
    ManagedBy = "Terraform"
  }
}
