resource "aws_ses_domain_identity" "email" {
  domain = var.domain
}

resource "aws_ses_domain_dkim" "email" {
  domain = aws_ses_domain_identity.email.domain
}

resource "aws_ses_domain_mail_from" "email" {
  domain                 = aws_ses_domain_identity.email.domain
  mail_from_domain       = "mail.${var.domain}"
  behavior_on_mx_failure = "USE_DEFAULT_VALUE"
}

resource "aws_ses_configuration_set" "email" {
  name = "${var.project_name}-email"
}

resource "aws_iam_policy" "send_email" {
  name = "${var.project_name}-ses-send-email"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = ["ses:SendEmail", "ses:SendRawEmail"]
      Resource = [aws_ses_domain_identity.email.arn]
    }]
  })
}
