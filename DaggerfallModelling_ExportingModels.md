_Alpha Version 0.6.8_

# Exporting Models #

In Daggerfall Modelling 0.6.8 and up it is possible to export models to Collada .dae format. Textures are exported to a selection of image formats.

Following is a brief tutorial on exporting a model from a location and opening it in Blender.

## 1. Finding Somewhere Interesting ##

In Daggerfall Modelling type "direnni" into the search field and hit Enter. This will locate Direnni Tower on the Isle of Balfiera. Expand out the search results and select **Direnni Tower**. This will load the location as shown.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial1.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial1.png)

## 2. Switch To Exterior Mode ##

As Direnni Tower is primarily a dungeon location, Daggerfall Modelling will open it in Dungeon Mode. Switch to the exterior of this dungeon by clicking the **Exterior Mode** button.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial2.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial2.png)

## 3. Switch To Free Camera ##

We now have a top-down view of the outside of Direnni Tower. This is nice, but it would be nicer if we could fly around the tower. Switch to **Free Camera** by clicking the toolbar button highlighted below.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial3.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial3.png)

Check the controls in [Location View](DaggerfallModelling_LocationView.md) to learn how to fly around the scene. Go ahead and explore a little before proceeding.

## 4. Selecting A Model ##

The only model of interest is Direnni Tower itself. When you move the mouse over the tower, it is outlined in gold wireframe as shown. If you double-click a highlighted model, the selected model will be opened in Model View. It is possible to select any model you can touch with the mouse in Block and Location scenes.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial4.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial4.png)

## 5. Model View ##

After double-clicking the highlighted model, you will find yourself in [Model View](DaggerfallModelling_ModelView.md). This view is for exploring an individual model in detail. You will notice that faces are highlighted as you move the mouse over the model. This will eventually be used to query face properties such as the texture used by that face.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial5.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial5.png)

## 6. Export The Model ##

With a model loaded into the Model View, we can export this model to Collada format. This is true of any model loaded into this view, whether you searched for it, selected it from a thumbnail, or picked it from a city or dungeon.

To export the model, click the Export Model button on the toolbar.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial6.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial6.png)

## 7. Modify Settings ##

There are a few settings that affect how the model is exported.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial7.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial7.png)

  * **Output Path** is the destination folder where Collada files are placed. Set this to whatever path you want to export models into. The model will be automatically named based on ModelID.
  * **Orientation** is the axis your destination modelling package or 3D engine considers to be "up". This is set by way of a rotation on the exported model's scene node. As we're using Blender in this tutorial keep the default setting of Z\_UP.
  * **Image Format** is the destination bitmap format textures are saved as. Keep the default of PNG. Daggerfall Modelling will create an "images" subfolder beneath your Output Path into which all textures are saved.

Once you've setup your preferences, click **OK** to export the model. Daggerfall Modelling will remember your preferences between sessions so you only need to set these once.

## 8. Open In Blender ##

Now that we've exported a model, let's open it up in Blender. This is done with the following process.

  1. Open Blender. Clear the default cube model by hitting **DEL**.
  1. Import your exported model by clicking **File** -> **Import** -> **COLLADA (.dae)** on the menu.
  1. Browse to your export folder and open "500.dae". The number "500" is the ModelID for Direnni Tower's exterior. Every model has a unique numeric ID. You can rename this file to whatever you like.
  1. This is a big model so we need to adjust the clip distance. Hit **N** to open view properties (or **View** -> **Properties**) and change **clip end** to **100m**. Close view properties by hitting **N** again.
  1. Scroll out by rolling the mouse wheel down until the entire model is in view. You can rotate the model around while holding down the middle mouse button.
  1. Turn on texturing by hitting **Alt+Z**.

Congratulations! You have now exported a model from Daggerfall and imported it into a modelling package to work with. Try exploring the world further with Daggerfall Modelling and finding other interesting models to export.

![http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial8.png](http://www.dfworkshop.net/wp-content/uploads/2011/04/ExportTutorial8.png)