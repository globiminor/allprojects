-Disassemble the PortableDeviceApi Interop assembly using the command:

ildasm Interop.PortableDeviceApiLib.dll /out:pdapi.il

-Open the IL in Notepad and search for the following string:

instance void GetDevices([in][out] string& marshal( lpwstr) pPnPDeviceIDs,

-Replace all instances of the string above with the following string:

instance void GetDevices([in][out] string[] marshal( lpwstr[]) pPnPDeviceIDs,

-Save the IL and reassemble the interop using the command:

ilasm pdapi.il /dll /output=Interop.PortableDeviceApiLib.dll


Bemerkung: Nach einem Rebuild ist wieder die ungünstige dll vorhanden. 
Das führt zu einem Kompilierungsfehler bei     devMgr.GetDevices(devices, ref nDev); und ist deshalb leicht zu erkennen.
 