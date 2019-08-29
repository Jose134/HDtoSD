using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HDtoSD
{
    class Program
    {
        //Default replacing variable
        private const bool DEFAULT_REPLACING = false;

        //File search filters for file extensions
        private static string[] filters = new string[] { "jpg", "png" };

        //Rainbow mode variables
        private static bool coloring = false;

        static void Main(string[] args)
        {
            bool replace = DEFAULT_REPLACING;
            ConsoleColor startColor = Console.ForegroundColor;

            //Handles arguments
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    //Checks the current parameter
                    if (args[i] == "-r" || args[i] == "--replace")
                    {
                        //Tries to parse the given parameter to a boolean value
                        if (!bool.TryParse(args[i+1], out replace))
                        {
                            //Error handling
                            Console.WriteLine("ERROR: Couldn't set replace parameter, using default option");
                            replace = DEFAULT_REPLACING;
                        }
                        i++;
                    }
                    else if (args[i] == "-c" || args[i] == "--color")
                    {
                        coloring = true;
                    }
                    else if (args[i] == "-h" || args[i] == "--help")
                    {
                        DisplayHelp();
                        return;
                    }
                    else
                    {
                        //Error handling
                        Console.WriteLine("ERROR: Unkown parameter " + args[i]);
                        return;
                    }
                }
            }

            //Gets the path where the executable file is
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            path = path.Substring(6) + @"\";

            //Gets all the files that we have to convert
            var files = GetFilesFrom(path, filters, replace);
            
            //Displays information
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Replacing: " + replace);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(files.Length + " files will be generated");
            Console.WriteLine("\nPress C to cancel or any other key to continue");

            //Checks user input
            if (Console.ReadKey().Key == ConsoleKey.C)
            {
                //Stops the program
                return;
            }
            else
            {
                Array colors = ConsoleColor.GetValues(typeof(ConsoleColor));
                int color = 0;
                   
                //Resizes the images
                foreach(string file in files)
                {
                    //Changes the color if rainbow mode is on
                    if (coloring)
                    {
                        Console.ForegroundColor = (ConsoleColor)colors.GetValue(color % 16);
                        color++;
                    }
                    
                    //Outputs and resizes the file
                    Console.WriteLine(file);
                    Resize(file, file.Substring(0, file.Length - 7) + file.Substring(file.Length - 4), 0.5f);
                }
            }

            //Finishes the program
            Console.ForegroundColor = startColor;
            Console.WriteLine("\n\nFile generation finished, press any key to exit the program");
            Console.ReadKey();
        }

        //Returns an array containing the HD image files in the skin folder
        private static string[] GetFilesFrom(string searchFolder, string[] filters, bool replace)
        {
            //Get all files in the folder
            List<string> filesFound = new List<string>();
            var searchOption = SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), searchOption));
            }

            //Finds the HD images between all the found files
            List<string> hdFiles = new List<string>();
            for (int i = 0; i < filesFound.Count; i++)
            {
                var compare = filesFound[i].Substring(filesFound[i].Length - 7, 3);
                var no2x = filesFound[i].Substring(0, filesFound[i].Length - 7) + filesFound[i].Substring(filesFound[i].Length - 4, 4);

                //Checks if it's an HD file
                if (compare.Equals("@2x"))
                {
                    //Checks if there's an SD version if we don't want to replace the already existing ones
                    if (!replace)
                    {
                        bool sd = FindFile(filesFound, no2x);
                        if (sd)
                        {
                            //Skips the current for iteration
                            continue;
                        }
                    }

                    hdFiles.Add(filesFound[i]);
                }
            }

            //Returns the hdFiles list in array form
            return hdFiles.ToArray();
        }

        //Checks if a given file is contained in a given list
        private static bool FindFile (List<string> files, string search)
        {
            //Checks each file
            foreach (string f in files)
            {
                if (f == search)
                {
                    return true;
                }
            }

            return false;
        }
        
        //Resizes a image given its path
        private static void Resize (string inputPath, string outputFile, float scale)
        {
            using (var srcImage = Image.FromFile(inputPath))
            {
                //Checks if the image will have a valid resolution after resizing
                if (srcImage.Width * scale >= 1 && srcImage.Height * scale >= 1) {

                    var newWidth = (int)(srcImage.Width * scale);
                    var newHeight = (int)(srcImage.Height * scale);
                    using (var newImage = new Bitmap(newWidth, newHeight))
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        //Sets quality parameters
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        //Resizes the image
                        graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));

                        //Saves the resized image
                        newImage.Save(outputFile);
                    }

                }
                else
                {
                    //If the image is a 1x1 it will just do a copy for the SD version
                    srcImage.Save(outputFile);
                }
            }
        }

        private static void DisplayHelp ()
        {
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("osu! HD skin to SD converter");
            Console.WriteLine("Code written by DarkBird (https://osu.ppy.sh/users/8042593)");
            Console.WriteLine("Usage: HDtoSD.exe [parameters]");
            Console.WriteLine("\n\n\nOptional parameters:");
            Console.WriteLine("-r   --replace + true/false     If set to true the already existing SD images will be overwritten");
            Console.WriteLine("-c   --color                    Activates rainbow mode (just for fun)");
            Console.WriteLine("-h   --help                     Displays help information");
            Console.WriteLine("\n\n-----------------------------------------");
        }
    }
}
