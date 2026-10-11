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

variable "key_pair_name" {
  type     = string
  nullable = true
}

variable "admin_cidr" {
  type = string
}

variable "tags" {
  type = map(string)
}
