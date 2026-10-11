variable "project_name" {
  type = string
}

variable "account_id" {
  type = string
}

variable "limit_amount" {
  type = string
}

variable "alert_threshold" {
  type = number
}

variable "notification_email" {
  type = string
}

variable "tags" {
  type = map(string)
}
