output "app_public_ip" {
  description = "Public IPv4 address of the Lightsail application instance."
  value       = module.app_host.public_ip
}

output "app_url_http" {
  description = "Initial HTTP URL for the Lightsail application instance. Configure TLS before production use."
  value       = "http://${module.app_host.public_ip}"
}

output "database_endpoint" {
  description = "Lightsail PostgreSQL endpoint hostname."
  value       = module.database.endpoint
}

output "database_port" {
  description = "Lightsail PostgreSQL endpoint port."
  value       = module.database.port
}

output "s3_bucket_name" {
  description = "Name of the private S3 bucket."
  value       = module.storage.bucket_name
}

output "monthly_budget_name" {
  description = "Name of the AWS monthly cost budget."
  value       = module.budget.name
}

output "ses_domain_verification_token" {
  description = "Publish as a TXT record at _amazonses.<ses_domain> to verify the domain in SES."
  value       = module.email.verification_token
}

output "ses_dkim_tokens" {
  description = "Publish each token as a CNAME record at <token>._domainkey.<ses_domain> using <token>.dkim.amazonses.com as the target."
  value       = module.email.dkim_tokens
}

output "ses_smtp_endpoint" {
  description = "Managed regional Amazon SES SMTP endpoint; no self-hosted SMTP server is created."
  value       = module.email.smtp_endpoint
}

output "ses_smtp_port" {
  description = "Recommended SES SMTP submission port using STARTTLS."
  value       = 587
}

output "ses_mail_from_domain" {
  description = "Custom MAIL FROM subdomain for SES. Publish its MX and SPF DNS records."
  value       = module.email.mail_from_domain
}

output "ses_mail_from_mx_record" {
  description = "MX record value for the SES custom MAIL FROM subdomain."
  value       = module.email.mail_from_mx_record
}

output "ses_mail_from_spf_record" {
  description = "TXT SPF record value for the SES custom MAIL FROM subdomain."
  value       = module.email.mail_from_spf_record
}

output "ses_configuration_set_name" {
  description = "SES configuration set name for use by the application when sending."
  value       = module.email.configuration_set_name
}

output "ses_send_email_policy_arn" {
  description = "Attach this least-privilege IAM policy to the identity used by the application to send via SES."
  value       = module.email.send_email_policy_arn
}
