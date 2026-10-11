output "verification_token" {
  description = "TXT value used to verify the SES domain identity."
  value       = aws_ses_domain_identity.email.verification_token
}

output "dkim_tokens" {
  description = "CNAME tokens used to enable Easy DKIM for the domain."
  value       = aws_ses_domain_dkim.email.dkim_tokens
}

output "smtp_endpoint" {
  description = "Regional managed Amazon SES SMTP endpoint."
  value       = "email-smtp.${var.region}.amazonaws.com"
}

output "mail_from_domain" {
  description = "Configured custom MAIL FROM subdomain."
  value       = aws_ses_domain_mail_from.email.mail_from_domain
}

output "mail_from_mx_record" {
  description = "MX record value required for the custom MAIL FROM domain."
  value       = "10 feedback-smtp.${var.region}.amazonses.com"
}

output "mail_from_spf_record" {
  description = "SPF TXT record value required for the custom MAIL FROM domain."
  value       = "v=spf1 include:amazonses.com -all"
}

output "configuration_set_name" {
  description = "SES configuration set for application email sending."
  value       = aws_ses_configuration_set.email.name
}

output "send_email_policy_arn" {
  description = "IAM policy ARN granting send-only permissions for the SES domain identity."
  value       = aws_iam_policy.send_email.arn
}
