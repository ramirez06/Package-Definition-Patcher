using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PackageDefinitionPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            string app_path = Directory.GetParent(Environment.GetCommandLineArgs()[0]).FullName;
            bool packagedef_exist = File.Exists(app_path + @"\packagedefinition.txt");
            bool already_exist_bak = File.Exists(app_path + @"\packagedefinition.bak");

            byte[] current_pdef = new byte[] { };

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("PackageDefinition Patcher made by ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("The Big Shell\n");
            Console.ForegroundColor = ConsoleColor.White;

            Thread.Sleep(750);

            if (packagedef_exist)
            {
                Log("Loading packagedefinition...");
                current_pdef = File.ReadAllBytes(app_path + @"\packagedefinition.txt");
                Log("Done");
                Thread.Sleep(750);

                if (!already_exist_bak)
                {
                    Log("Creating a backup...");
                    File.Move(app_path + @"\packagedefinition.txt", app_path + @"\packagedefinition.bak");
                    Log("Done");

                    Thread.Sleep(750);
                }
                else
                {
                    Log("There is already a backup");
                    Thread.Sleep(750);
                }

                Log("Patching...");
                string temp_string = Encoding.Default.GetString(TBS_XTEA.XTEA_GetDecryptedBytes(current_pdef));
                temp_string = Regex.Replace(temp_string, "patchlevel=.*", "patchlevel=10000");
                current_pdef = TBS_XTEA.XTEA_GetEncryptedBytes(Encoding.Default.GetBytes(temp_string));
                Log("Done\nWriting...");
                Thread.Sleep(750);
                File.WriteAllBytes(app_path + @"\packagedefinition.txt", current_pdef);
                Log("Done\nClosing...");
            }

            Thread.Sleep(750);
        }

        static void Log(string Message, ConsoleColor Color = ConsoleColor.White)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(Message);
        }
    }
}
