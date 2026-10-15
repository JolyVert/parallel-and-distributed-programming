

using System;
using System.Threading;
using System.Threading.Tasks;

namespace lab1
{
    class Program
    {
        static int zloze = 2000;
        static int magazyn = 0;
        const int pojemnoscPojazdu = 200;
        const int czasWydobyciaJednostkiMs = 10;
        const int czasRozladunkuJednostkiMs = 10;
        const int czasPrzejazduMs = 10000;

        static SemaphoreSlim semaforZloza = new SemaphoreSlim(2, 2);
        static SemaphoreSlim semaforMagazynu = new SemaphoreSlim(1, 1);
        static object blokadaZloza = new object();
        static object blokadaMagazynu = new object();
        static object blokadaKonsoli = new object();

        static bool symulacjaTrwa = true;

        static void Main(string[] args)
        {
            Console.Clear();
            Console.CursorVisible = false;

            int liczbaGornikow = 5;
            Task[] gornicy = new Task[liczbaGornikow];

            Task monitor = Task.Run(() => OdswiezajStanOgolny());

            for (int i = 0; i < liczbaGornikow; i++)
            {
                int idGornika = i + 1;
                int liniaKonsoli = i + 3;
                gornicy[i] = Task.Run(() => PracaGornika(idGornika, liniaKonsoli));
            }

            Task.WaitAll(gornicy);

            symulacjaTrwa = false;
            monitor.Wait();

            RysujWConsoli(0, 0, $"Stan złoża: {zloze} jednostek węgla");
            RysujWConsoli(0, 1, $"Stan magazynu: {magazyn} jednostek węgla");
            RysujWConsoli(0, liczbaGornikow + 4, "--- Symulacja zakończona ---");
            Console.ReadLine();
        }

        static void RysujWConsoli(int x, int y, string tekst)
        {
            lock (blokadaKonsoli)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(tekst.PadRight(60));
            }
        }

        static void OdswiezajStanOgolny()
        {
            while (symulacjaTrwa)
            {
                RysujWConsoli(0, 0, $"Stan złoża: {zloze} jednostek węgla");
                RysujWConsoli(0, 1, $"Stan magazynu: {magazyn} jednostek węgla");
                Thread.Sleep(100);
            }
        }

        static void PracaGornika(int id, int liniaWykonsoli)
        {
            RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Gotowy do pracy");

            while (true)
            {
                int iloscDoPobrania = 0;

                RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Czeka na dostęp do złoża...");
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

                RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Wydobywa węgiel...");
                Thread.Sleep(iloscDoPobrania * czasWydobyciaJednostkiMs);

                semaforZloza.Release();

                RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Transportuje do magazynu...");
                Thread.Sleep(czasPrzejazduMs);

                RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Czeka na rozładunek w magazynie...");
                semaforMagazynu.Wait();

                RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Rozładowuje węgiel...");
                Thread.Sleep(iloscDoPobrania * czasRozladunkuJednostkiMs);

                lock (blokadaMagazynu)
                {
                    magazyn += iloscDoPobrania;
                }
                semaforMagazynu.Release();

                RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Wraca do kopalni...");
                Thread.Sleep(czasPrzejazduMs);
            }

            RysujWConsoli(0, liniaWykonsoli, $"Górnik {id}: Zakończył pracę.");
        }
    }
}

