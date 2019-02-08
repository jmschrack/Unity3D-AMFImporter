# Unity3D-AMFImporter
Allows you to import Adjutant Model Files (AMF aka Halo BSPs ) natively into Unity3D.

-Includes Adjutant/MultiBlend shader for handling blend maps.

#Limitations
It's only as good as the AMF file is. If Adjutant didn't properly export, it won't properly import.

- Best results are Halo 1 and Halo 3 maps. Adjutant has partial support for all the others.
- Adjutant only supports "Regular" and "Terrain" shaders. These are mapped to "Standard (Metallic)" and "Adjutant/MultiBlend".

#To Do
- Finish the "HaloRegular" to support the various setups found in maps. (i.e. Materials with 2 Details maps)
- Convert "HaloRegular" to an Uber style shader




