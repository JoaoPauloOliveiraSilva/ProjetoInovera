output "endpoint" {
  description = "Hostname of the Lightsail PostgreSQL database endpoint."
  value       = aws_lightsail_database.postgres.master_endpoint_address
}

output "port" {
  description = "Port of the Lightsail PostgreSQL database endpoint."
  value       = aws_lightsail_database.postgres.master_endpoint_port
}
