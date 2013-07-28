Contained in this archieve are the as-of-yet created assets of the Hexagonal Canister (HexCan) resource pod system.

/ Root
	LICENSE.txt	-	CC-BY 3.0 License
	Readme.txt	-	This file

/HexCans	-	Copy this folder into /GameData/

/Model Basefiles- 	Original and raw files from various points in the development
	/CATIA	-	The original models as created in CATIA, should also load in Solidworks
	/STL	-	the STLs exported from CATIA to be loaded into Blender, or the poly modeller of your choice
	/Blend	-	The .blend files for each part, allowing for custom edits to the UV map or whatever really
	/Unity	-	The complete unity project and scene including all parts as they are configured for the stock sets 

/Texture Basefiles	Original and raw texture files cuz why not
	/	-	The completed generic textures
	/UVs	-	the PNG format UV maps as exported from Blender
	/GIMP Raws	project format .xcf files of the stock textures for use in The GIMP

It is the intent of the HexCan project to make a lightweight model/texture set for radially attachable
resource containers able to fit diverse needs while still looking kinda nice. It is being released under
CC-BY 3.0 license with the express intent that any person may take the models and apply customized
textures and cfgs to create a variant that fits their need, as well as being free to distribute the result.
We desire that any derivative works be released under the same license, but it is not mandatory.

--------------------------------------------------
Release History
5/15/2013	0.0.1	Initial Public Build
5/17	  	0.0.2	Finished most stock resources and textures
5/22	  	0.1.0	
					Updated to .20
					tweaked textures
					finished all stock resources

5/24		0.1.1	
					Changed collider
					fixed categories
					fixed resources

5/26		0.2.0	
					Removed 2/3rds of .mu and texture files
					added can rack and decoupler, as well as rescale of stock fuel... tube... thing

6/9		0.2.1	
					Increased space and contents of Liquid Barite canisters
					fixed LB descriptions
					fixed LB nodes
					Fixed all HexCan dryweights
					Corrected Xenon cfgs
					Changed file name of ASAS hexcan to indicate can size
					Altered resource contents based on new measurements

6/11		0.2.2
					Corrected Barite Normal tank
					Reduced weight of probes
					Corrected Oxy small mass
					Removed all drymass declarations 
					Verified all parts have mass declarations
					Verified that all parts weigh an appropriate amount

6/16		0.3.0			
					Converted all finished parts to use MODEL{} and load from unified models
					Advanced HexPod from raw STL to pre-textured Unfinished Part state