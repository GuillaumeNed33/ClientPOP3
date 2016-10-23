/* NEDELEC Guillaume et POIRIER Antoine */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;


namespace ConsoleClientPOP3_Mai2015
{
    class Program
    {
        static bool VERBOSE = false;
        static string username = "mstinfo1";
        static string password = "master";
        static string nomServeur = "pop.free.fr";

        static void FATAL(string Message)
        {
            Console.WriteLine(Message);
            Console.ReadLine();
            Environment.Exit(-1);
        }


        static void LireLigne(StreamReader input, out string ligne)
        {
            ligne = input.ReadLine();
            if (VERBOSE)
                Console.WriteLine("     recu  >> " + ligne);
        }
        static void EcrireLigne(StreamWriter output, string ligne)
        {
            output.WriteLine(ligne);
            if (VERBOSE)
                Console.WriteLine("     envoi << " + ligne);
        }

        static void travail(TcpClient socketClient)
        {
            // Stream pour lecture et écriture
            StreamWriter sw;
            StreamReader sr;

            if (socketClient.Connected)
            {
                // connexion ok, mise en place des streams pour lecture et écriture
                sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
                sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
                sw.AutoFlush = true;

                string ligne, tampon;

                /* reception banniere */
                LireLigne(sr, out ligne);  // ou  ligne = sr.ReadLine();
                if (ligne[0] != '+')
                {
                    FATAL("Pas de banniere. Abandon");
                };

                /* envoi identification */
                tampon = "USER " + username;
                EcrireLigne(sw, tampon);   // ou  sw.WriteLine(tampon);
                LireLigne(sr, out ligne);  // ou  ligne = sr.ReadLine();
                if (ligne[0] != '+')
                {
                    FATAL("USER rejeté. Abandon");
                };

                /* envoi mot de passe */
                tampon = "PASS " + password;
                EcrireLigne(sw, tampon);
                LireLigne(sr, out ligne);
                if (ligne[0] != '+')
                {
                    FATAL("PASS rejeté. Abandon");
                }

                /* envoi STAT pour recuperer nb messages */
                tampon = "STAT";
                EcrireLigne(sw, tampon);


                /* reception de +OK n mm */
                LireLigne(sr, out ligne);
                string[] lesValeurs = ligne.Split(' ');
                int nombre_de_messages = Int32.Parse(lesValeurs[1]);
                int taille_boite = Int32.Parse(lesValeurs[2]);
                Console.Write("Il y a " + nombre_de_messages.ToString() + " messages dans la boite.\n");
                Console.Write("La taille totale est de " + taille_boite.ToString() + " octets.\n");
            }
        }

        static TcpClient connexion(string nomServeur, int port)
        {
            TcpClient socketClient = new TcpClient();

            //(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            IPAddress adresse = IPAddress.Parse("127.0.0.1");
            //IPAddress adresse;
            bool trouve = false;
            IPAddress[] adresses = Dns.GetHostAddresses(nomServeur);
            foreach (IPAddress ip in adresses)
            {//on cherche la première adresse IPV4
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    trouve = true;
                    adresse = ip;
                    break;
                }
            }
            if (!trouve)
            {
                FATAL("Echec recherche IP serveur");
            }
            socketClient.Connect(adresse, port);
            return socketClient;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Démarrage du client\n");
            TcpClient socketClient;

            int port = 110;

            socketClient = connexion(nomServeur, port);
            travail(socketClient);

            MenuPrincipal();
            ChoixMenuPrincipal(socketClient);
        }

        /***** Partie Principale *****/
        static void MenuPrincipal()
        {
            Console.WriteLine("\nMenu Principal\n");
            Console.WriteLine("1. Gérer les messages");
            Console.WriteLine("2. Quitter le client\n");
        }

        static void ChoixMenuPrincipal(TcpClient socketClient)
        {
            int quit = 2;
            int liste = 1;

            string userinput = Console.ReadLine();

            while (userinput != quit.ToString() && userinput != liste.ToString())
            {
                Console.WriteLine("\nCommande introuvable ! Réessayez.\n");
                userinput = Console.ReadLine();
            }
            if (userinput == liste.ToString())
            {
                GestionMessage(socketClient);
            }
            else if (userinput == quit.ToString())
            {
                socketClient.Close();
                Console.WriteLine("\nFin du client -> Taper une touche pour terminer");
            }
            Console.ReadLine();
        }

