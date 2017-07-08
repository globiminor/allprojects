
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OcadTest.OEvent
{
  internal class FrontRueck { public string Front; public string Rueck; }
}

[TestClass]
public class DeviceTest
{
  [StructLayout(LayoutKind.Explicit, Size = 16)]
  public struct PropVariant
  {
    [FieldOffset(0)]
    public short variantType;
    [FieldOffset(8)]
    public IntPtr pointerValue;
    [FieldOffset(8)]
    public byte byteValue;
    [FieldOffset(8)]
    public long longValue;
  }
  [TestMethod]
  public void CanGetDevices()
  {
    PortableDeviceApiLib.PortableDeviceManager devMgr = new PortableDeviceApiLib.PortableDeviceManager();
    PortableDeviceApiLib.IPortableDeviceManager iDevMgr = devMgr;
    uint nDev = 0;
    devMgr.RefreshDeviceList();

    devMgr.GetDevices(null, ref nDev);
    string[] devices = new string[nDev];
    devMgr.GetDevices(devices, ref nDev);


    for (int i = 0; i < nDev; i++)
    {
      uint maxChar = 1024;
      ushort[] t = new ushort[maxChar];

      uint nChar = maxChar;
      devMgr.GetDeviceFriendlyName(devices[i], ref t[0], ref nChar);
      string device = GetString(t, nChar);

      nChar = maxChar;
      devMgr.GetDeviceManufacturer(devices[i], ref t[0], ref nChar);
      string manufactor = GetString(t, nChar);

      nChar = maxChar;
      devMgr.GetDeviceDescription(devices[i], ref t[0], ref nChar);
      string descr = GetString(t, nChar);

      PortableDeviceApiLib.IPortableDeviceValues devVals = InitDeviceValues();
      PortableDeviceApiLib.PortableDeviceFTM dev = new PortableDeviceApiLib.PortableDeviceFTM();

      dev.Open(devices[i], devVals);

      PortableDeviceApiLib.IPortableDeviceContent content;
      dev.Content(out content);

      EnumRecursive(content, "DEVICE");
    }

    System.Runtime.InteropServices.Marshal.ReleaseComObject(devMgr);
  }

  private void EnumRecursive(PortableDeviceApiLib.IPortableDeviceContent content, string parentId)
  {
    PortableDeviceApiLib.IEnumPortableDeviceObjectIDs enumObjIds;
    content.EnumObjects(0, parentId, null, out enumObjIds);

    List<string> ids = new List<string>();
    string objId;
    uint fetched = 1;
    while (fetched > 0)
    {
      enumObjIds.Next(1, out objId, ref fetched);
      if (fetched > 0)
      { ids.Add(objId); }
    }

    foreach (string id in ids)
    {
      PortableDeviceApiLib.IPortableDeviceProperties props;
      content.Properties(out props);

      PortableDeviceApiLib.IPortableDeviceValues values;
      props.GetValues(id, null, out values);

      uint nValues = 0;
      values.GetCount(ref nValues);
      for (uint iValue = 0; iValue < nValues; iValue++)
      {
        PortableDeviceApiLib._tagpropertykey propKey =
                new PortableDeviceApiLib._tagpropertykey();
        PortableDeviceApiLib.tag_inner_PROPVARIANT ipValue =
                        new PortableDeviceApiLib.tag_inner_PROPVARIANT();
        values.GetAt(iValue, ref propKey, ref ipValue);

        //
        // Allocate memory for the intermediate marshalled object
        // and marshal it as a pointer
        //
        IntPtr ptrValue = Marshal.AllocHGlobal(Marshal.SizeOf(ipValue));
        Marshal.StructureToPtr(ipValue, ptrValue, false);

        //
        // Marshal the pointer into our C# object
        //
        PropVariant pvValue =
            (PropVariant)Marshal.PtrToStructure(ptrValue, typeof(PropVariant));

        //
        // Display the property if it a string (VT_LPWSTR is decimal 31)
        //
        if (pvValue.variantType == 31 /*VT_LPWSTR*/)
        {
          string value = Marshal.PtrToStringUni(pvValue.pointerValue);
          Console.WriteLine("{0}: Value is \"{1}\"",
              (iValue + 1).ToString(), value);
        }
        else
        {
          Console.WriteLine("{0}: Vartype is {1}",
              (iValue + 1).ToString(), pvValue.variantType.ToString());
        }
        //PropVariant pv = new PropVariant(ip);

      }

      EnumRecursive(content, id);
    }
  }

  private PortableDeviceApiLib.IPortableDeviceValues InitDeviceValues()
  {
    // We'll use an IPortableDeviceValues object to transform the
    // string into a PROPVARIANT
    PortableDeviceApiLib.IPortableDeviceValues pValues =
        (PortableDeviceApiLib.IPortableDeviceValues)
            new PortableDeviceTypesLib.PortableDeviceValues();

    return pValues;

    //// We insert the string value into the IPortableDeviceValues object
    //// using the SetStringValue method
    //pValues.SetStringValue(ref PortableDevicePKeys.WPD_OBJECT_ID, value);

    //// We then extract the string into a PROPVARIANT by using the 
    //// GetValue method
    //pValues.GetValue(ref PortableDevicePKeys.WPD_OBJECT_ID,
    //                            out propvarValue);

  }

  private string GetString(ushort[] t, uint count)
  {
    char[] charArray = new char[count - 1];
    System.Array.Copy(t, charArray, count - 1);
    string s = new string(charArray);
    return s;
  }
}