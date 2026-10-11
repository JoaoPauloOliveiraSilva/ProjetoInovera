variable "project_name" {
  type = string
}

variable "availability_zone" {
  type = string
}

variable "blueprint_id" {
  type = string
}

variable "bundle_id" {
  type = string
}

variable "database_name" {
  type = string
}

variable "username" {
  type = string
}

variable "password" {
  type      = string
  sensitive = true
}

variable "backup_window" {
  type = string
}

variable "tags" {
  type = map(string)
}
