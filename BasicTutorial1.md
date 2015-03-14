# Your First Daggerfall Connect Application #

This tutorial will demonstrate how to:
  * Create a new project in Visual C# Express.
  * Import the Daggerfall Connect DLL.
  * Enumerate texture files from Daggerfall's ARENA2 folder.
  * Open each texture file and output a description.

## Getting Started ##

  1. Download and extract the latest version of Daggerfall Connect from [Downloads](http://code.google.com/p/daggerfallconnect/downloads/list).
  1. Create a new project in Visual C# Express using **File** -> **New Project** and select _"Console Application"_ as the project type. Name the project and click **OK**.
  1. Add a reference to the DaggerfallConnect.dll using **Project** -> **Add Reference**. Click the **Browse** tab and navigate to **DaggerfallConnect.dll** from your downloaded.
  1. Open **Program.cs** in **Solution Explorer** and replace all code with the block shown below.
  1. Set variable MyArena2Path to your Daggerfall installations ARENA2 folder. Daggerfall Connect is read only and will not modify your game files.
  1. Build and run your application pressing **Ctrl+F5**.

## Code ##

```
using System;
using DaggerfallConnect;

namespace Tutorial_BasicSeries_1
{
    /// <summary>
    /// This tutorial demonstrates basic enumeration of image files.
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

            // Output information about each file in library
            foreach (DFImageFile file in MyImageFileReader)
                Console.WriteLine("{0} - {1}", file.FileName, file.Description);
        }
    }
}
```

## Output ##

<pre>
...<br>
TEXTURE.484 - Light Thief<br>
TEXTURE.485 - Normal Mage<br>
TEXTURE.486 - Normal Mage<br>
TEXTURE.487 - Medium Fighter<br>
TEXTURE.488 - Medium Fighter<br>
TEXTURE.489 - Thief Mage<br>
TEXTURE.490 - Thief Mage<br>
TEXTURE.491 - Female heavy horse<br>
TEXTURE.492 - Male heavy horse<br>
TEXTURE.493 - Female medium horse<br>
TEXTURE.494 - Male medium horse<br>
TEXTURE.495 - Female light horse<br>
TEXTURE.500 - Rain Forest<br>
TEXTURE.501 - !Sub_Tropical<br>
TEXTURE.502 - Swamp<br>
TEXTURE.503 - Desert<br>
TEXTURE.504 - Temperate woodland<br>
TEXTURE.505 - Snow woodland<br>
TEXTURE.506 - Woodland Hills<br>
TEXTURE.507 - Snow woodland Hills<br>
TEXTURE.508 - Haunted woodland<br>
TEXTURE.509 - Snow haunted woodlan<br>
TEXTURE.510 - mountains<br>
TEXTURE.511 - Snow Mountains<br>
Press any key to continue . . .<br>
</pre>