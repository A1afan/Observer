using System;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Observer
{
    interface IObserver
    {
        void Update(string message);
    }

    interface IObgect
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify(string message);
    }

    class RealObserver : IObserver
    {
        private string _name;
        public RealObserver(string name)
        {
            _name = name;
        }
        public void Update(string message)
        {
            Console.WriteLine($"{_name} received message: {message}");
            SendEmail(_name, "New massage", message);

        }
        private void SendEmail(string to, string subject, string body)
        {
            string fromEmail = "sanya.kruty1@gmail.com"; // ← Твоя Gmail адреса
            string clientId = "826924525568-i082uv4bal564be3glr5kdvnhjgbc4n2.apps.googleusercontent.com";
            string clientSecret = "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";
            string refreshToken = "1//09lrg6pUpXh06CgYIARAAGAkSNwF-L9IrMmIH5tyIUisdY_xAOzHOGRqQsDpiG4jqWkVCiXr0wdj6Y4t4lucr65heTOQstLbYTS4";

            // Отримуємо access token
            var tokenResponse = new TokenResponse
            {
                RefreshToken = refreshToken
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            });

            var credential = new UserCredential(flow, "user", tokenResponse);
            bool success = credential.RefreshTokenAsync(CancellationToken.None).Result;

            if (!success)
            {
                Console.WriteLine("Не вдалося оновити access token.");
                return;
            }

            string accessToken = credential.Token.AccessToken;

            // Створення листа
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("System Notifier", fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    var oauth2 = new SaslMechanismOAuth2(fromEmail, accessToken);
                    client.Authenticate(oauth2);

                    client.Send(message);
                    client.Disconnect(true);
                }

                Console.WriteLine($"Email sent to {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка надсилання email до {to}: {ex.Message}");
            }
        }
    }
        

    class RealObject : IObgect
    {
        private List<IObserver> _observers = new List<IObserver>();
        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }
        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }
        public void Notify(string message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(message);
            }
        }

        public void SendMessage(string message)
        {
            Console.WriteLine($"Sending message: {message}");
            Notify(message);
        }
    }
}
