using MMS.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MMS.Classes
{
    public static class Fn
    {
        public enum CrudMode
        {
            Create,
            Edit,
            Details,
            Delete,
            Other
        }

        public static string RemoveLastString(string text, string remove)
        {
            if (text.EndsWith(remove))
                text = text.Remove(text.Length - remove.Length);

            return text;
        }

        public static string EncryptText(string text)
        {
            string encrypted = "";
            try
            {
                UnicodeEncoding codificador = new UnicodeEncoding();

                byte[] datos = codificador.GetBytes(text);

                SHA1 encripta = new SHA1CryptoServiceProvider();
                byte[] resultado;
                resultado = encripta.ComputeHash(datos);

                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < resultado.Length; i++)
                    sBuilder.Append(resultado[i].ToString("x2"));

                encrypted = sBuilder.ToString();
            }
            catch
            {
                return "";
            }
            return encrypted;
        }

        public static string GetJsonString(object data)
        {
            string jsonString = null;

            if (data != null)
            {
                jsonString = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(data);
                jsonString = Newtonsoft.Json.Linq.JObject.Parse(jsonString).ToString(Newtonsoft.Json.Formatting.Indented);
            }

            return jsonString;
        }

        public static void AddLog(string usuario, string evento, string key, object data, string client)
        {
            try
            {
                using (var db = new MMSContext())
                {
                    int? eventoId = db.Evento.FirstOrDefault(e => e.Nombre == evento && e.Activo)?.Id;
                    if (eventoId == null)
                        return;

                    var log = new Log()
                    {
                        Fecha = DateTime.Now,
                        Usuario = string.IsNullOrWhiteSpace(usuario) ? "Anonymous" : usuario,
                        Data = Fn.GetJsonString(data),
                        Cliente = client,
                        EventoId = (int)eventoId,
                        Key = key
                    };

                    db.Log.Add(log);
                    db.SaveChanges();
                }
            }
            catch { }
        }

        public static bool SendEmail(string to, string subject, string mensaje)
        {
            using (var db = new MMSContext())
            {
                var tbconfig = db.Database.SqlQuery<Configuracion>("SELECT TOP 1 * FROM Configuracion").First();
                var mail = new MailMessage();
                var smtp = new SmtpClient();

                try
                {
                    mail.From = new MailAddress(tbconfig.ConfigSmtpFrom);
                    mail.To.Add(new MailAddress(to));
                    mail.Subject = subject;
                    mail.IsBodyHtml = true;
                    mail.Body = mensaje;
                    smtp.Host = tbconfig.ConfigSmtpHost;
                    smtp.Port = (int)tbconfig.ConfigSmtpPort;
                    smtp.Credentials = new NetworkCredential(tbconfig.ConfigSmtpUserName, tbconfig.ConfigSmtPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return false;
                }
            }
        }

        public static void SendHtmlEmail(string to, string toName, string subject, string msg, string app){

            string body = string.Empty;
            using (var sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/Email_Base.html"))
                body = sr.ReadToEnd();

            body = body.Replace("{username}", toName);
            body = body.Replace("{message}", msg);

            using (var mail = new MailMessage())
            {
                using (var db = new MMSContext())
                {
                    var config = db.Configuracion.First();
                    body = body.Replace("{url}", $"http://{app}.{config.ConfigDominioWeb}");

                    mail.From = new MailAddress(config.ConfigSmtpFrom);
                    mail.To.Add(new MailAddress(to));
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    var smtp = new SmtpClient()
                    {
                        Host = config.ConfigSmtpHost,
                        UseDefaultCredentials = config.configSmtpUseDefaultCredentials ?? true,
                        EnableSsl = config.configSmtpUseDefaultCredentials ?? true,
                        Credentials = (config.configSmtpUseDefaultCredentials ?? true) ? new NetworkCredential(config.ConfigSmtpUserName, config.ConfigSmtPassword) : null,
                        Port = (int)config.ConfigSmtpPort,
                    };
                    smtp.Send(mail);
                }
            }
        }

        public static Bitmap ScaleBitmap(Bitmap bmp, int side)
        {
            int sideBase = Math.Max(bmp.Width, bmp.Height);
            float ratio = (float)side / (float)sideBase;

            return new Bitmap(bmp, new Size(Convert.ToInt32(bmp.Width * ratio), Convert.ToInt32(bmp.Height * ratio)));
        }

        public static byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }

        public static byte[] BitmapToByte(Bitmap image)
        {
            var converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        public class EnumData
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }

        public static IEnumerable<EnumData> EnumToIEnumarable<T>()
        {
            if (typeof(T).BaseType != typeof(Enum))
                throw new InvalidCastException();

            var enumValues = (T[])Enum.GetValues(typeof(T));
            var enumList = from value in enumValues
                           select new EnumData { Value = Convert.ToInt32(value), Name = value.ToString().Replace("__", "-").Replace("_", " ") };

            return enumList;
        }

        public static byte[] ConvertToByte(HttpPostedFileBase file)
        {
            byte[] imageByte = null;
            BinaryReader rdr = new BinaryReader(file.InputStream);
            imageByte = rdr.ReadBytes((int)file.ContentLength);
            return imageByte;
        }

        public class DummyInt
        {
            public int Value { get; set; }
        }

        public static string HASH(string cadena)
        {
            SHA1 sha1 = SHA1Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha1.ComputeHash(encoding.GetBytes(cadena));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);

            return sb.ToString().ToUpper();
        }

        public static Bitmap ResizeBitmap(Bitmap image, int maxWidth, int maxHeight)
        {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }


        public static int GetDv(string nit)
        {
            string temp;
            int count, res = 0, acum = 0;
            int[] vector = new int[] { 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };

            for (count = 0; count < nit.Length; count++)
            {
                temp = nit[(nit.Length - 1) - count].ToString();
                acum += Convert.ToInt32(temp) * vector[count];
            }

            res = acum % 11;

            if (res > 1)
                return 11 - res;

            return res;
        }

       
    }
}