        /***** Partie Messages *****/
        static void GestionMessage(TcpClient socketClient)
        {
            Console.WriteLine();
            MenuMessages();
            ChoixMenuMessages(socketClient);
        }

        static void MenuMessages()
        {
            Console.WriteLine("\nMenu Messages\n");
            Console.WriteLine("1. Récuperer un message");
            Console.WriteLine("2. Supprimer un message");
            Console.WriteLine("3. Afficher la liste des messages");
            Console.WriteLine("4. Informations sur les messages (Expediteur et Objet)");
            Console.WriteLine("5. Quitter le client");
        }

        static void ChoixMenuMessages(TcpClient socketClient)
        {
            string userinput = Console.ReadLine();
            int recup = 1;
            int suppr = 2;
            int liste = 3;
            int info = 4;
            int quit = 5;

            while (userinput != quit.ToString() && userinput != recup.ToString() && userinput != suppr.ToString() && userinput != liste.ToString() && userinput != info.ToString())
            {
                Console.WriteLine("\nCommande introuvable ! Réessayez.\n");
                userinput = Console.ReadLine();
            }
            if (userinput == recup.ToString())
            {
                GestionLectureMessage(socketClient);
            }
            else if (userinput == suppr.ToString())
            {
                GestionSupprMessage(socketClient);
            }
            else if (userinput == liste.ToString())
            {
                CommandLIST(socketClient);
                GestionMessage(socketClient);
            }
            else if (userinput == info.ToString())
            {
                GestionInformations(socketClient);
            }
            else if (userinput == quit.ToString())
            {
                socketClient.Close();
                Console.WriteLine("\nFin du client -> Taper une touche pour terminer");
            }
        }

        /**** Resultat Commande LIST ****/
        static void CommandLIST(TcpClient socketClient)
        {
            StreamWriter sw;
            StreamReader sr;
            sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
            sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
            sw.AutoFlush = true;

            string ligne, tampon;
            tampon = "LIST";

            EcrireLigne(sw, tampon);
            LireLigne(sr, out ligne);

            Console.WriteLine("\nListe des messages :\n");
            while (ligne != ".")
            {
                LireLigne(sr, out ligne);
                if (ligne != ".")
                {
                    string[] decoup = ligne.Split(' ');
                    int numero = Int32.Parse(decoup[0]);
                    int taille = Int32.Parse(decoup[1]);
                    Console.WriteLine("Message n°" + numero.ToString() + " de taille " + taille.ToString() + " octets.");
                }
            }
        }

        /***** Recuperation des Messages *****/
        static void GestionLectureMessage(TcpClient socketClient)
        {
            Console.WriteLine("\nX. Lire le message X");
            Console.WriteLine("INFO X. Récuperer les infos sur le message X (Expediteur et Objet)");
            Console.WriteLine("LIST. Afficher la liste des messages");
            Console.WriteLine("0. Retour au Menu des Messages");
            Console.WriteLine("QUIT. Quitter le client");
            ChoixLectureMessage(socketClient);
        }

        static void ChoixLectureMessage(TcpClient socketClient)
        {
            string userinput = Console.ReadLine();
            int retour = 0;
            int nombreMessage = nbMessage(socketClient);
            string quit = "QUIT";
            string liste = "LIST";
            bool messageRecup = false;
            string info = "INFO ";

            int[] message = new int[nombreMessage];

            for (int i = 0; i < nombreMessage; i++)
            {
                message[i] = i + 1;
            }

        label:

            foreach (int i in message)
            {
                if (userinput == i.ToString())
                {
                    messageRecup = true;
                    RetierMessage(socketClient, i);
                    GestionLectureMessage(socketClient);
                }
                else if (userinput == info + i.ToString())
                {
                    Afficherinfo(socketClient, i, 1);
                }
            }

            if (userinput == retour.ToString())
            {
                GestionMessage(socketClient);
            }

            else if (userinput == quit)
            {
                socketClient.Close();
                Console.WriteLine("\nFin du client -> Taper une touche pour terminer");
            }

            else if (userinput == liste)
            {
                CommandLIST(socketClient);
                GestionLectureMessage(socketClient);
            }

            while (userinput != quit && userinput != retour.ToString() && !messageRecup && userinput != liste)
            {
                Console.WriteLine("\nCe message n'existe pas ! Réessayez.\n");
                userinput = Console.ReadLine();
                goto label;
            }
        }

