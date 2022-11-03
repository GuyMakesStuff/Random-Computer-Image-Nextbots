using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Experiments.Global.IO
{
    // Utilities For Saving And Loading Save Files
    // From The Computer Using Binary.
    public static class Saver
    {
        // The Format For The Saved Files.
        static string Format
        {
            get
            {
                return "sav";
            }
        }

        // Function For Saving A SaveFile Class Into A Binary
        // File On The Computer.
        public static void Save(SaveFile file)
        {
            // Format The Files Path
            string Path = GetPath(file);
            // Create A Stream For Creating The File At The File Path.
            FileStream stream = new FileStream(Path, FileMode.Create);
            // Create A Formatter Class For Formatting The SaveFile Class Into Binary
            // And Saving That Into A File On The Computer.
            BinaryFormatter formatter = new BinaryFormatter();
            // Convert The SaveFile To Binary Data And Save That To A File On
            // The Computer At The File Path.
            formatter.Serialize(stream, file);
            // Close The Stream.
            stream.Close();
        }
        // Function For Loading A SaveFile Class From A Binary
        // File On The Computer
        public static SaveFile Load(SaveFile file)
        {
            // Format The Files Path.
            string Path = GetPath(file);
            // If The File Does Not Exist, Return Null.
            if(!File.Exists(Path))
            {
                Debug.Log("File Not Found At-" + Path);
                return null;
            }

            // Create A Stream For Opening The File At The File Path.
            FileStream stream = new FileStream(Path, FileMode.Open);
            // Create A Formatter Class For Converting The Binary Data From The File
            // Into A SaveFile Class.
            BinaryFormatter formatter = new BinaryFormatter();
            // Load Binary Data From A File On The Computer At The File Path,
            // And Convert That Into A SaveFile Class.
            SaveFile LoadedData = formatter.Deserialize(stream) as SaveFile;
            // Close The Stream.
            stream.Close();
            // Output The Converted SaveFile Class.
            return LoadedData;
        }
        // Format A Path On The Computer For A SaveFile Class
        // Using A Persistent Path.
        static string GetPath(SaveFile file)
        {
            return Application.persistentDataPath + "/" + file.FileName + "." + Format;
        }
    }
}