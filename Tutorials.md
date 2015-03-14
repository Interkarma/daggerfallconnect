# Tutorials #

There are several tutorials to help you get started with Daggerfall Connect. These assume an understanding of C# but are hopefully easy enough to follow if you're still learning.

You should also have a basic idea of the types of files in Daggerfall's ARENA2 folder, such as the various image files and content archives. You don't need to understand how to read each file type, that's what Daggerfall Connect is for, but it does help to know what each file contains and how they work together. A great resource for this information can be found in the [Daggerfall Hacking Guide](http://www.uesp.net/wiki/Daggerfall:Hacking_Guide) on UESP.

## Basic Series ##

These tutorials introduce you to the fundamentals of creating a project, importing the DaggerfallConnect.dll, and working with basic files in the ARENA2 folder.

#### BasicTutorial1 ####
Explains how to create a new project and enumerate an image library (collection of files of a specific image type).

#### BasicTutorial2 ####
Demonstrates opening a texture archive (collection of images in a single file) and reading each record and frame. It also shows how to get a Daggerfall texture as a managed bitmap.

#### BasicTutorial3 ####
Introduces BSA files. This is a common content archive used in ARENA2 files. BSA files contain information such as 3D models and map layouts. Many of the higher-level Daggerfall Connect classes work with BSA files.