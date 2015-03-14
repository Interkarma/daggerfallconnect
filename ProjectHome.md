# Daggerfall Connect #

### Summary ###

Daggerfall Connect is a class library for reading Daggerfall’s native file formats and converting to standard managed types. For example, you can export Daggerfall’s image formats to a managed Bitmap object without needing to understand the underlying file structures. More complex types like 3D models and map layouts are returned as type-safe structures. The addition of helper classes further assist with extracting more complex data from Daggerfall’s files.

### Features ###
  * Developed 100% in C# for the .NET Framework 2.0.
  * Also works with higher versions of .NET such as 3.5 and 4.0.
  * Easy to get started and reduces complexity of working with Daggerfall’s file formats.
  * Several tutorials and example projects.
  * Nearly complete support for the following Daggerfall files: TEXTURE, .IMG, .CIF, .RCI, SKY, CLIMATE.PAK, POLITIC.PAK, ARCH3D.BSA, BLOCKS.BSA, MAPS.BSA, WOODS.WLD, DAGGER.SND.
  * Helper classes like ImageFileReader create a simple and unified way to read Daggerfall's files.