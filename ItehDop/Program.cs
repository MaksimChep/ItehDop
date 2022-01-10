using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ItehDop
{
    class Program
    {
        public static string GetRequestPostData(HttpListenerRequest request)
        {
            using (Stream body = request.InputStream) // here we have data
            {
                using (StreamReader reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }


        static void Listen(HttpListener listener)
        {
            listener.Start();
            Console.WriteLine("Ожидание подключений...");
            // метод GetContext блокирует текущий поток, ожидая получение запроса 
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            // получаем объект ответа
            HttpListenerResponse response = context.Response;
            // создаем ответ в виде кода html
            CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
            if(request.HasEntityBody)
            {
                string str = GetRequestPostData(request);
                string volume = str.Substring(7);
                defaultPlaybackDevice.Volume = Convert.ToInt32(volume);
            }       
            string responseStr = "<html><head><meta charset='utf8'><script>function slide(){var form = document.getElementById(\"formrange\");form.submit();}</script></head><body><datalist id=\"tickmarks\"><option value=\"0\"><option value=\"10\"><option value=\"20\"><option value=\"30\"><option value=\"40\"><option value=\"50\"><option value=\"60\"><option value=\"70\"><option value=\"80\"><option value=\"90\"><option value=\"100\"></datalist><form method=\"post\" id=\"formrange\"><input type=\"range\" min=\"0\" max=\"100\" step=\"1\" value=\"" + defaultPlaybackDevice.Volume + "\" id = \"volume\" name = \"volume\" onchange = \"slide()\" style=\"width: 300\" list =\"tickmarks\"></form> </body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseStr);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // закрываем поток
            output.Close();
            // останавливаем прослушивание подключений
            listener.Stop();
            Console.WriteLine("Обработка подключений завершена");
        }
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            
            // установка адресов прослушки
            listener.Prefixes.Add("http://*:8765/");
            while (true)
            {
                Listen(listener);
            }         
        }
    }
}
