using System;
using System.Configuration;
using Microsoft.Owin.Hosting;

namespace EvoEduNet.API
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "http://localhost:5000/";

            try
            {
                using (WebApp.Start<Startup>(baseUrl))
                {
                    Console.Title = "EvoEduNet API - .NET Framework 4.8";
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("==================================================================");
                    Console.WriteLine("  🚀 EvoEduNet.API - Controle de Matrículas Escolares (.NET 4.8)");
                    Console.WriteLine("==================================================================");
                    Console.ResetColor();
                    Console.WriteLine($"[INFO] Servidor HTTP iniciado em: {baseUrl}");
                    Console.WriteLine($"[INFO] Teste de conectividade: {baseUrl}api/status");
                    Console.WriteLine("------------------------------------------------------------------");
                    Console.WriteLine("Pressione [Ctrl+C] ou [Enter] para encerrar o servidor...");

                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FALHA DE INICIALIZAÇÃO] Não foi possível iniciar o servidor em {baseUrl}");
                Console.WriteLine($"Detalhes: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Causa: {ex.InnerException.Message}");
                }
                Console.ResetColor();
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
            }
        }
    }
}
