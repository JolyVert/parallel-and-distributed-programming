using System;
using System.Diagnostics;   
using System.Threading;
using System.Threading.Tasks;

namespace lab1
{
    class Program
    {
        const int poczatkoweZloze = 2000;
        const int pojemnoscPojazdu = 200;
        const int czasWydobyciaJednostkiMs = 10;
        const int czasRozladunkuJednostkiMs = 10;
        const int czasPrzejazduMs = 3000;

        static int zloze;
        static int magazyn;
        static SemaphoreSlim semaforZloza;
        static SemaphoreSlim semaforMagazynu;
        static object blokadaZloza = new object();
        static object blokadaMagazynu = new object();

        static void Main(string[] args)
        {
            Console.WriteLine("=== Pomiar przyśpieszenia i efektywności ===\n");

            int maxGornikow = 7;
            double czasDlaJeden = 0;

            for (int p = 1; p <= maxGornikow; p++)
            {
                zloze = poczatkoweZloze;
                magazyn = 0;
                semaforZloza = new SemaphoreSlim(2, 2);
                semaforMagazynu = new SemaphoreSlim(1, 1);

                Stopwatch sw = Stopwatch.StartNew();

                Task[] gornicy = new Task[p];
                for (int i = 0; i < p; i++)
                {
                    gornicy[i] = Task.Run(() => PracaGornika());
                }

                Task.WaitAll(gornicy);
                sw.Stop();

                double czasSekundy = sw.Elapsed.TotalSeconds;

                if (p == 1)
                {
                    czasDlaJeden = czasSekundy;
                }

                double przyspieszenie = czasDlaJeden / czasSekundy;
                double efektywnosc = przyspieszenie / p;

                Console.WriteLine($"liczba górników: {p}, czas: {czasSekundy:F2} s przyśpieszenie: {przyspieszenie:F2} efektywność: {efektywnosc:F2}");
            }

            Console.WriteLine("\n--- Pomiar zakończony ---");
        }

        static void PracaGornika()
        {
            while (true)
            {
                int iloscDoPobrania = 0;

                semaforZloza.Wait();

                lock (blokadaZloza)
                {
                    if (zloze <= 0)
                    {
                        semaforZloza.Release();
                        break;
                    }
                    iloscDoPobrania = Math.Min(pojemnoscPojazdu, zloze);
                    zloze -= iloscDoPobrania;
                }

                Thread.Sleep(iloscDoPobrania * czasWydobyciaJednostkiMs);
                semaforZloza.Release();

                Thread.Sleep(czasPrzejazduMs);

                semaforMagazynu.Wait();

                Thread.Sleep(iloscDoPobrania * czasRozladunkuJednostkiMs);

                lock (blokadaMagazynu)
                {
                    magazyn += iloscDoPobrania;
                }
                semaforMagazynu.Release();

                Thread.Sleep(czasPrzejazduMs);
            }
        }
    }
}

