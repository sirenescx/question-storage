using System.Net.Mail;

namespace QuestionStorage.Helpers
{
    public class MessageSender
    {
        private readonly string sender;
        private readonly string password;
        
        public MessageSender(string sender = "qstorage.cshse@gmail.com", string password = "Silva2022")
        {
            this.sender = sender;
            this.password = password;
        }  
        
        public void CreateMessage(string to, string body, string subject)
        {
            var client = new SmtpClient("smtp.gmail.com")
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(sender, password),
                EnableSsl = true,
                Port = 587, 
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var message = new MailMessage(sender, to)
            {
                Subject = subject,
                Body = body
            };

            client.Send(message);
        }
    }
}