output "public_ip" {
  description = "Reserved public IPv4 address of the Lightsail application instance."
  value       = aws_lightsail_static_ip.app.ip_address
}
