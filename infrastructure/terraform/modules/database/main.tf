resource "aws_lightsail_database" "postgres" {
  relational_database_name         = "${var.project_name}-postgres"
  availability_zone                = var.availability_zone
  relational_database_blueprint_id = var.blueprint_id
  bundle_id                        = var.bundle_id
  master_database_name             = var.database_name
  master_username                  = var.username
  master_password                  = var.password
  publicly_accessible              = false
  backup_retention_enabled         = true
  preferred_backup_window          = var.backup_window
  apply_immediately                = false
  tags                             = var.tags
}
