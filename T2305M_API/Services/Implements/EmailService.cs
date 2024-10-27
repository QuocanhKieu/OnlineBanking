﻿// EmailService.cs
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using T2305M_API.Models;
namespace T2305M_API.Services.Implements
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {

            var smtpClient = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Username, "Online Banking System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Set to true if you're sending HTML content
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailTemplateAsync(string to, string subject, string templateFilePath, Dictionary<string, string> placeholders)
        {

            string template = await File.ReadAllTextAsync(templateFilePath);


            string body = BuildEmailBody(template, placeholders);

            var smtpClient = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Username, "Online Banking System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Set to true if you're sending HTML content
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string BuildEmailBody(string template, Dictionary<string, string> placeholders)
        {

            // Replace placeholders with actual values
            foreach (var placeholder in placeholders)
            {
                template = template.Replace(placeholder.Key, placeholder.Value);
            }

            return template;
        }
    
        
    }

}