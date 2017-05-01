Installing OcadScratch.exe on Windows Device
--------------------------------------------
1. Copy executables of the folder bin (needed: OcadScratch.exe, Basics.dll, Macro.exe, Ocad.dll, Stream.dll) to any folder.

Prepare a O-Scratch Project
---------------------------
Recommended workflow

1. Create a copy (<copy>.ocd) of your ocad-File (<orig>.ocd) you want to correct. This <copy>.ocd will be used to store o-scratch elements.
2. Delete all elements in the <copy>.ocd.
   and close <copy>.ocd.
3. Add the <copy>.ocd as first background-Layer in the <orig>.ocd.

4. Copy <project>.config, <scratch.xml> and <symobls.xml> from your android device to a the <copy>.ocd-folder.
5. Start OcadScratch.exe
6. "Load" <project>.config. All elements of <scratch.xml> will be displayed in the list.
7. "Transfer" data to <copy>.ocd. 
   Remark: Make sure <copy>.ocd is not opened in OCAD. It must only exist as background layer in the opened OCAD-Session, or not at all.
   Executed actions: 
   - If <copy>.orig.ocd exists, <copy>.orig.ocd is copied to <copy>.ocd.
   - If <copy>.orig.ocd does not exist, <copy>.ocd is copied to <copy>.orig.ocd.
   - The elements of the list are integrated as grafics objects to <copy>.ocd.
8. Open <orig>.ocd (with OCAD) and make background-layer <copy>.ocd visible.
   Remark: If <orig>.ocd is already open, toggle (twice) the visibilty of the background-layer <copy>.ocd to see the current state of <copy>.ocd.

Navigation
----------
a) In OcadScratch: Select any element in the list. It get's displayed separatly.
b) "Move to" centers the extent of OCAD to the center of the element
c) "Move to next": The current element is marked as handled, the closest unhandled element is selected and "Move to" is executed.
d) In OCAD: edit the <orig>.ocd as needed.

