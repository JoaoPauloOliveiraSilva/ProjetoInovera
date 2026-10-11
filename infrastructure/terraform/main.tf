terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.0, < 7.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

data "aws_caller_identity" "current" {}

module "app_host" {
  source = "./modules/app_host"

  project_name     = var.project_name
  availability_zone = var.availability_zone
  blueprint_id     = var.lightsail_blueprint_id
  bundle_id        = var.lightsail_bundle_id
  key_pair_name    = var.lightsail_key_pair_name
  admin_cidr       = var.admin_cidr
  tags             = var.tags
}

module "database" {
  source = "./modules/database"

  project_name     = var.project_name
  availability_zone = var.availability_zone
  blueprint_id     = var.lightsail_database_blueprint_id
  bundle_id        = var.lightsail_database_bundle_id
  database_name    = var.database_name
  username         = var.database_username
  password         = var.database_password
  backup_window    = var.database_backup_window
  tags             = var.tags
}

module "storage" {
  source = "./modules/storage"

  bucket_name = var.s3_bucket_name
  tags        = var.tags
}

module "email" {
  source = "./modules/email"

  project_name = var.project_name
  domain       = var.ses_domain
  region       = var.aws_region
}

module "budget" {
  source = "./modules/budget"

  project_name      = var.project_name
  account_id        = data.aws_caller_identity.current.account_id
  limit_amount      = var.monthly_budget_limit
  alert_threshold   = var.budget_alert_threshold
  notification_email = var.budget_notification_email
  tags              = var.tags
}