# Working With Texture Files #

This tutorial will show you how to:

  * Open a texture file.
  * Read basic properties of a texture file.
  * Loop through all records and frames of a texture file.
  * Acquire a managed bitmap from a texture file.


## Getting Started ##

  1. Check [BasicTutorial1](BasicTutorial1.md) for steps on creating a project and adding a reference to DaggerfallConnect.dll.
  1. Add another reference to **System.Drawing** using **Project** -> **Add Reference**. Click the **.NET** tab and select **System.Drawing** before clicking **OK**.
  1. Open **Program.cs** in the **Solution Explorer** and replace all code with the block shown below.
  1. Set variable MyArena2Path to your Daggerfall installation's ARENA2 folder.
  1. Build and run your program by pressing **Ctrl+F5**.

## Code ##

```
using System;
using System.Drawing;
using DaggerfallConnect;

namespace Tutorial_BasicSeries_2
{
    /// <summary>
    /// This tutorial demonstrates how to work with a single image file.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Specify Arena2 path of local Daggerfall installation
            string MyArena2Path = "C:\\dosgames\\DAGGER\\ARENA2";

            // Instantiate ImageFileReader
            ImageFileReader MyImageFileReader = new ImageFileReader(MyArena2Path);

            // Set desired library type
            MyImageFileReader.LibraryType = LibraryTypes.Texture;

            // Load TEXTURE.285 file
            MyImageFileReader.LoadFile("TEXTURE.285");
            
            // Output some information about this file
            Console.WriteLine("Image file {0} has {1} records",
                MyImageFileReader.FileName,
                MyImageFileReader.RecordCount);

            // Get image file to work with
            DFImageFile imageFile = MyImageFileReader.ImageFile;

            // Loop through all records
            for (int r = 0; r < imageFile.RecordCount; r++)
            {
                // Output some information about this record
                int frameCount = imageFile.GetFrameCount(r);
                Size sz = imageFile.GetSize(r);
                Console.WriteLine("Record {0} has {1} frames and is {2}x{3} pixels",
                    r, frameCount,
                    sz.Width,
                    sz.Height);

                // Get first frame of record as a managed bitmap
                Bitmap managedBitmap = imageFile.GetManagedBitmap(r, 0, true, false);
            }
        }
    }
}
```

## Output ##

<pre>
Image file TEXTURE.285 has 20 records<br>
Record 0 has 4 frames and is 63x115 pixels<br>
Record 1 has 4 frames and is 65x113 pixels<br>
Record 2 has 4 frames and is 68x111 pixels<br>
Record 3 has 4 frames and is 61x110 pixels<br>
Record 4 has 4 frames and is 60x108 pixels<br>
Record 5 has 6 frames and is 81x113 pixels<br>
Record 6 has 5 frames and is 78x111 pixels<br>
Record 7 has 6 frames and is 72x111 pixels<br>
Record 8 has 6 frames and is 69x112 pixels<br>
Record 9 has 6 frames and is 79x111 pixels<br>
Record 10 has 1 frames and is 80x114 pixels<br>
Record 11 has 1 frames and is 78x115 pixels<br>
Record 12 has 1 frames and is 89x118 pixels<br>
Record 13 has 1 frames and is 81x118 pixels<br>
Record 14 has 1 frames and is 79x115 pixels<br>
Record 15 has 1 frames and is 53x112 pixels<br>
Record 16 has 1 frames and is 62x111 pixels<br>
Record 17 has 1 frames and is 53x109 pixels<br>
Record 18 has 1 frames and is 36x110 pixels<br>
Record 19 has 1 frames and is 52x109 pixels<br>
Press any key to continue . . .<br>
</pre>