        static void RetierMessage(TcpClient socketClient, int i)
        {
            StreamWriter sw;
            StreamReader sr;
            sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
            sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
            sw.AutoFlush = true;

            string ligne, tampon;
            bool debut = false;
            string message = "";

            tampon = "RETR " + i.ToString();
            EcrireLigne(sw, tampon);
            LireLigne(sr, out ligne);

            if (ligne[0] != '+')
            {
                Console.WriteLine("\nMessage introuvable.\n");
            }
            else
            {
                while (ligne != ".")
                {
                    if (debut)
                    {
                        message += ligne + "\n";
                    }
                    else if (ligne == "")
                    {
                        debut = true;
                    }

                    LireLigne(sr, out ligne);
                }

                string[] decoup = message.Split('\n');
                string new_message = "";
                
                for(int k = 0; k < decoup.Length ; k++)
                {
                    if(decoup[k].IndexOf('.') == 0)
                    {
                        new_message += decoup[k].Substring(1) +"\n";
                    }
                    else
                    {
                        new_message += decoup[k] + "\n";
                    }
                }

                Afficherinfo(socketClient, i, 3);
                Console.WriteLine("Message: \n\n" + new_message);
            }
        }

        /***** Partie Suppression de messages *****/
        static void GestionSupprMessage(TcpClient socketClient)
        {
            Console.WriteLine("\nX. Supprimer le message X");
            Console.WriteLine("ALL. Supprimer tous les messages");
            Console.WriteLine("INFO X. Donne les informations sur le message X (Expediteur et Objet)");
            Console.WriteLine("LIST. Afficher la liste des messages");
            Console.WriteLine("0. Retour au Menu des Messages");
            Console.WriteLine("QUIT. Quitter le client");
            ChoixSupprMessage(socketClient);
        }

        static void ChoixSupprMessage(TcpClient socketClient)
        {
            string userinput = Console.ReadLine();
            int retour = 0;
            string all = "ALL";
            int nombreMessage = nbMessage(socketClient);
            string quit = "QUIT";
            bool messageSuppr = false;
            string liste = "LIST";
            string info = "INFO ";

            int[] message = new int[nombreMessage];
            for (int i = 0; i < nombreMessage; i++)
            {
                message[i] = i + 1;
            }

        label:

            foreach (int i in message)
            {
                if (userinput == i.ToString())
                {
                    messageSuppr = true;
                    SupprMessage(socketClient, i);
                    GestionSupprMessage(socketClient);
                }

                else if (userinput == info + i.ToString())
                {
                    Afficherinfo(socketClient, i, 2);
                }
            }

            if (userinput == retour.ToString())
            {
                GestionMessage(socketClient);
            }

            else if (userinput == quit)
            {
                socketClient.Close();
                Console.WriteLine("\nFin du client -> Taper une touche pour terminer");
            }

            else if (userinput == liste)
            {
                CommandLIST(socketClient);
                GestionSupprMessage(socketClient);
            }

            else if (userinput == all)
            {
                foreach (int i in message)
                {
                    SupprMessage(socketClient, i);
                }
                GestionSupprMessage(socketClient);
            }

            while (userinput != quit && userinput != liste && userinput != retour.ToString() && userinput != all && !messageSuppr)
            {
                Console.WriteLine("\nCe message n'existe pas ! Réessayez.\n");
                userinput = Console.ReadLine();
                goto label;
            }
        }

        static void SupprMessage(TcpClient socketClient, int i)
        {
            StreamWriter sw;
            StreamReader sr;
            sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
            sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
            sw.AutoFlush = true;

            string ligne, tampon;

            tampon = "DELE " + i.ToString();
            EcrireLigne(sw, tampon);
            LireLigne(sr, out ligne);
            if (ligne[0] != '+')
            {
                Console.WriteLine("\nMessage introuvable.\n");
            }
            else
            {
                Console.WriteLine("\nLe message " + i.ToString() + " a bien été supprimé.");
            }
        }

        /****** Partie Informations sur les messages *****/
        static void GestionInformations(TcpClient socketClient)
        {
            /* Sous Menu */
            Console.WriteLine("\nX. Récuperer les infos du message X");
            Console.WriteLine("ALL. Récuperer les infos de TOUS les messages");
            Console.WriteLine("ADD. Récuperer un message");
            Console.WriteLine("SUPPR. Supprimer un message");
            Console.WriteLine("LIST. Afficher la liste des messages");
            Console.WriteLine("0. Retour au Menu des Messages");
            Console.WriteLine("QUIT. Quitter le client");
            ChoixInformation(socketClient);
        }

