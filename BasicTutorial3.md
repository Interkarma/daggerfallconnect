# BSA Basics #

This tutorial will show you how to:

  * Open a BSA file.
  * Read basic properties of a BSA file.
  * Get a BSA record as a byte array.

## Getting Started ##

  1. Check [BasicTutorial1](BasicTutorial1.md) for steps on creating a project and adding a reference to DaggerfallConnect.dll.
  1. Open Program.cs in the **Solution Explorer** and replace all the code with the block shown below.
  1. Set variable MyArena2Path to your Daggerfall installation's ARENA2 folder.
  1. Build and run your program by pressing **Ctrl+F5**.

## Code ##

```
using System;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;

namespace Tutorial_BasicSeries_3
{
    /// <summary>
    /// This tutorial demonstrates how to open a BSA file and read records.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Specify Arena2 path of local Daggerfall installation
            string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

            // Path to BSA file
            string FilePath = Path.Combine(MyArena2Path, "BLOCKS.BSA");

            // Open BSA file
            BsaFile bsaFile = new BsaFile(
                FilePath,
                FileUsage.UseDisk,
                true);

            // Output some information about the file
            Console.WriteLine("BSA file {0} has {1} records",
                Path.GetFileName(FilePath),
                bsaFile.Count);

            // Output some information about a record
            int record = 0;
            int length = bsaFile.GetRecordLength(record);
            string name = bsaFile.GetRecordName(record);
            Console.WriteLine("Record {0} is {1} bytes long and named {2}",
                record,
                length,
                name);

            // Get the record data
            byte[] data = bsaFile.GetRecordBytes(record);

            // This byte array now contains the binary data for this record
            // You can query it using streams to read in whatever data you like
            // (i.e. work with the file formats directly)
            // Here the array is just ignored and will pass out of scope
            // before being garbage collected.
        }
    }
}
```

## Output ##

<pre>
BSA file BLOCKS.BSA has 1295 records<br>
Record 0 is 7391 bytes long and named WALLAA03.RMB<br>
Press any key to continue . . .<br>
</pre>

## Further Information ##

The BsaFile class can be used if you want to get BSA records directly for any reason. Normally, you would use higher-level classes like Arch3dFile and BlocksFile to handle these file formats.

The BsaFile class could be used as a starting point for writing your own format readers, or working with data in a way not provided by DaggerfallConnect.