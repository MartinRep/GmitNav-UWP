# GmitNav-UWP
GMIT Navigation system. Universal Windows Project 

## Description

Application to help locate Room by room number inside Galway Mayo Institute of Technology Dublin road campus building complex. This project was developed for Mobile application development module.


## Technology
- [Bing](https://www.microsoft.com/en-us/maps/documentation) maps API
- [Json.Net](https://www.newtonsoft.com/json) NewtonSoft
- [Neo4j](https://neo4j.com/) Graph Database

## Hosting
Neo4j database is hosted on [GrapeneDb](https://www.graphenedb.com/). Due to number of nodes nad relationships database falls into GrapheneDb HOBBY pricing package. This is a **FREE** tier package. Currently limited to 1K nodes and 10K relationships. This makes this project fully funtional with no expenses for running the service.

## Usage

 - Application starts with placing map overlays on top of the Bing map, this is View zoom sensitive and can cause misplacement of image overlay if Zoom level of the view is disturbed.
 - Rooms details are retreived from server
 - Controls are displayed once the map is configured.
 - By typing the room number and pressing Enter or Clicking on Search button. Location of the room with correct level overlay is displayed and map is centered on that location.
 - Changing the level hides the icon.
 - Escape key deletes the icon.
 
 
 ![Demo](https://github.com/MartinRep/GmitNav-UWP/blob/master/GmitNavUWP/GitAssets/GmitNavUwpDemo.gif)
 
 
 ## Future development
 
 - Full navigation from room to room. Most functionality is done, but not enabled yet due to database creation. Over 576 Rooms has to be mapped to Corridors, stairs, etc...
 - User location inside building detection. Due to restricted usage of GPS system inside the building, [IndorAtlas](https://app.indooratlas.com/) was chosen as alternative. Bulding has to be mapped to EM noise.