        static void ChoixInformation(TcpClient socketClient)
        {
            string userinput = Console.ReadLine();
            int nombreMessage = nbMessage(socketClient);
            int retour = 0;
            string all = "ALL";
            string quit = "QUIT";
            string liste = "LIST";
            string suppr = "SUPPR";
            string recup = "ADD";
            bool messageRecup = false;

            int[] message = new int[nombreMessage];

            for (int i = 0; i < nombreMessage; i++)
            {
                message[i] = i + 1;
            }

        label2:

            foreach (int i in message)
            {
                if (userinput == i.ToString())
                {
                    messageRecup = true;
                    Afficherinfo(socketClient, i, 0);
                }
            }

            if (userinput == retour.ToString())
            {
                GestionMessage(socketClient);
            }
            else if (userinput == all)
            {
                foreach (int i in message)
                {
                    Afficherinfo(socketClient, i, 3);
                }
                GestionInformations(socketClient);
            }

            else if (userinput == liste)
            {
                CommandLIST(socketClient);
                GestionInformations(socketClient);
            }

            else if (userinput == suppr)
            {
                GestionSupprMessage(socketClient);
            }

            else if (userinput == recup)
            {
                GestionLectureMessage(socketClient);
            }

            else if (userinput == quit)
            {
                socketClient.Close();
                Console.WriteLine("\nFin du client -> Taper une touche pour terminer");
            }

            while (userinput != quit && userinput != suppr && userinput != recup && userinput != liste && userinput != all && userinput != retour.ToString() && !messageRecup)
            {
                Console.WriteLine("\nCe message n'existe pas ! Réessayez.");
                userinput = Console.ReadLine();
                goto label2;
            }
        }

        static void Afficherinfo(TcpClient socketClient, int i, int type)
        {
            StreamWriter sw;
            StreamReader sr;
            sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
            sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
            sw.AutoFlush = true;

            string ligne, tampon, text, text_sujet, text_date;

            tampon = "TOP " + i.ToString() + " 1";
            EcrireLigne(sw, tampon);
            LireLigne(sr, out ligne);
            text = "";
            text_sujet = text;
            text_date = text;

            Console.WriteLine("\nMessage n°" + i.ToString() + " :\n");

            while (ligne != ".")
            {
                if (ligne.StartsWith("From:"))
                {
                    text = ligne;
                }
                else if (ligne.StartsWith("Subject:"))
                {
                    text_sujet = ligne;
                }
                else if(ligne.StartsWith("Date:"))
                {
                    text_date = ligne;
                }
                LireLigne(sr, out ligne);
            }

            string[] decoup = text.Split(' ');
            string[] decoup_sujet = text_sujet.Split(' ');
            string[] decoup_date = text_date.Split(' ');

            string sujet = "";
            string expediteur = sujet;
            string date = sujet;
            bool fin = false;

            foreach (string s in decoup_sujet)
            {
                if (s != "Subject:")
                {
                    sujet += s + " ";
                }
            }

            foreach (string s in decoup)
            {
                if (s != "From:")
                {
                    expediteur += s + " ";
                }
            }

            foreach (string s in decoup_date)
            {
                if (s.StartsWith("+"))
                {
                    fin = true;
                }
                else if (s != "Date:" && !fin )
                {
                    date += s + " ";
                }
            }

            if (sujet == " ")
            {
                sujet = "<Sans Objet>";
            }
            Console.WriteLine("Expediteur: " + expediteur);
            Console.WriteLine("Date de l'envoi : " + date);
            Console.WriteLine("Objet : " + sujet);

            switch (type)
            {
                case 0:
                    GestionInformations(socketClient);
                    break;
                case 1:
                    GestionLectureMessage(socketClient);
                    break;
                case 2:
                    GestionSupprMessage(socketClient);
                    break;
                case 3:
                    break;
            }
        }

        /*** Retourne le nombre de message ***/
        static int nbMessage(TcpClient socketClient)
        {
            StreamWriter sw;
            StreamReader sr;
            sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
            sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
            sw.AutoFlush = true;

            string ligne, tampon;

            tampon = "STAT";
            EcrireLigne(sw, tampon);

            LireLigne(sr, out ligne);
            string[] lesValeurs = ligne.Split(' ');
            int nombre_de_messages = Int32.Parse(lesValeurs[1]);
            return nombre_de_messages;
        }
    }
}


