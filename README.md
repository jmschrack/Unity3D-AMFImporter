<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/jmschrack/Unity3D-AMFImporter/">
  </a>

  <h3 align="center">Unity3D-AMFImporter</h3>

  <p align="center">
    Allows you to import Adjutant Model Files (AMF aka Halo BSPs ) natively into Unity3D!
  </p>
</p>



<!-- TABLE OF CONTENTS -->
## Table of Contents

* [About the Project](#about-the-project)
* [Getting Started](#getting-started)
* [Warning](#warning)
* [Troubleshooting](#troubleshooting)
* [Contact](#contact)
* [Acknowledgements](#acknowledgements)



<!-- ABOUT THE PROJECT -->
## About The Project
Allows you to import Adjutant Model Files (AMF aka Halo BSPs ) natively into Unity3D.
* Includes Adjutant/MultiBlend shader for handling blend maps.
* Supported importing rigged meshes

## Warning
It's only as good as the AMF file is. If Adjutant didn't properly export, it won't properly import.
* Best results are Halo 1 and Halo 3 maps. Adjutant has partial support for all the others.
* Adjutant only supports "Regular" and "Terrain" shaders. These are mapped to "Standard (Metallic)" and "Adjutant/MultiBlend".


<!-- GETTING STARTED -->
## Getting Started

* Place the "AMFImporter" folder into your Unity Assets folder.
* (Optional but highly recommend) Create a Folder for the AMF you want to import. 
* Extract all AMF related textures into this folder, preservering the folder structure
* Lastly, add the AMF file to this folder. Unity will generate meshes and materials.

<!-- TROUBLESHOOTING -->
### Troubleshooting

* If textures look extremely dark and washed out on an object, go to the corresponding texture import settings and uncheck "sRGB." Many, if not all, Halo 3 textures and later were authored in linear color space.
* All 3D objects have a "AMF Material Helper" which contains a list of "Shader Settings." Clicking on an entry will show you the raw data extracted from the AMF source. 
* Currently, there is not a way to determine if a texture has an Alpha channel or not. You will have to change some of the generated Material's Smoothness settings yourself
* AMFImporter will attempt to find existing Materials in the local folder before creating new ones. So put all your AMF files in the same folder if you want to reuse materials.
* Some Materials may need to be set to Transparent. If so, change the Smoothness source to "Metal Alpha."




<!-- ROADMAP -->
## Roadmap

* Implement a hybrid Cook-Torrance BRDF shader as outlined in [Lighting and Materials of Halo3](https://developer.amd.com/wordpress/media/2013/01/Chapter01-Chen-Lighting_and_Material_of_Halo3.pdf)
* Finish the "HaloRegular" to support the various setups found in maps. (i.e. Fading emission maps, double detail maps)





<!-- CONTACT -->
## Contact

Jonathan Schrack - [@jmschrack](https://twitter.com/jmschrack)


<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements
Huge Thanks to Shelly and the CE Reclaimers discord for the help!
